using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AnalysisManager : MonoBehaviour
{
    [SerializeField]
    private StudyManager m_studyManager;

    [SerializeField]
    private ReplayManager m_replayManager;

    [SerializeField]
    private DataQueryManager m_dataQueryManager;

    public StudyManager StudyManager { get => m_studyManager; private set => m_studyManager = value; }

    private static AnalysisManager m_instance;

    public static AnalysisManager Instance { get { return m_instance; } }

    private void Awake()
    {
        if (m_instance != null && m_instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            m_instance = this;
        }
    }

    // Start is called before the first frame update
    void Start()
    {

        if (m_studyManager.ActiveStudy.studySessions == null || m_studyManager.ActiveStudy.studySessions.Count == 0)
        {
            Debug.LogWarning("No Study Sessions defined in StudyDefinition. Reading directly from directory.");
            TextFileReader.TryReadStudySessionsFromDirectory(m_studyManager.ActiveStudy, out var studySessions);
            m_studyManager.ActiveStudy.studySessions = studySessions;
        }

        List<SpatialAudioEventDataPoint> audioEventDataPoints = new List<SpatialAudioEventDataPoint>();
        List<CustomSpatialEventDataPoint> customEventDataPoints = new List<CustomSpatialEventDataPoint>();
        List<ParticipantTrackingDataPoint> continuousDataPoints = new List<ParticipantTrackingDataPoint>();
        List<NLPCalcResponse> nlpCalcResponses = new List<NLPCalcResponse>();

        m_replayManager.Play();

        foreach (var studySession in m_studyManager.ActiveStudy.studySessions)
        {
            foreach (var studyParticipant in studySession.studyParticipants)
            {
                TextFileReader textFileReader = new TextFileReader(StudyManager.ActiveStudy, studySession, studyParticipant);
                TextFileWriter textFileWriter = new TextFileWriter(StudyManager.ActiveStudy, studySession, studyParticipant);

                if (textFileReader.TryReadLoggingDataPoints<SpatialAudioEventDataPoint>(LoggingDataPointType.SpatialAudioEvent, out var audioEventDataPointsArray))
                {
                    audioEventDataPoints.AddRange(audioEventDataPointsArray);
                }

                if (textFileReader.TryReadLoggingDataPoints<CustomSpatialEventDataPoint>(LoggingDataPointType.SpatialAudioEvent, out var customEventDataPointArray))
                {
                    customEventDataPoints.AddRange(customEventDataPointArray);
                }

                if (textFileReader.TryReadLoggingDataPoints<ParticipantTrackingDataPoint>(LoggingDataPointType.ParticipantTrackingData, out var continuousDataPointsArray))
                {
                    continuousDataPoints.AddRange(continuousDataPointsArray);
                }
                if (!textFileReader.TryReadFromNLPCalc(out var nlpCalcResponsesArray))
                {
                    NLPCalcRequester sentimentCalcRequester = new NLPCalcRequester();
                    StartCoroutine(sentimentCalcRequester.Post(audioEventDataPointsArray, textFileWriter));
                }
                else
                {
                    nlpCalcResponses.AddRange(nlpCalcResponsesArray);
                }
            }
        }
        var audioPIdDictionary = audioEventDataPoints.GroupBy(k => k.participantId, v => v).ToDictionary(g => g.Key, g => g.ToList());
        var audioTIdDictionary = audioEventDataPoints.GroupBy(k => k.taskId, v => v).ToDictionary(g => g.Key, g => g.ToList());
        var continuousPIdDictionary = continuousDataPoints.GroupBy(k => k.participantId, v => v).ToDictionary(g => g.Key, g => g.ToList());
        var continuousTIdDictionary = continuousDataPoints.GroupBy(k => k.taskId, v => v).ToDictionary(g => g.Key, g => g.ToList());

        if (nlpCalcResponses.Count == 0)
        {
            foreach (var studySession in m_studyManager.ActiveStudy.studySessions)
            {
                foreach (var studyParticipant in studySession.studyParticipants)
                {
                    TextFileReader textFileReader = new TextFileReader(StudyManager.ActiveStudy, studySession, studyParticipant);
                    if (textFileReader.TryReadFromNLPCalc(out var nlpCalcResponsesArray))
                    {
                        nlpCalcResponses.AddRange(nlpCalcResponsesArray);
                    }
                }
            }
        }

        var nlpCalcPIdDictionary = nlpCalcResponses.GroupBy(k => k.participantId, v => v).ToDictionary(g => g.Key, g => g.ToList());
        var nlpCalcTIdDictionary = nlpCalcResponses.GroupBy(k => k.taskId, v => v).ToDictionary(g => g.Key, g => g.ToList());


        //m_pathRenderer.ShowVisualizationDataPoint(continuousPIdDictionary);
        //m_pointCloud.ShowAudioDataPoints(audioAndInfoPIdDictionary);
        //m_heatMapRenderer.ShowHeatMaps(continuousDataPoints);
        MapPositionToAudioEvent();
    }

    /// <summary>
    /// TODO Maybe change date time format
    /// </summary>
    public void MapPositionToAudioEvent()
    {
        var participantData = m_dataQueryManager.GetSpatialDataPoints<ParticipantTrackingDataPoint>(LoggingDataPointType.ParticipantTrackingData, GroupingCategory.ParticipantId);

        foreach (var studySession in m_studyManager.ActiveStudy.studySessions)
        {
            foreach (var studyParticipant in studySession.studyParticipants)
            {
                var textFileReader = new TextFileReader(m_studyManager.ActiveStudy, studySession, studyParticipant);
                var textFileWriter = new TextFileWriter(m_studyManager.ActiveStudy, studySession, studyParticipant);
                if(!textFileReader.LoggingDataPointsExist<OfflineSpatialAudioEventDataPoint>(LoggingDataPointType.OfflineSpatialAudioEvent))
                {
                    if (textFileReader.TryReadLoggingDataPoints<ParticipantTrackingDataPoint>(LoggingDataPointType.ParticipantTrackingData, out var participantTrackingDataPointArray))
                    {
                        if (textFileReader.TryReadLoggingDataPoints<OfflineAudioEventDataPoint>(LoggingDataPointType.OfflineAudioEvent, out var offlineAudioEventDataPointArray))
                        {
                            List<OfflineSpatialAudioEventDataPoint> offlineSpatialAudioEventDataPoints = new List<OfflineSpatialAudioEventDataPoint>();

                            foreach (var offlineAudioEventData in offlineAudioEventDataPointArray)
                            {
                                ParticipantTrackingDataPoint prevParticipant = null;
                                OfflineSpatialAudioEventDataPoint offlineSpatialAudioEventDataPoint = null;

                                foreach (var participant in participantTrackingDataPointArray)
                                {
                                    if (offlineSpatialAudioEventDataPoint == null)
                                    {
                                        if (participant.pointInTimeUtc.Equals(offlineAudioEventData.pointInTimeUtc)
                                            || (prevParticipant != null && participant.pointInTimeUtc >= offlineAudioEventData.pointInTimeUtc))
                                        {
                                            var leftMsDistance = (prevParticipant.pointInTimeUtc - offlineAudioEventData.pointInTimeUtc).TotalMilliseconds;
                                            var rightMsDistance = (participant.pointInTimeUtc - offlineAudioEventData.pointInTimeUtc).TotalMilliseconds;
                                            double timeOfPositionOffset;
                                            string position;
                                            if (Math.Abs(leftMsDistance) > rightMsDistance)
                                            {
                                                timeOfPositionOffset = rightMsDistance;
                                                position = participant.position;
                                            }
                                            else
                                            {
                                                timeOfPositionOffset = leftMsDistance;
                                                position = prevParticipant.position;
                                            }
                                            offlineSpatialAudioEventDataPoint = offlineAudioEventData.ParseToSpatialOfflineAudioEvent(position);
                                            offlineSpatialAudioEventDataPoint.startOffsetInMs = timeOfPositionOffset;
                                        }
                                    }
                                    else
                                    {
                                        if (participant.pointInTimeUtc.Equals(offlineAudioEventData.endPointInTimeUtc)
                                            || (prevParticipant != null && participant.pointInTimeUtc >= offlineAudioEventData.endPointInTimeUtc))
                                        {
                                            var leftMsDistance = (prevParticipant.pointInTimeUtc - offlineAudioEventData.endPointInTimeUtc).TotalMilliseconds;
                                            var rightMsDistance = (participant.pointInTimeUtc - offlineAudioEventData.endPointInTimeUtc).TotalMilliseconds;
                                            double timeOfPositionOffset;
                                            string position;
                                            if (Math.Abs(leftMsDistance) > rightMsDistance)
                                            {
                                                timeOfPositionOffset = rightMsDistance;
                                                position = participant.position;
                                            }
                                            else
                                            {
                                                timeOfPositionOffset = leftMsDistance;
                                                position = prevParticipant.position;
                                            }
                                            offlineSpatialAudioEventDataPoint.endPosition = position;
                                            offlineSpatialAudioEventDataPoint.endOffsetInMs = timeOfPositionOffset;
                                            offlineSpatialAudioEventDataPoints.Add(offlineSpatialAudioEventDataPoint);
                                            break;
                                        }
                                    }
                                    prevParticipant = participant;
                                }
                            }

                            textFileWriter.WriteToCSV(offlineSpatialAudioEventDataPoints);
                        }
                    }
                    else
                    {
                        Debug.Log("OfflineSpatialAudioEvent already calculated.");
                    }
                }
            }
        }

    }

}
