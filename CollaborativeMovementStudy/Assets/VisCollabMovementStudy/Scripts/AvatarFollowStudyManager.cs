using Photon.Pun;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public class AvatarFollowStudyManager : MonoBehaviour, IPunObservable
{
    public GameSettings gameSettings;
    public GameObject PlayerHead;
    public GameObject StudyOverHintsParent;
    [Space]
    public PhotonView photonView;
    public PhotonView PhotonViewScriptedAvatar;
    public ScriptedVRAvatar ScriptedAvatar;
    public List<StudyLookAtInteractable> Interactables = new List<StudyLookAtInteractable>();

    public float MovementWaitDelay = 1.5f;
    public int RoomLoggingIndex = 1;
    public int m_currentStageIndex = 0;
    public int CurrentStageIndex { get { return m_currentStageIndex; } }
    private bool m_firstStageBegun = false;
    private bool m_arrivedAtFirstInteractable = false;
    private bool m_processedAllStages = false;
    /// <summary>
    /// Should only be set to false for Demo Manager
    /// </summary>
    public bool DoPersistAndSendLogs = true;
    /// <summary>
    /// LogAnalysis Tool is a VR logging tool from another students master thesis.
    /// It is integrated for a first field test.
    /// It is completly seperate to the logging functions made for this study.
    /// </summary>
    public bool DoUseLogAnalysisTool = true;

    [Space]
    [SerializeField]
    private int m_currentRouteIndex = 0;
    public int CurrentRouteIndex { get { return m_currentRouteIndex; } }

    public List<ScriptedAvatarRoute> Routes = new List<ScriptedAvatarRoute>();

    [Space]
    public UnityEvent OnFirstPictureTriggered;
    public UnityEvent OnAllStagesProcessed;

    private bool m_hasUnappliedNetworkData = false;
    private int r_currentStage;
    private int r_currentRouteIndex;

    #region Reset Handling
    /// <summary>
    /// Sends out an Photon RPC. The player whos actor number matches will then request ownership and Reset the StudyManager.
    /// </summary>
    /// <param name="a_actorId"></param>
    /// <param name="a_routeIndex"></param>
    public void AssignPlayerStudyManagerAndReset(int a_actorId, int a_routeIndex)
    {
        try
        {
            if (PhotonNetwork.IsConnectedAndReady)
            {
                photonView.RPC("RPC_OnBeingAssignedMngrWRst", RpcTarget.All, new object[] { a_actorId, a_routeIndex });
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("Error while starting RPC RPC_OnBeingAssignedMngrWRst: " + ex, this.gameObject);
        }
    }

    [PunRPC]
    public void RPC_OnBeingAssignedMngrWRst(int a_actorId, int a_routeIndex)
    {
        try
        {
            //Debug.Log($"RPC_OnBeingAssignedMngrWRst received. For Actor: {a_actorId} RouteIndex: {a_routeIndex}");

            if (PhotonNetwork.LocalPlayer != null && PhotonNetwork.LocalPlayer.ActorNumber == a_actorId)
            {
                //Debug.Log($"RPC_OnBeingAssignedMngrWRst. Request is for local client");
                RequestOwnershipAndReset(a_routeIndex);
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("Error while handling RPC_OnBeingAssignedMngrWRst: " + ex, this.gameObject);
        }
    }

    public void RequestOwnershipAndReset(int a_routeIndex)
    {
        RequestOwnershipOfAvatarAndInteractables();
        ChangeRouteIndexAndReset(a_routeIndex);
    }

    public void RequestOwnershipOfAvatarAndInteractables()
    {
        photonView.RequestOwnership();
        var photonViewAvatar = ScriptedAvatar.GetComponent<PhotonView>();
        if (photonViewAvatar != null)
        {
            photonViewAvatar.RequestOwnership();
        }
        else
        {
            Debug.LogWarning("Avatar has no PhotonViewToRequestOwnership", this.gameObject);
        }

        foreach (var i in Interactables)
        {
            var i_view = i.GetComponent<PhotonView>();
            if (i_view != null)
            {
                i_view.RequestOwnership();
            }
        }
    }

    public void ChangeRouteIndexAndReset(int a_routeIndex)
    {
        m_currentRouteIndex = a_routeIndex;
        ResetStudy();
    }

    public void ResetStudy()
    {
        m_currentStageIndex = 0;
        m_processedAllStages = false;
        m_firstStageBegun = false;
        m_arrivedAtFirstInteractable = false;
        if (StudyOverHintsParent != null)
            StudyOverHintsParent.SetActive(false);

        // reset interactables
        foreach (var interactable in Interactables)
        {
            interactable.ResetInteractable();
            interactable.EnableInteractions(false);
        }

        if (HasRouteForIndex(m_currentRouteIndex))
        {
            Routes[m_currentRouteIndex].StartInteractable.EnableInteractions(true);
            ScriptedAvatar.Orders = Routes[m_currentRouteIndex].CreateMovementOrderListFromStages(MovementWaitDelay);
        }
        else
        {
            ScriptedAvatar.Orders = new List<MovementOrder>();
        }

        ScriptedAvatar.ResetAvatar();


    }

    #endregion

    #region Study Event Handling
    private bool HasRouteForIndex(int a_index)
    {
        return a_index < Routes.Count;
        // length = 1 -> max index 0
        // length = 2 -> max index 1
    }

    /// <summary>
    ///  this is what does progress the study
    /// </summary>
    public void OnInteractableTriggered()
    {
        ScriptedAvatar.ProcessNextOrder();
        SwitchToNextStage();
    }

    public void OnPlayerFirstArrivedAtInteractable()
    {
        DateTime logFrameTime = DateTime.UtcNow;
        if (m_firstStageBegun)
        {
            var stage = Routes[m_currentRouteIndex].Stages[m_currentStageIndex];
            stage.MarkUserArrivedAtInteractable(logFrameTime, PlayerHead.transform);
            // -------stage_-1------- i0 -------stage_0------- i1 -------stage_1------- i2 -------stage_2------- i3
            LogArrivedFirstTimeAtTargetStandingArea(logFrameTime, m_currentStageIndex + 1, m_currentStageIndex, stage.StageTimeTookArrive, stage.maxdistanceToUserPathWhenfirstArrived);

            Debug.Log($"########## Arrived for Stage_idx: {m_currentStageIndex} time: {stage.StageTimeTookArrive} mxUpDist: {stage.MaxDistanceToUserPath}");
        }
        else
        {
            m_arrivedAtFirstInteractable = true;
            StartDetailedLogging();

            LogArrivedFirstTimeAtTargetStandingArea(logFrameTime, 0, -1, new TimeSpan(0), 0);
        }
    }

    private void SwitchToNextStage()
    {
        if (HasRouteForIndex(m_currentRouteIndex))
        {
            DateTime logFrameTime = DateTime.UtcNow;

            if ((m_currentStageIndex + 1) >= Routes[m_currentRouteIndex].Stages.Count)
            {
                // no more stages
                if (m_processedAllStages == false)
                {
                    Debug.Log("########## No more Stages left - Stopping Logging", this.gameObject);
                    m_processedAllStages = true;

                    var stage = Routes[m_currentRouteIndex].Stages[m_currentStageIndex];
                    stage.MarkUserActivatedInteractable(logFrameTime, PlayerHead.transform);
                    LogEventTriggeredInteractable(logFrameTime, m_currentStageIndex + 1, m_currentStageIndex, stage.StageTimeTookActivate, stage.maxdistanceToUserPathWhenTriggeredTarget);
                    StopDetailedLogging();

                    OnAllStagesProcessed.Invoke();
                    if (StudyOverHintsParent != null)
                        StudyOverHintsParent.SetActive(true);
                }
                return;
            }

            if (m_firstStageBegun)
            {
                // in stage 0+
                var stage = Routes[m_currentRouteIndex].Stages[m_currentStageIndex];
                stage.MarkUserActivatedInteractable(logFrameTime, PlayerHead.transform);

                LogEventTriggeredInteractable(logFrameTime, m_currentStageIndex + 1, m_currentStageIndex, stage.StageTimeTookActivate, stage.maxdistanceToUserPathWhenTriggeredTarget);
                m_currentStageIndex++;
            }
            else
            {
                // in stage -1
                Debug.Log($"########## First ineractable Triggered. Beginning firstStage.");
                m_firstStageBegun = true;

                LogEventTriggeredInteractable(logFrameTime, m_currentStageIndex, -1, new TimeSpan(0), 0);

            }
            Debug.Log("########## START Measurement of stageIdx: " + m_currentStageIndex);

            Routes[m_currentRouteIndex].Stages[m_currentStageIndex].ActivateStage(logFrameTime);
        }
        else
        {
            Debug.LogWarning($"Cannot switch to next stage as there is no route for current index {m_currentRouteIndex}", this.gameObject);
        }
    }
    #endregion

    #region PathDebugging
    [Header("Path Debugging")]
    public bool updateDebuggingPathStats = false;

    public LineRenderer d_DrawPathAvatar;
    public LineRenderer d_DrawPathUser;
    public LineRenderer d_DrawPathUser1;
    public LineRenderer d_DrawPathUser2;

    public RouteStatistic debug_statistic0A;
    public RouteStatistic debug_statistic0U;
    public RouteStatistic debug_statistic1A;
    public RouteStatistic debug_statistic1U;
    public RouteStatistic debug_statistic2A;
    public RouteStatistic debug_statistic2U;

    private void UpdateDebuggingPathStats()
    {
        var statPath2 = DrawDebugInfo(d_DrawPathAvatar, d_DrawPathUser2, 2, MovementWaitDelay);
        var statPath0 = DrawDebugInfo(d_DrawPathAvatar, d_DrawPathUser, 0, MovementWaitDelay);
        var statPath1 = DrawDebugInfo(d_DrawPathAvatar, d_DrawPathUser1, 1, MovementWaitDelay);

        if (statPath2 != null)
        {
            debug_statistic2A = statPath2.Item1;
            debug_statistic2U = statPath2.Item2;
        }
        if (statPath1 != null)
        {
            debug_statistic1A = statPath1.Item1;
            debug_statistic1U = statPath1.Item2;
        }
        if (statPath0 != null)
        {
            debug_statistic0A = statPath0.Item1;
            debug_statistic0U = statPath0.Item2;
        }
    }

    private Tuple<RouteStatistic, RouteStatistic> DrawDebugInfo(LineRenderer a_rendererAvatarLine, LineRenderer a_rendererUserPath, int a_routeIndex, double a_movmentDelay)
    {
        if (a_rendererAvatarLine != null && a_rendererUserPath != null && HasRouteForIndex(a_routeIndex))
        {
            var ordersAvatar = Routes[a_routeIndex].CreateMovementOrderListFromStages(MovementWaitDelay);
            var statisticAvatar = new RouteStatistic();
            statisticAvatar.CreateFromOrderList(ordersAvatar, a_routeIndex, a_movmentDelay);
            a_rendererAvatarLine.positionCount = statisticAvatar.Waypoints.Length;
            a_rendererAvatarLine.SetPositions(statisticAvatar.Waypoints);

            var ordersUser = Routes[a_routeIndex].CreateMovementOrderListFromStagesForUser(MovementWaitDelay);
            var statisticUser = new RouteStatistic();
            statisticUser.CreateFromOrderList(ordersUser, a_routeIndex, a_movmentDelay);
            a_rendererUserPath.positionCount = statisticUser.Waypoints.Length;
            a_rendererUserPath.SetPositions(statisticUser.Waypoints);

            return new Tuple<RouteStatistic, RouteStatistic>(statisticAvatar, statisticUser);
        }
        else
        {
            //Debug.LogWarning("No LineRenderer assigned or no index. Doing nothing");
        }
        return null;
    }
    #endregion

    private void Awake()
    {
        SetupLogFileDirectory();

        if (gameSettings == null)
            throw new MissingComponentException("gameSettings component is not set");
        if (PlayerHead == null)
            throw new MissingComponentException("PlayerHead component is not set");

        PhotonViewScriptedAvatar = ScriptedAvatar.GetComponent<PhotonView>();
        if (PhotonViewScriptedAvatar == null)
        {
            Debug.LogWarning("Scripted Avatar has no PhotonView Attached. This may cause problems if Avatar is Networked. Otherwise this can be ignored", this.gameObject);
        }
        if (Routes.Count == 0)
        {
            Debug.LogWarning("No routes defined. Is this intentional?", this.gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // prepare stages  of routes
        foreach (var r in Routes)
        {
            r.InterconnectStages();
        }

        ResetStudy();
    }

    // Update is called once per frame
    void Update()
    {
        if (!this.photonView.IsMine)
        {
            // only if not owned. -> apply data from network
            if (m_hasUnappliedNetworkData)
            {
                m_hasUnappliedNetworkData = false;

                m_currentRouteIndex = r_currentRouteIndex;
                m_currentStageIndex = r_currentStage;
            }
        }

        //// TODO  this is a debug logging, must be removed
        //if (HasRouteForIndex(m_currentRouteIndex))
        //{
        //    foreach (var stage in Routes[m_currentRouteIndex].Stages)
        //    {
        //        stage.ShortestDistanceToPath(d1_E.transform);
        //    }
        //}
    }

    void FixedUpdate()
    {
        // take measurements at ~50fps (50ms) (same "speed" as it would be possible to move for the avatar)

        DateTime FixedUpdateLogtime = DateTime.UtcNow;
        string FixedUpdateLogtimeStr = FixedUpdateLogtime.ToString("o");
        string tpMode = gameSettings.TeleportationMode.ToString();


        if (m_firstStageBegun && !m_processedAllStages)
        {
            //Debug.Log("logging a stage in progress");
            if (HasRouteForIndex(m_currentRouteIndex))
            {
                // update distances
                var stage = Routes[m_currentRouteIndex].Stages[m_currentStageIndex];
                stage.UpdateCurrentAndMaxDistance(PlayerHead.transform);
                float curentDistanceUserPath = stage.LastShortestDistanceToUserPath;

                // do continuous Logs (if DoPersistAndSendLogs == true)

                LogParticipantHeadPosition(FixedUpdateLogtimeStr, CurrentRouteIndex, CurrentStageIndex, tpMode, curentDistanceUserPath);
                LogScriptedAvatarPosition(FixedUpdateLogtimeStr, CurrentRouteIndex, CurrentStageIndex, tpMode);
            }
        }
        else if (m_arrivedAtFirstInteractable && m_firstStageBegun == false)
        {
            //Debug.Log("arrived but waiting to trigger first interactable");
            LogParticipantHeadPosition(FixedUpdateLogtimeStr, CurrentRouteIndex, -1, tpMode, 0);
            LogScriptedAvatarPosition(FixedUpdateLogtimeStr, CurrentRouteIndex, -1, tpMode);
        }

        if (updateDebuggingPathStats)
        {
            UpdateDebuggingPathStats();
        }
    }

    #region LogAnalysisTool

    // TODO add LogAnalysis Tool code
    private LoggingStartInformation GenerateLogginStartInfo()
    {
        string taskId = $"Task1-RouteIdx{m_currentRouteIndex}";
        return new LoggingStartInformation(gameSettings.LoggingSessionId, PlayerSettings.Instance.LoggingParticipantId, taskId);
    }

    private void StartDetailedLogging()
    {
        string taskId = $"Task1-RouteIdx{m_currentRouteIndex}";
        string sessionId = gameSettings.LoggingSessionId;
        string participantId = PlayerSettings.Instance.LoggingParticipantId;

        Debug.Log($"########## Start Detailed Logging - session: {sessionId} - task: {taskId} - pid: {participantId}");
        if (DoPersistAndSendLogs && DoUseLogAnalysisTool)
        {
            // TODO add LogAnalysis Tool code
            var logginInfo = GenerateLogginStartInfo();
            //Debug.Log($"########## Start Detailed Logging - session: {logginInfo.sessionId} - task: {logginInfo.sessionId} - pid: {logginInfo.participantId}");
            Logger.StartLogging(logginInfo);
        }
    }

    private void StopDetailedLogging()
    {
        Debug.Log("########## Stop Detailed Logging (DoPersistAndSendLogs: " + DoPersistAndSendLogs + ")");
        if (DoPersistAndSendLogs)
        {
            // TODO add LogAnalysis Tool code
            Logger.StopLogging();
        }
    }
    #endregion

    #region Event and Contiuous Logging
    private static string m_currentLogFileDirectory = "";

    private void LogParticipantHeadPosition(string a_logTime, int a_routeIdx, int a_currStageIdx, string a_tpMode, float a_currDistanceUserPath)
    {
        if (DoPersistAndSendLogs)
        {
            Transform phead = PlayerHead.transform;
            float rotX = phead.rotation.x;
            float rotY = phead.rotation.y;
            float rotZ = phead.rotation.z;
            float rotW = phead.rotation.w;

            string log = $"pH;{gameSettings.LoggingSessionId};{PlayerSettings.Instance.LoggingParticipantId};{a_routeIdx};{a_logTime};{phead.position.x};{phead.position.y};{phead.position.z};{rotX};{rotY};{rotZ};{rotW};{a_currStageIdx};{a_currDistanceUserPath};{a_tpMode}";

            string filename = $"PlayerHead_ssn{gameSettings.LoggingSessionId}.csv";
            WriteToFile(filename, log);
        }
        else
        {
            return;
        }
    }

    private void LogScriptedAvatarPosition(string a_logTime, int a_routeIdx, int a_currStageIdx, string a_tpMode)
    {
        if (DoPersistAndSendLogs)
        {
            Transform aHead = ScriptedAvatar.Avatar.transform;
            float rotX = aHead.rotation.x;
            float rotY = aHead.rotation.y;
            float rotZ = aHead.rotation.z;
            float rotW = aHead.rotation.w;
            string log = $"aH;{gameSettings.LoggingSessionId};{PlayerSettings.Instance.LoggingParticipantId};{a_routeIdx};{a_logTime};{aHead.position.x};{aHead.position.y};{aHead.position.z};{rotX};{rotY};{rotZ};{rotW};{a_currStageIdx};{a_tpMode}";

            string filename = $"AvatarHead_ssn{gameSettings.LoggingSessionId}.csv";
            WriteToFile(filename, log);
        }
        else
        {
            return;
        }
    }

    private void LogArrivedFirstTimeAtTargetStandingArea(DateTime a_frameTime, int a_interactableIdx, int a_activeOrEndedStageIdx, TimeSpan a_TimePastSinceStart, float a_maxDistanceUserPath)
    {
        if (DoPersistAndSendLogs)
        {
            // -------stage_-1------- i0 -------stage_0------- i1 -------stage_1------- i2 -------stage_2------- i3
            // how many interactables: stages + 1 => 3+1 = 4 (0-3) || 11+1= 12

            // needed info
            string logTimeStr = a_frameTime.ToString("o");
            double msPastSinceStart = a_TimePastSinceStart.TotalMilliseconds;
            Transform aHead = ScriptedAvatar.Avatar.transform;
            float rotX = aHead.rotation.x;
            float rotY = aHead.rotation.y;
            float rotZ = aHead.rotation.z;
            float rotW = aHead.rotation.w;
            string tpMode = gameSettings.TeleportationMode.ToString();

            //string logHeader = "eAr;Session;Participantid;Route idx;Time;Interactable idx;Stage idx;msPast;maxDistanceUserPath;phead x;y;z;Rotation x;y;z;w;tpMode";
            string log = $"eAr;{gameSettings.LoggingSessionId};{PlayerSettings.Instance.LoggingParticipantId};{CurrentRouteIndex};{logTimeStr};{a_interactableIdx};{a_activeOrEndedStageIdx};{msPastSinceStart};{a_maxDistanceUserPath};{aHead.position.x};{aHead.position.y};{aHead.position.z};{rotX};{rotY};{rotZ};{rotW};{tpMode}";
            string filename = $"Events_ArrivedAtInteractable_ssn{gameSettings.LoggingSessionId}.csv";

            WriteToFile(filename, log);
            if (DoUseLogAnalysisTool)
            {
                // TODO add LogAnalysis Tool code
                CustomSpatialEventArgs evt = new CustomSpatialEventArgs("ArrivedAtTargetInteractable", $"Arrived at interactableIdx: {a_interactableIdx} msPast: {msPastSinceStart} currentStage: {a_activeOrEndedStageIdx} routeIndex: {CurrentRouteIndex} maxDistanceToUserPath: {a_maxDistanceUserPath}", msPastSinceStart.ToString(), typeof(double), aHead.position);
                Logger.InvokeCustomSpatialEvent(evt);
            }
        }
        else
        {
            return;
        }
    }

    private void LogEventTriggeredInteractable(DateTime a_frameTime, int a_interactableIdx, int a_stageIdxThatEnded, TimeSpan a_TimePastSinceStart, float a_maxDistanceUserPath)
    {
        if (DoPersistAndSendLogs)
        {
            // needed info
            string logTimeStr = a_frameTime.ToString("o");
            double msPastSinceStart = a_TimePastSinceStart.TotalMilliseconds;
            Transform aHead = ScriptedAvatar.Avatar.transform;
            float rotX = aHead.rotation.x;
            float rotY = aHead.rotation.y;
            float rotZ = aHead.rotation.z;
            float rotW = aHead.rotation.w;
            string tpMode = gameSettings.TeleportationMode.ToString();

            // -------stage_-1------- i0 -------stage_0------- i1 -------stage_1------- i2 -------stage_2------- i3
            // how many interactables: stages + 1 => 3+1 = 4 (0-3) || 11+1= 12

            //string logHeader = "eAr;Session;Participantid;Route idx;Time;Interactable idx;Ended Stage Idx;msPast;maxDistanceUserPath;phead x;y;z;Rotation x;y;z;w;tpMode";
            string log = $"eTr;{gameSettings.LoggingSessionId};{PlayerSettings.Instance.LoggingParticipantId};{CurrentRouteIndex};{logTimeStr};{a_interactableIdx};{a_stageIdxThatEnded};{msPastSinceStart};{a_maxDistanceUserPath};{aHead.position.x};{aHead.position.y};{aHead.position.z};{rotX};{rotY};{rotZ};{rotW};{tpMode}";
            string filename = $"Events_InteractableTriggered_ssn{gameSettings.LoggingSessionId}.csv";

            WriteToFile(filename, log);

            if (DoUseLogAnalysisTool)
            {
                // TODO add LogAnalysis Tool code
                CustomSpatialEventArgs evt = new CustomSpatialEventArgs("TriggeredTargetInteractable", $"Arrived at interactableIdx: {a_interactableIdx} msPast: {msPastSinceStart} endedStage: {a_stageIdxThatEnded} routeIndex: {CurrentRouteIndex} maxDistanceToUserPath: {a_maxDistanceUserPath}", msPastSinceStart.ToString(), typeof(double), aHead.position);
                Logger.InvokeCustomSpatialEvent(evt);
            }
        }
        else
        {
            return;
        }
    }

    private void SetupLogFileDirectory()
    {
        m_currentLogFileDirectory = Application.persistentDataPath + Path.DirectorySeparatorChar + "CollabMovement_LogData" + Path.DirectorySeparatorChar;
        //Debug.Log($"Manually Path {Application.persistentDataPath + Path.DirectorySeparatorChar + "CollabMovement_LogData" + Path.DirectorySeparatorChar}");
        //Debug.Log($"Combine Path {Path.Combine(Application.persistentDataPath, "CollabMovement_LogData")}");
        if (!Directory.Exists(m_currentLogFileDirectory)) // if it doesn't exist, create
        {
            Debug.Log("Creating Logging Directory Path: " + m_currentLogFileDirectory);
            Directory.CreateDirectory(m_currentLogFileDirectory);
        }
    }

    private void WriteToFile(string a_filename, string a_text)
    {
        string path = m_currentLogFileDirectory + a_filename;
        bool doesFileExist = System.IO.File.Exists(path);

        if (doesFileExist)
        {
            using (StreamWriter sw = File.AppendText(path))
            {
                sw.WriteLine(a_text);
            }
        }
        else
        {
            using (StreamWriter sw = File.CreateText(path))
            {
                sw.WriteLine(a_text);
            }
        }
    }

    #endregion

    #region Networking
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(m_currentStageIndex);
            stream.SendNext(m_currentRouteIndex);
        }
        else
        {
            r_currentStage = (int)stream.ReceiveNext();
            r_currentRouteIndex = (int)stream.ReceiveNext();
            m_hasUnappliedNetworkData = true;
        }
    }
    #endregion

}

