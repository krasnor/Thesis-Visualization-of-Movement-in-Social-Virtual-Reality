using Assets.VisCollabMovementStudy.Scripts;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

public class CollaborationStudyManager : MonoBehaviour
{
    public enum Variant { Undefined, VariantA, VariantB, VariantC }

    public NetworkedConstructionSite ConstructionSite;
    public GameSettings GameSettings;
    public bool DoSaveLogs = true;

    [Space]
    public int ModelIdVariantA = 0;
    public GameObject ParentDigsitesVariantA;
    public int ModelIdVariantB = 1;
    public GameObject ParentDigsitesVariantB;
    public int ModelIdVariantC = 2;
    public GameObject ParentDigsitesVariantC;

    private void Awake()
    {
        if (GameSettings == null)
            throw new MissingComponentException("GameSettings component not set");
        if (ConstructionSite == null)
            throw new MissingComponentException("ConstructionSite component not set");

        if (ParentDigsitesVariantA == null)
            throw new MissingComponentException("ParentDigsitesVariantA component not set");
        if (ParentDigsitesVariantB == null)
            throw new MissingComponentException("ParentDigsitesVariantB component not set");
        //Debug.LogWarning("No digsites parent defined for VariantB. Is this intentional?");
        if (ParentDigsitesVariantC == null)
            throw new MissingComponentException("ParentDigsitesVariantC component not set");
    }
    // Start is called before the first frame update
    void Start()
    {
        ConstructionSite.OnBuildSiteCompleted.AddListener(OnBuildSiteCompleted);
        ConstructionSite.OnBuildPartDelivered.AddListener(OnBuildPartDelivered);
    }

    // Update is called once per frame
    void Update()
    {
        // update Visibility status - this is done locally, but digsite is managed over network
        UpdateDigsiteVisibility();
    }

    public void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
    }

    public void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
    }

    public void SendTriggerParticipantMeasurementEvent(bool a_studyStarted)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }

        byte eventCode;
        if (a_studyStarted)
        {
            eventCode = CollabStudyNetworkEventCodes.StartTask2MeasurementEventCode;
        }
        else
        {
            eventCode = CollabStudyNetworkEventCodes.EndTask2MeasurementEventCode;
        }
        
        object[] content = new object[] { };
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All }; // You would have to set the Receivers to All in order to receive this event on the local client as well
        PhotonNetwork.RaiseEvent(eventCode, content, raiseEventOptions, SendOptions.SendReliable);
    }

    private void OnEvent(EventData photonEvent)
    {
        try
        {
            byte eventCode = photonEvent.Code;

            if (eventCode == CollabStudyNetworkEventCodes.StartTask2MeasurementEventCode)
            {
                Debug.Log("CollaborationStudyManager: EVT: START Task 2 Measurement");

                //object[] data = (object[])photonEvent.CustomData;
                //int actorId = (int)data[0];
                // TODO add LogAnalysis Tool code
                //var logginInfo = GenerateLogginStartInfo();
                //Logger.StartLogging(logginInfo);

            }
            if (eventCode == CollabStudyNetworkEventCodes.EndTask2MeasurementEventCode)
            {
                Debug.Log("CollaborationStudyManager: EVT: STOP Task 2 Measurement");
                // TODO add LogAnalysis Tool code
                //Logger.StopLogging();
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("CollaborationStudyManager: An error occured while handling Task2MeasurementEvent. Error: " + ex);
        }
    }

    //private LoggingStartInformation GenerateLogginStartInfo()
    //{
    //    string taskId = $"Task2-BuildModelId{ConstructionSite.CurrentBuildModelId}";
    //    return  new LoggingStartInformation(gameSettings.LoggingSessionId, PlayerSettings.Instance.LoggingParticipantId, taskId);
    //}


    private void UpdateDigsiteVisibility()
    {
        int currentBuildVariant = ConstructionSite.CurrentBuildModelId;
        bool shouldVariantABeActive = (currentBuildVariant == ModelIdVariantA);
        bool shouldVariantBBeActive = (currentBuildVariant == ModelIdVariantB);
        bool shouldVariantCBeActive = (currentBuildVariant == ModelIdVariantC);

        if (ParentDigsitesVariantA.activeSelf != shouldVariantABeActive)
            ParentDigsitesVariantA.SetActive(shouldVariantABeActive);
        if (ParentDigsitesVariantB.activeSelf != shouldVariantBBeActive)
            ParentDigsitesVariantB.SetActive(shouldVariantBBeActive);
        if (ParentDigsitesVariantC.activeSelf != shouldVariantCBeActive)
            ParentDigsitesVariantC.SetActive(shouldVariantCBeActive);
    }


    public void RequestOwnershipAndResetStudyToVariant(Variant a_ModelVariant)
    {
        DidStudyStart = false;
        Task2Start = DateTime.MinValue;
        Task2End = DateTime.MinValue;

        int variantId = -1;
        List<NetworkedDigsite> listDigsites = new List<NetworkedDigsite>();
        switch (a_ModelVariant)
        {
            case Variant.VariantA:
                variantId = ModelIdVariantA;
                listDigsites.AddRange(ParentDigsitesVariantA.GetComponentsInChildren<NetworkedDigsite>());
                break;
            case Variant.VariantB:
                variantId = ModelIdVariantB;
                listDigsites.AddRange(ParentDigsitesVariantB.GetComponentsInChildren<NetworkedDigsite>());
                break;
            case Variant.VariantC:
                variantId = ModelIdVariantC;
                listDigsites.AddRange(ParentDigsitesVariantC.GetComponentsInChildren<NetworkedDigsite>());
                break;
            case Variant.Undefined:
                Debug.LogWarning("Cannot Reset to Variant.Undefined");
                return;
            default:
                Debug.LogWarning("Unhandled modelVariant " + a_ModelVariant);
                return;
        }


        ConstructionSite.RequestOwnershipAndReset(variantId);
        foreach (var b in listDigsites)
        {
            b.RequestOwnershipAndResetDigsite();
        }

        // Digsite visibility will be updated next Update()
    }

    public Variant CurrentBuildModelIdToVariant()
    {
        int currentBuildVariant = ConstructionSite.CurrentBuildModelId;

        // switch not possible as C#-Switch must be statically evaluated (CS0150)

        if (currentBuildVariant == ModelIdVariantA)
        {
            return Variant.VariantA;
        }
        else if (currentBuildVariant == ModelIdVariantB)
        {
            return Variant.VariantB;
        }
        else if (currentBuildVariant == ModelIdVariantC)
        {
            return Variant.VariantC;
        }
        else
        {
            return Variant.Undefined;
        }
    }

    public void OverrideAdvanceProgress()
    {
        ConstructionSite.OverrideAdvanceProgress();
    }

    public DateTime Task2Start;
    public DateTime Task2End;
    public bool DidStudyStart = false;

    public void BeginStudy()
    {
        Debug.Log("BeginStudy - Task 2 - Variant: " + ConstructionSite.CurrentBuildModelId);
        Task2Start = DateTime.UtcNow;
        DidStudyStart = true;

        SendTriggerParticipantMeasurementEvent(true);
    }

    public void EndStudy()
    {
        if (DidStudyStart && PhotonNetwork.IsMasterClient)
        {           
            // only masterclient should/needs to make this log
            DidStudyStart = false;
            Task2End = DateTime.UtcNow;

            TimeSpan duration = Task2End - Task2Start;
            string str_sessionId = GameSettings.LoggingSessionId;
            string str_started = Task2Start.ToString(@"O");
            string str_ended = Task2End.ToString(@"O");
            string str_duration = duration.ToString(@"hh\:mm\:ss\.fffffff");
            string str_durationMillis = duration.TotalMilliseconds.ToString();
            int buildModelId = ConstructionSite.CurrentBuildModelId;
            string str_mode = GameSettings.TeleportationMode.ToString();

            string m_logLineDescr = $"Event; SessionId; Task Started; Task Ended; Duration; Duration Millis; BuildModelId; TP Mode";
            string line = $"ConstructionComplete; {str_sessionId}; {str_started}; {str_ended}; {str_duration}; {str_durationMillis}; {buildModelId}; {str_mode}";
            Debug.Log("OnBuildSiteCompleted - Task 2 - " + line);
            if (DoSaveLogs)
            {
                string filename = $"log_Task2_ssn{str_sessionId}.txt";
                WriteToFile(line, "/", filename, m_logLineDescr);
            }
            
            // TODO add LogAnalysis Tool code
            string line_logger = $"ConstructionComplete SessionId: {str_sessionId} Started: {str_started} Ended: {str_ended} Duration: {str_duration} DurationMillis: {str_durationMillis} BuildModelId: {buildModelId} TPMode: {str_mode}";
            //CustomEventArgs args = new CustomEventArgs("StageComplete", line_logger, duration.TotalMilliseconds, typeof(double), ConstructionSite.gameObject.transform.position);
            //Logger.InvokeCustomEvent(args);

            SendTriggerParticipantMeasurementEvent(false);
        }
        else
        {
            Debug.Log($"OnBuildSiteCompleted - Task 2 - Event received but not processed DidStudyStart: {DidStudyStart} IsMasterClient: {PhotonNetwork.IsMasterClient} BuidModelId: {ConstructionSite?.CurrentBuildModelId.ToString()} TPMode: {GameSettings.TeleportationMode.ToString()}");
        }
    }


    void OnBuildPartDelivered(int a_buildModelId, int a_buildPartId)
    {
        if (DidStudyStart && PhotonNetwork.IsMasterClient)
        {
            string str_sessionId = GameSettings.LoggingSessionId;
            var timeDelivered = DateTime.UtcNow;
            string timeDeliveredStr = timeDelivered.ToString(@"O");
            string strBuildModel = a_buildModelId.ToString();
            string strBuildPartId = a_buildPartId.ToString();
            string str_mode = GameSettings.TeleportationMode.ToString();

            string line = $"BuildPartDelivered; {str_sessionId}; {timeDeliveredStr}; {strBuildModel}; {strBuildPartId}; {str_mode}";
            Debug.Log($"BuildPartDelivered - Task 2 - {line}");

            if (DoSaveLogs)
            {
                string filename = $"log_Task2_ssn{str_sessionId}.txt";
                WriteToFile(line, "/", filename, m_logLineDescrPartDelivered);
            }

            // TODO add LogAnalysis Tool code
            string lineLogger = $"BuildPartDelivered SessionId: {str_sessionId} Delivered: {timeDeliveredStr} BuildModelId: {strBuildModel} BuildPartId: {strBuildPartId} TPMode: {str_mode}";
            //CustomEventArgs args = new CustomEventArgs("StageComplete", lineLogger, timeDeliveredStr, typeof(string), ConstructionSite.gameObject.transform.position);
            //Logger.InvokeCustomEvent(args);
        }
        else
        {
            Debug.Log($"BuildPartDelivered - Task 2 - Event received but not processed DidStudyStart: {DidStudyStart} IsMasterClient: {PhotonNetwork.IsMasterClient} BuidModelId: {a_buildModelId.ToString()} BuildPartId: {a_buildPartId} TPMode: {GameSettings.TeleportationMode.ToString()}");
        }
    }

    void OnBuildSiteCompleted(int a_buildModelId)
    {
        EndStudy();
    }

    private Regex m_illegalInFileName = new Regex(string.Format("[{0}]", Regex.Escape(new string(Path.GetInvalidFileNameChars()))), RegexOptions.Compiled);
    private readonly string m_logLineDescr = $"Event; SessionId; Task Started; Task Ended; Duration; Duration Millis; BuildModelId; TP Mode";
    private readonly string m_logLineDescrPartDelivered = $"Event; SessionId; Time Delivered; BuildModelId; BuildPartId; TP Mode";

    private void WriteToFile(string a_line, string a_path = "/", string a_filename = "test.txt", string a_firstLineInNewFile = "")
    {
        //Debug.Log("writing: " + a_line);
        try
        {
            a_filename = m_illegalInFileName.Replace(a_filename, "");

            string path = Application.persistentDataPath + a_path + a_filename;
            bool doesFileExist = System.IO.File.Exists(path);

            //Write some text to the test.txt file
            StreamWriter writer = new StreamWriter(path, true);
            if (!doesFileExist)
            {
                if (!string.IsNullOrWhiteSpace(a_firstLineInNewFile))
                    writer.WriteLine(a_firstLineInNewFile);
            }
            writer.WriteLine(a_line);
            writer.Close();
        }
        catch (Exception)
        {

            throw;
        }
    }
}
