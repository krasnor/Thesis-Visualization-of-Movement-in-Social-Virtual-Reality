using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class AvatarFollowStudyStage
{
    public StudyLookAtInteractable Interactable;
    public List<GameObject> Waypoints = new List<GameObject>();
    private List<LineSegment> PathSegmentsAvatar = new List<LineSegment>();
    private List<LineSegment> PathSegmentsUser = new List<LineSegment>();
    public float LastShortestDistanceToAvatarPath = 0;
    public float LastShortestDistanceToUserPath = 0;
    public float MaxDistanceToUserPath = 0;
    public float MaxDistanceToAvatarPath = 0;

    public bool IsStageActive = false;
    public bool HasPlayerArrivedAtInteractable = false;
    public DateTime StageActivatedActivatedStage = DateTime.MinValue;
    public DateTime StageFirstArrivedTargetInteractable = DateTime.MinValue;
    public DateTime StageActivatedTargetInteractable = DateTime.MinValue;
    public TimeSpan StageTimeTookArrive = TimeSpan.MinValue;
    public TimeSpan StageTimeTookActivate = TimeSpan.MinValue;

    public void SetupStage(GameObject a_startWaypoint, StudyLookAtInteractable a_startinteractable)
    {
        IsStageActive = false;
        HasPlayerArrivedAtInteractable = false;
        StageActivatedActivatedStage = DateTime.MinValue;
        StageFirstArrivedTargetInteractable = DateTime.MinValue;
        StageActivatedTargetInteractable = DateTime.MinValue;
        StageTimeTookArrive = TimeSpan.MinValue;
        StageTimeTookActivate = TimeSpan.MinValue;

        LastShortestDistanceToAvatarPath = 0;
        LastShortestDistanceToUserPath = 0;
        MaxDistanceToAvatarPath = 0;
        MaxDistanceToUserPath = 0;

        distanceToUserPathWhenfirstArrived = 0;
        maxdistanceToUserPathWhenfirstArrived = 0;
        distanceToUserPathWhenActivatedTargetInteractable = 0;
        maxdistanceToUserPathWhenTriggeredTarget = 0;

        PathSegmentsAvatar.Clear();

        for (int i = 0; i < Waypoints.Count; i++)
        {
            if (i == 0)
            {
                PathSegmentsAvatar.Add(new LineSegment() { A = a_startWaypoint.transform, B = Waypoints[i].transform });
            }
            else
            {
                PathSegmentsAvatar.Add(new LineSegment() { A = Waypoints[i - 1].transform, B = Waypoints[i].transform });
            }
        }

        PathSegmentsUser.Clear();
        Transform fromStandingArea = a_startinteractable.GetStandingAreaLocation();
        Transform toStandingArea = Interactable.GetStandingAreaLocation();

        for (int i = 0; i < Waypoints.Count; i++)
        {
            Transform nextWaypoint = Waypoints[i].transform;
            if (i + 1 >= Waypoints.Count)
            {
                //next Waypoint is also last
                nextWaypoint = toStandingArea;
            }

            if (i == 0)
            {
                Transform firstWaypoint = fromStandingArea.transform;
                PathSegmentsUser.Add(new LineSegment() { A = firstWaypoint, B = nextWaypoint });
            }
            else
            {
                PathSegmentsUser.Add(new LineSegment() { A = Waypoints[i - 1].transform, B = nextWaypoint });
            }
        }
    }

    public void DebugDrawStagePath(LineRenderer a_avatar, LineRenderer a_user)
    {
        if (a_avatar != null)
        {
            List<Vector3> points = new List<Vector3>();
            foreach (var pi in PathSegmentsAvatar)
            {
                points.Add(pi.A.position);
                points.Add(pi.B.position);
            }

            a_avatar.positionCount = points.Count;
            a_avatar.SetPositions(points.ToArray());
        }
        if (a_user != null)
        {
            List<Vector3> points = new List<Vector3>();
            foreach (var pi in PathSegmentsUser)
            {
                points.Add(pi.A.position);
                points.Add(pi.B.position);
            }

            a_user.positionCount = points.Count;
            a_user.SetPositions(points.ToArray());
        }
    }
    /// <summary>
    /// Enables Interactions of assigned Interactable. And Marks Stage as begun.
    /// </summary>
    public void ActivateStage(DateTime a_frameTime)
    {
        IsStageActive = true;
        StageActivatedActivatedStage = a_frameTime;
        Interactable.EnableInteractions(true);
    }

    public float distanceToUserPathWhenfirstArrived = 0;
    public float maxdistanceToUserPathWhenfirstArrived = 0;
    public void MarkUserArrivedAtInteractable(DateTime a_frameTime, Transform a_head)
    {
        StageFirstArrivedTargetInteractable = a_frameTime;
        StageTimeTookArrive = StageFirstArrivedTargetInteractable - StageActivatedActivatedStage;
        distanceToUserPathWhenfirstArrived = ShortestDistanceToUserPath(a_head);
        maxdistanceToUserPathWhenfirstArrived = Math.Max(distanceToUserPathWhenfirstArrived, MaxDistanceToUserPath);
    }

    public float distanceToUserPathWhenActivatedTargetInteractable = 0;
    public float maxdistanceToUserPathWhenTriggeredTarget = 0;
    public void MarkUserActivatedInteractable(DateTime a_frameTime, Transform a_head)
    {
        StageActivatedTargetInteractable = a_frameTime;
        StageTimeTookActivate = StageActivatedTargetInteractable - StageActivatedActivatedStage;
        distanceToUserPathWhenActivatedTargetInteractable = ShortestDistanceToUserPath(a_head);
        maxdistanceToUserPathWhenTriggeredTarget = Math.Max(distanceToUserPathWhenActivatedTargetInteractable, MaxDistanceToUserPath);

        IsStageActive = false;
    }


    //public void StartMeasurements(DateTime)
    //{
    //    if (!IsMeasurementInProgress)
    //    {
    //        StageStarted = DateTime.UtcNow;
    //        IsMeasurementInProgress = true;
    //    }
    //    else
    //    {
    //        Debug.LogWarning("Cannot start measurements. Measurements already in progress.");
    //    }
    //}



    /// <summary>
    /// Measures Distance of transform to the Stages Path(s).
    /// Only works when measurement was Stared and not yet ended.
    /// </summary>
    /// <param name="a_e"></param>
    public void UpdateCurrentAndMaxDistance(Transform a_e)
    {
        //Debug.Log("Taking Measurement");
        LastShortestDistanceToAvatarPath = ShortestDistanceToAvatarPath(a_e);
        LastShortestDistanceToUserPath = ShortestDistanceToUserPath(a_e);

        MaxDistanceToAvatarPath = Mathf.Max(LastShortestDistanceToAvatarPath, MaxDistanceToAvatarPath);
        MaxDistanceToUserPath = Mathf.Max(LastShortestDistanceToUserPath, MaxDistanceToUserPath);
    }



    //public void EndMeasurements()
    //{
    //    if (IsMeasurementInProgress)
    //    {
    //        StageEnded = DateTime.UtcNow;
    //        StageTimeTook = StageEnded - StageStarted;
    //        IsMeasurementInProgress = false;
    //    }
    //    else
    //    {
    //        Debug.LogWarning("Cannot end measurements as no measurements were started.");
    //    }
    //}

    public GameObject GetLastWaypoint()
    {
        return Waypoints.LastOrDefault();
    }

    public float ShortestDistanceToAvatarPath(Transform a_e)
    {
        return ShortestDistanceToPath(PathSegmentsAvatar, LineSegment.ProjectFlat(a_e));
    }

    public float ShortestDistanceToUserPath(Transform a_e)
    {
        return ShortestDistanceToPath(PathSegmentsUser, LineSegment.ProjectFlat(a_e));
    }

    public static float ShortestDistanceToPath(List<LineSegment> a_pathSegments, Vector2 a_e)
    {
        float shortestDist = float.MaxValue;

        foreach (var p in a_pathSegments)
        {
            float p_dist = p.ShortestDistance(a_e);
            if (p_dist < shortestDist)
            {
                shortestDist = p_dist;
            }
        }
        return shortestDist;
    }
}