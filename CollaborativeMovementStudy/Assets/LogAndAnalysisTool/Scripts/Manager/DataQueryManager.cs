using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DataQueryManager : MonoBehaviour
{
    [SerializeField]
    private StudyManager m_studyManager;

    private List<ParticipantTrackingDataPoint> m_participantTrackingDataPoints;

    private List<TransformTrackingDataPoint> m_transformTrackingDataPoints;

    private List<InputActionEventDataPoint> m_inputActionEventDataPoints;

    private List<SpatialAudioEventDataPoint> m_audioEventDataPoints;

    private List<OfflineAudioEventDataPoint> m_offlineAudioEventDataPoints;

    private List<OfflineSpatialAudioEventDataPoint> m_offlineSpatialAudioEventDataPoints;

    private List<CustomSpatialEventDataPoint> m_customEventDataPoints;


    public List<T> LoadDataPoints<T>(LoggingDataPointType a_loggingDataPointType) where T : LoggingDataPoint
    {
        var dataPoints = new List<T>();
        foreach (var studySession in m_studyManager.ActiveStudy.studySessions)
        {
            foreach (var studyParticipant in studySession.studyParticipants)
            {
                var textFileReader = new TextFileReader(m_studyManager.ActiveStudy, studySession, studyParticipant);
                if (textFileReader.TryReadLoggingDataPoints<T>(a_loggingDataPointType, out var dataPointArray))
                {
                    dataPoints.AddRange(dataPointArray);
                }
            }
        }
        return dataPoints;
    }

    public Dictionary<string, List<T>> GetSpatialDataPoints<T>(LoggingDataPointType a_loggingDataPointType, Dictionary<FilterCategory, List<string>> a_filterSelections, GroupingCategory a_grouping) where T : SpatialLoggingDataPoint
    {
        IEnumerable<T> dataPoints = new List<T>();
        var groupingPredicate = GetGroupingPredicate<T>(a_grouping);
        var spatialDataPoints = GetSpatialDataPoints<T>(a_loggingDataPointType, a_filterSelections);
        var data = spatialDataPoints.GroupBy(groupingPredicate).ToDictionary(g => g.Key, g => g.ToList());
        return data;
    }

    public Dictionary<string, List<T>> GetSpatialDataPoints<T>(LoggingDataPointType a_loggingDataPointType, GroupingCategory a_grouping) where T : SpatialLoggingDataPoint
    {
        IEnumerable<T> dataPoints = new List<T>();
        var groupingPredicate = GetGroupingPredicate<T>(a_grouping);
        var spatialDataPoints = GetSpatialDataPoints<T>(a_loggingDataPointType);
        var data = spatialDataPoints.GroupBy(groupingPredicate).ToDictionary(g => g.Key, g => g.ToList());
        return data;
    }

    public IEnumerable<T> GetSpatialDataPoints<T>(LoggingDataPointType a_loggingDataPointType, Dictionary<FilterCategory, List<string>> a_filterSelections) where T : SpatialLoggingDataPoint
    {
        var filterPredicate = GetFilterPredicate<T>(a_filterSelections);
        IEnumerable<T> dataPoints = GetSpatialDataPoints<T>(a_loggingDataPointType);
        return dataPoints.Where(filterPredicate);
    }

    public IEnumerable<T> GetSpatialDataPoints<T>(LoggingDataPointType a_loggingDataPointType) where T : SpatialLoggingDataPoint
    {
        IEnumerable<T> dataPoints = new List<T>();
        switch (a_loggingDataPointType)
        {
            case LoggingDataPointType.ParticipantTrackingData:
                dataPoints = (IEnumerable<T>)GetParticipantData();
                break;
            case LoggingDataPointType.SpatialAudioEvent:
                dataPoints = (IEnumerable<T>)GetAudioEventData();
                break;
            case LoggingDataPointType.InputActionEvent:
                dataPoints = (IEnumerable<T>)GetInputActionEventData();
                break;
            case LoggingDataPointType.CustomSpatialEvent:
                dataPoints = (IEnumerable<T>)GetCustomEventData();
                break;
            case LoggingDataPointType.TransformTrackingData:
                dataPoints = (IEnumerable<T>)GetTransformData();
                break;
            case LoggingDataPointType.OfflineAudioEvent:
                dataPoints = (IEnumerable<T>)GetOfflineAudioEventData();
                break;
            case LoggingDataPointType.OfflineSpatialAudioEvent:
                dataPoints = (IEnumerable<T>)GetOfflineSpatialAudioEventData();
                break;
            default:
                dataPoints = LoadDataPoints<T>(a_loggingDataPointType);
                break;
        }

        return dataPoints;
    }

    private static Func<T, string> GetGroupingPredicate<T>(GroupingCategory a_grouping) where T : SpatialLoggingDataPoint
    {
        return dataPoint =>
        {
            return a_grouping switch
            {
                GroupingCategory.TaskId => dataPoint.taskId,
                GroupingCategory.ParticipantId => dataPoint.participantId,
                _ => "",
            };
        };
    }

    private static Func<T, bool> GetFilterPredicate<T>(Dictionary<FilterCategory, List<string>> a_filterSelections) where T : SpatialLoggingDataPoint
    {
        return dataPoint => a_filterSelections[FilterCategory.Task].Contains(dataPoint.taskId)
                            && a_filterSelections[FilterCategory.Participant].Contains(dataPoint.participantId);
    }


    //public Dictionary<string, List<T>> GetDataPoints<T>(LoggingDataPointType loggingDataPointType, GroupingParameter a_groupingParameter, bool a_reload = false) where T : LoggingDataPoint
    //{
    //    List<ParticipantTrackingDataPoint> participantTrackingDataPoints = GetParticipantTrackingDataPoints(a_reload);
    //    if (TryGroupBy(participantTrackingDataPoints, a_groupingParameter, out var groupedCollection))
    //    {
    //        return groupedCollection;
    //    }

    //    return null;
    //}
    //public List<T> GetDataPoints<T>(bool a_reload) where T : LoggingDataPoint
    //{
    //    if (a_reload)
    //        return (List<T>)LoadParticipantTrackingDataPoints();
    //    else
    //        return m_participantTrackingDataPoints;
    //}

    public List<ParticipantTrackingDataPoint> GetParticipantData(bool a_reload = false)
    {
        if (a_reload || m_participantTrackingDataPoints == null)
        {
            m_participantTrackingDataPoints = LoadDataPoints<ParticipantTrackingDataPoint>(LoggingDataPointType.ParticipantTrackingData);
        }

        return m_participantTrackingDataPoints;
    }

    public List<TransformTrackingDataPoint> GetTransformData(bool a_reload = false)
    {
        if (a_reload || m_transformTrackingDataPoints == null)
        {
            m_transformTrackingDataPoints = LoadDataPoints<TransformTrackingDataPoint>(LoggingDataPointType.TransformTrackingData);
        }

        return m_transformTrackingDataPoints;
    }

    public List<InputActionEventDataPoint> GetInputActionEventData(bool a_reload = false)
    {
        if (a_reload || m_inputActionEventDataPoints == null)
        {
            m_inputActionEventDataPoints = LoadDataPoints<InputActionEventDataPoint>(LoggingDataPointType.InputActionEvent);
        }

        return m_inputActionEventDataPoints;
    }

    public List<SpatialAudioEventDataPoint> GetAudioEventData(bool a_reload = false)
    {
        if (a_reload || m_audioEventDataPoints == null)
        {
            m_audioEventDataPoints = LoadDataPoints<SpatialAudioEventDataPoint>(LoggingDataPointType.SpatialAudioEvent);
        }

        return m_audioEventDataPoints;
    }

    public List<OfflineAudioEventDataPoint> GetOfflineAudioEventData(bool a_reload = false)
    {
        if (a_reload || m_offlineAudioEventDataPoints == null)
        {
            m_offlineAudioEventDataPoints = LoadDataPoints<OfflineAudioEventDataPoint>(LoggingDataPointType.OfflineAudioEvent);
        }

        return m_offlineAudioEventDataPoints;
    }

    public List<OfflineSpatialAudioEventDataPoint> GetOfflineSpatialAudioEventData(bool a_reload = false)
    {
        if (a_reload || m_offlineSpatialAudioEventDataPoints == null)
        {
            m_offlineSpatialAudioEventDataPoints = LoadDataPoints<OfflineSpatialAudioEventDataPoint>(LoggingDataPointType.OfflineSpatialAudioEvent);
        }

        return m_offlineSpatialAudioEventDataPoints;
    }

    public List<CustomSpatialEventDataPoint> GetCustomEventData(bool a_reload = false)
    {
        if (a_reload || m_customEventDataPoints == null)
        {
            m_customEventDataPoints = LoadDataPoints<CustomSpatialEventDataPoint>(LoggingDataPointType.CustomSpatialEvent);
        }

        return m_customEventDataPoints;
    }

    public bool TryGroupBy<T>(List<T> a_listToGroup, Func<T, string> a_keySelector, out Dictionary<string, List<T>> a_groupedCollection) where T : LoggingDataPoint
    {
        try
        {
            a_groupedCollection = a_listToGroup.GroupBy(a_keySelector, v => v).ToDictionary(g => g.Key, g => g.ToList());

            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
        a_groupedCollection = null;
        return false;
    }

    public bool TryGetParticipantData(Func<ParticipantTrackingDataPoint, string> a_keySelector, out Dictionary<string, List<ParticipantTrackingDataPoint>> a_groupedCollection, bool a_reload = false)
    {
        var data = GetParticipantData(a_reload);
        if (TryGroupBy(data, a_keySelector, out a_groupedCollection))
            return true;

        return false;

    }

    public bool TryGetTransformsData(Func<TransformTrackingDataPoint, string> a_keySelector, out Dictionary<string, List<TransformTrackingDataPoint>> a_groupedCollection, bool a_reload = false)
    {
        var data = GetTransformData(a_reload);
        if (TryGroupBy(data, a_keySelector, out a_groupedCollection))
            return true;

        return false;

    }

    public List<string> GetStudyParticipants()
    {
        List<string> studyParticipants = new List<string>();
        foreach (var studySession in m_studyManager.ActiveStudy.studySessions)
        {
            foreach (var studyParticipant in studySession.studyParticipants)
            {
                studyParticipants.Add(studyParticipant.id);
            }
        }

        return studyParticipants;
    }

    /// <summary>
    /// Todo
    /// </summary>
    /// <returns></returns>
    public List<string> GetStudyTasks()
    {
        HashSet<string> eventTypes = new HashSet<string>();

        foreach (var studySession in m_studyManager.ActiveStudy.studySessions)
        {
            foreach (var studyParticipant in studySession.studyParticipants)
            {
                TextFileReader textFileReader = new TextFileReader(m_studyManager.ActiveStudy, studySession, studyParticipant);
                if (textFileReader.TryReadFromLoggingInfo(out var participantLoggingInfo))
                {
                    foreach (var taskId in participantLoggingInfo.taskIds)
                    {
                        eventTypes.Add(taskId);
                    }
                }
            }
        }

        return eventTypes.ToList();
    }

    /// <summary>
    /// Todo maybe return names of custom events too
    /// </summary>
    /// <returns></returns>
    public List<string> GetEventTypes()
    {
        return Enum.GetNames(typeof(LoggingDataPointType)).ToList();
    }

}
