using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptedVRAvatar : MonoBehaviour
{
    public enum OrderExecutionStatus
    {
        QUEUED,
        PROCESSING_MOVEMENT,
        REACHED_TARGET
    }


    private OrderExecutionStatus m_OrderExecutionStatus = OrderExecutionStatus.QUEUED;
    private float m_currentOrderFinishTime = 0f;

    public GameObject Avatar;
    public AvatarTraceManager TraceManager;
    public TeleportationType TP_Type = TeleportationType.TP_INSTANT;
    [Tooltip("Avatar Speed in m/s")]
    public float AvatarSpeed = 3.0f;

    [SerializeField]
    public int CurrentOrderIndex = 0;
    [SerializeField]
    public List<MovementOrder> Orders = new List<MovementOrder>();
    private bool m_reachedTargetAndStillProcessingRotation = false;

    public bool Debug_LogOrderInfo = false;

    public void ProcessNextOrder()
    {
        if(Debug_LogOrderInfo)
            Debug.Log($"ProcessNextOrder called Orders.Count: {Orders.Count} - Current {CurrentOrderIndex}");
        int nextIndex = CurrentOrderIndex + 1;

        if (nextIndex < Orders.Count)
        {
            CurrentOrderIndex = nextIndex;
            if(Debug_LogOrderInfo)
                Debug.Log("Switching to OrderIndex " + CurrentOrderIndex);

            m_currentOrderFinishTime = 0f;
            m_OrderExecutionStatus = OrderExecutionStatus.QUEUED;
            m_reachedTargetAndStillProcessingRotation = false;
        }
        else
        {
            Debug.Log("No more orders");
        }
    }

    private bool TryPeekNextOrder(out MovementOrder a_order)
    {
        int nextIndex = CurrentOrderIndex + 1;
        if (nextIndex < Orders.Count)
        {
            a_order = Orders[nextIndex];
            return true;
        }
        else
        {
            // no more oders
            a_order = null;
            return false;
        }
    }

    public void ResetAvatar()
    {
        Debug.Log("Resetting Avatar", this.gameObject);
        CurrentOrderIndex = 0;

        m_currentOrderFinishTime = 0f;
        m_OrderExecutionStatus = OrderExecutionStatus.QUEUED;
        m_reachedTargetAndStillProcessingRotation = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (Avatar == null)
        {
            throw new MissingComponentException("Avatar component is not set");
        }
        if (TraceManager == null)
        {
            throw new MissingComponentException("TraceManager component is not set");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Orders.Count != 0)
        {
            // Process CurrentOrder
            MovementOrder currentOrder = Orders[CurrentOrderIndex];
            switch (m_OrderExecutionStatus)
            {
                case OrderExecutionStatus.QUEUED:
                    // do first init stuff
                    m_OrderExecutionStatus = OrderExecutionStatus.PROCESSING_MOVEMENT;
                    //Debug.Log($"ScriptedVRAvatar: Queued MovementOrder[{CurrentOrderIndex}] - {Time.time}");

                    d_start_pos = Avatar.transform.position;
                    d_distance = Vector3.Distance(d_start_pos, currentOrder.MovementTarget.position);

                    // rotate into movment direction
                    RotateAvatarIntoMovementDirection(currentOrder);
                    break;
                case OrderExecutionStatus.PROCESSING_MOVEMENT:
                    DoMovementStep(Time.deltaTime, currentOrder);

                    if (HasReachedCurrentTarget(currentOrder.MovementTarget))
                    {
                        m_currentOrderFinishTime = Time.time;
                        m_OrderExecutionStatus = OrderExecutionStatus.REACHED_TARGET;
                    }

                    break;
                case OrderExecutionStatus.REACHED_TARGET:

                    //Debug.Log($"ScriptedVRAvatar: REACHED_TARGET MovementOrder[{CurrentOrderIndex}] - {Time.time}");
                    //Debug.Log($"ScriptedVRAvatar: Estimate {(d_distance/AvatarSpeed)} REAL: {(m_currentOrderFinishTime - d_TimeStarted)}");
                    if (!m_reachedTargetAndStillProcessingRotation)
                    {
                        switch (currentOrder.AvatarRotationBehavior)
                        {
                            case MovementOrder.RotationBehavior.AlignWithWaypoint:
                                Avatar.transform.rotation = currentOrder.MovementTarget.rotation;
                                m_reachedTargetAndStillProcessingRotation = true;
                                break;
                            case MovementOrder.RotationBehavior.AlignWithNextWaypoint:
                                if (TryPeekNextOrder(out var nextOrder))
                                {
                                    RotateAvatarIntoMovementDirection(nextOrder);
                                }
                                m_reachedTargetAndStillProcessingRotation = true;
                                break;
                            case MovementOrder.RotationBehavior.NONE:
                            default:
                                // do nothing
                                m_reachedTargetAndStillProcessingRotation = true;
                                break;
                        }
                    }

                    switch (currentOrder.MovementType)
                    {
                        case MovementOrder.MovmentOrderType.DELAY:
                            if (Time.time - m_currentOrderFinishTime > currentOrder.DelayTime)
                            {
                                if(Debug_LogOrderInfo)
                                    Debug.Log("ScriptedVRAvatar: Delay finished switching to next Order");
                                ProcessNextOrder();
                            }

                            break;
                        case MovementOrder.MovmentOrderType.INSTANT:
                            if(Debug_LogOrderInfo)
                                Debug.Log("ScriptedVRAvatar: instant switching to next Order");
                            ProcessNextOrder();
                            break;
                        case MovementOrder.MovmentOrderType.WAIT_UNTIL_NOTIFY:
                        default:
                            // do nothing
                            break;
                    }

                    break;
                default:
                    break;
            }
        }
    }

    #region Movement
    private float d_distance = 0f;
    private Vector3 d_start_pos;
    private float d_TimeStarted;

    public enum TeleportationType
    {
        TP_INSTANT,
        TP_TRACE,
        TP_CONTINUOUS,
    }

    public void ChangeTracedTeleportationProviderMode(TracedTeleportationProviderMode a_mode)
    {
        switch (a_mode)
        {
            case TracedTeleportationProviderMode.Instant:
                TraceManager.LineRenderer.enabled = false;
                TP_Type = TeleportationType.TP_INSTANT;
                break;
            case TracedTeleportationProviderMode.Trace_Particles:
                Debug.LogWarning("Mode Trace_Particles is not supported by ScriptedVRAvatar. Using " + TeleportationType.TP_TRACE + " instead");
                TraceManager.LineRenderer.enabled = true;
                TP_Type = TeleportationType.TP_TRACE;
                break;
            case TracedTeleportationProviderMode.Trace_Line:
                TraceManager.LineRenderer.enabled = true;
                TP_Type = TeleportationType.TP_TRACE;
                break;
            case TracedTeleportationProviderMode.Continuous:
                TraceManager.LineRenderer.enabled = false;
                TP_Type = TeleportationType.TP_CONTINUOUS;
                break;
            default:
                Debug.LogWarning("Mode " + a_mode + " is not supported by ScriptedVRAvatar. Using " + TeleportationType.TP_INSTANT + " instead");
                TraceManager.LineRenderer.enabled = false;
                TP_Type = TeleportationType.TP_INSTANT;
                break;
        }
    }

    private void DoMovementStep(float a_deltaTime, MovementOrder a_currentOrder)
    {
        Transform currentPosition = Avatar.transform;
        Transform destination = a_currentOrder.MovementTarget;
        float debug_additionalTraceHeightOffset = 0; // only for debug! use offset defined in AvatarTraceManager!
        switch (TP_Type)
        {
            case TeleportationType.TP_INSTANT:
                TraceManager.LineRenderer.enabled = false;
                MoveAvatarToPoint(destination);
                break;
            case TeleportationType.TP_TRACE:
                Vector3 pos_trace_from = currentPosition.position;
                Vector3 pos_trace_to = destination.position; // To position
                pos_trace_from.y += debug_additionalTraceHeightOffset;
                pos_trace_to.y += debug_additionalTraceHeightOffset;

                TraceManager.LineRenderer.enabled = true;
                TraceManager.AddLineTrailSegment(pos_trace_from, pos_trace_to);

                MoveAvatarToPoint(destination);
                break;
            case TeleportationType.TP_CONTINUOUS:
                TraceManager.LineRenderer.enabled = false;

                Vector3 newPos = Vector3.MoveTowards(currentPosition.position, destination.position, AvatarSpeed * a_deltaTime);
                MoveAvatarToPoint(newPos);
                break;
            default:
                Debug.LogWarning("Unhandled TeleportationType " + TP_Type);
                break;
        }
    }
    void MoveAvatarToPoint(Transform t_pos)
    {
        MoveAvatarToPoint(t_pos.position);
    }
    void MoveAvatarToPoint(Vector3 t_pos)
    {
        Avatar.transform.position = t_pos;
    }

    void RotateAvatarIntoMovementDirection(MovementOrder a_currentOrder)
    {
        RotateAvatarToPosition(a_currentOrder.MovementTarget.position);
    }

    void RotateAvatarToPosition(Vector3 a_targetPosition)
    {
        Vector3 targetDirection = a_targetPosition - Avatar.transform.position;
        Avatar.transform.rotation = Quaternion.LookRotation(targetDirection);
    }

    private bool HasReachedCurrentTarget(Transform a_currentTargetPos)
    {
        if (Avatar.transform.position == a_currentTargetPos.position)
            return true;
        return false;
    }
    #endregion
}