using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class ScriptedAvatarRoute
{
    public StudyLookAtInteractable StartInteractable;
    public GameObject StartWaypoint;

    public List<AvatarFollowStudyStage> Stages = new List<AvatarFollowStudyStage>();

    public List<MovementOrder> CreateMovementOrderListFromStages(float a_movementWaitDelay)
    {
        if (StartWaypoint == null)
            return new List<MovementOrder>();
        List<MovementOrder> orders = new List<MovementOrder>();

        //add first Order
        orders.Add(
            new MovementOrder(StartWaypoint.transform, MovementOrder.MovmentOrderType.WAIT_UNTIL_NOTIFY, 0, MovementOrder.RotationBehavior.AlignWithWaypoint)
            );

        foreach (var stage in Stages)
        {
            for (int i = 0; i < stage.Waypoints.Count; i++)
            {
                if (i == stage.Waypoints.Count - 1)
                {
                    // last of stage -> wait until trigger
                    orders.Add(
                        new MovementOrder(stage.Waypoints[i].transform, MovementOrder.MovmentOrderType.WAIT_UNTIL_NOTIFY, 0, MovementOrder.RotationBehavior.AlignWithWaypoint)
                    );
                }
                else
                {
                    // on route
                    orders.Add(
                        new MovementOrder(stage.Waypoints[i].transform, MovementOrder.MovmentOrderType.DELAY, a_movementWaitDelay, MovementOrder.RotationBehavior.AlignWithNextWaypoint)
                    );

                }
            }
        }

        return orders;
    }
    public List<MovementOrder> CreateMovementOrderListFromStagesForUser(float a_movementWaitDelay)
    {
        if (StartInteractable == null)
            return new List<MovementOrder>();
        List<MovementOrder> orders = new List<MovementOrder>();

        //add first order (interactable)
        orders.Add(
            new MovementOrder(StartInteractable.GetStandingAreaLocation(), MovementOrder.MovmentOrderType.WAIT_UNTIL_NOTIFY, 0, MovementOrder.RotationBehavior.AlignWithWaypoint)
            );

        foreach (var stage in Stages)
        {
            for (int i = 0; i < stage.Waypoints.Count; i++)
            {
                if (i == stage.Waypoints.Count - 1)
                {
                    // last of stage -> wait until trigger
                    orders.Add(
                        new MovementOrder(stage.Interactable.GetStandingAreaLocation(), MovementOrder.MovmentOrderType.WAIT_UNTIL_NOTIFY, 0, MovementOrder.RotationBehavior.AlignWithWaypoint)
                    );
                }
                else
                {
                    // on route
                    orders.Add(
                        new MovementOrder(stage.Waypoints[i].transform, MovementOrder.MovmentOrderType.DELAY, a_movementWaitDelay, MovementOrder.RotationBehavior.AlignWithNextWaypoint)
                    );

                }
            }
        }

        return orders;
    }


    public void InterconnectStages()
    {
        InterconnectStages(StartWaypoint, Stages);
    }

    private void InterconnectStages(GameObject a_startWaypoint, List<AvatarFollowStudyStage> a_stages)
    {
        // interconnect stages
        GameObject lastWaypoint = StartWaypoint;
        StudyLookAtInteractable lastInteractable = StartInteractable;
        foreach (var stage in a_stages)
        {
            stage.SetupStage(lastWaypoint, lastInteractable);
            lastWaypoint = stage.GetLastWaypoint();
            lastInteractable = stage.Interactable;
        }
    }
}

[System.Serializable]
public class RouteStatistic
{
    [Header("Info")]
    public bool projectedFlat = false;
    public int RouteId = -1; 
    public double TotalPathLength = -1;
    public double TotalTimeNeeded = -1;
    public double TimeNeededAtConstantTravelSpeed = 3;

    [Header("Detail")]
    public double CountDelays = 0;
    public int PathSegmentCount = -1; 
    public double TotalMovmentDelayTime = -1;
    public double SpeedMetersPerSecond = 3;
    public double[] PathSegmentLength;
    public double[] PathSegmentTime;
    public Vector3[] Waypoints;

    public string segmentsStr = "";
    public double sd = -1;



    public void CreateFromOrderList(List<MovementOrder> a_orders, int a_routeId, double a_movementDelay, bool a_projectFlat = true)
    {
        projectedFlat = a_projectFlat;
        RouteId = a_routeId;
        List<Vector3> waypoints = new List<Vector3>();
        foreach (var order in a_orders)
        {
            var waypointPos = order.MovementTarget.position;
            if(projectedFlat)
                waypointPos.y = 0;
            waypoints.Add(waypointPos);
            if (order.MovementType == MovementOrder.MovmentOrderType.DELAY)
            {
                CountDelays++;
            }
        }

        TotalMovmentDelayTime = a_movementDelay * CountDelays;
        PathSegmentCount = waypoints.Count - 1;
        Waypoints = waypoints.ToArray();

        PathSegmentLength = new double[PathSegmentCount];

        for (int i = 1; i < waypoints.Count; i++)
        {
            var a = waypoints[i - 1];
            var b = waypoints[i];
            PathSegmentLength[i - 1] = Vector3.Distance(a, b);
        }

        TotalPathLength = PathSegmentLength.Sum();
        TimeNeededAtConstantTravelSpeed = TotalPathLength / SpeedMetersPerSecond;

        TotalTimeNeeded = TimeNeededAtConstantTravelSpeed + TotalMovmentDelayTime;
        segmentsStr = string.Join("\n", PathSegmentLength);

        double average = PathSegmentLength.Average();
        double sumOfSquaresOfDifferences = PathSegmentLength.Select(val => (val - average) * (val - average)).Sum();
        sd = Mathf.Sqrt((float) (sumOfSquaresOfDifferences / TotalPathLength));
        sd = standardDeviation(PathSegmentLength);
    }

    static double standardDeviation(IEnumerable<double> sequence)
    {
        double result = 0;

        if (sequence.Any())
        {
            double average = sequence.Average();
            double sum = sequence.Sum(d => Mathf.Pow((float) (d - average), 2));
            result = Mathf.Sqrt((float)((sum) / (sequence.Count() - 1)));
        }
        return result;
    }
}