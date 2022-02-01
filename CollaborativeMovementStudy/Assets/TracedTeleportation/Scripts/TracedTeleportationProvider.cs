using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.XR.Interaction.Toolkit;


public enum TracedTeleportationProviderMode
{
    Instant,
    Trace_Line,
    Trace_Particles,
    Continuous,
}

public class TracedTeleportationProvider : TeleportationProvider
{
    protected Queue<TeleportRequest> TeleportationRequestQueue;
    protected Vector3 m_teleportCameraOrigin;
    protected float m_teleportCameraHeight;
    protected Vector3 m_teleportFloorOrigin;
    //protected Vector3 teleportFloorOrigin2;

    public TracedTeleportationProviderMode MovementMode = TracedTeleportationProviderMode.Instant;

    [Header("Trace Settings")]
    public AvatarTraceManager TraceManager;
    /// <summary>
    /// Used for debugging.
    /// </summary>
    private float m_floorClipOffset = 0f; // 0.02f;

    [Header("Continuous Movement Settings")]
    [SerializeField]
    public bool UseConstantMovementSpeed = true;
    public ContinuousMovementRequestQueueBehaviour ContinuousRequestQueueBehaviour = ContinuousMovementRequestQueueBehaviour.BLOCK_REQUESTS;
    public float ConstantMovementSpeed = 3;
    public float FixedMovementTime = 1;
    private float m_constMovement_curr_interpolationStep = 0f;


    public enum ContinuousMovementRequestQueueBehaviour
    {
        QUEUE_REQUESTS, BLOCK_REQUESTS, MOST_RECENT
    }

    private float dbg_timeContMoveBegun = 0;
    private float dbg_timeContMoveArrived = 0;


    public virtual void ChangeTracedTeleportationProviderMode(TracedTeleportationProviderMode a_mode)
    {
        MovementMode = a_mode;
    }

    /// <summary>
    /// This function will queue a teleportation request within the provider.
    /// </summary>
    /// <param name="teleportRequest">The teleportation request to queue.</param>
    /// <returns>Returns <see langword="true"/> if successfully queued. Otherwise, returns <see langword="false"/>.</returns>
    public override bool QueueTeleportRequest(TeleportRequest teleportRequest)
    {
        // TeleportationProvider.validRequest: is set to true while processing a request
        if (validRequest  && MovementMode == TracedTeleportationProviderMode.Continuous && ContinuousRequestQueueBehaviour == ContinuousMovementRequestQueueBehaviour.BLOCK_REQUESTS)
        {
            return true;
        }

        // TODO encoded TP-Mode in the request? Note TeleportRequest is a struct, so enheritance not possibly. 
        // Wrapping would break this inherited method -> not directly compatible with the standard implementation of XRInteractionToolkit
        TeleportationRequestQueue.Enqueue(teleportRequest);
        return true;
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        TeleportationRequestQueue = new Queue<TeleportRequest>();
        m_teleportCameraOrigin = new Vector3();
        m_teleportCameraHeight = 0f;
        m_teleportFloorOrigin = new Vector3();

        m_constMovement_curr_interpolationStep = 0f;
    }

    protected override void Update()
    {
        var xrRig = system.xrRig;
        if (xrRig == null)
            return;

        // no request processing && Requests in Queue left
        bool canProcessNextRequest = !validRequest && TeleportationRequestQueue.Count > 0;
        // A request is in progress, but another is queued up already -> abort old request and switch to new request
        bool continuousRequestOrderOverride = TracedTeleportationProviderMode.Continuous == MovementMode && validRequest && ContinuousRequestQueueBehaviour == ContinuousMovementRequestQueueBehaviour.MOST_RECENT && TeleportationRequestQueue.Count > 0;

        if (canProcessNextRequest || continuousRequestOrderOverride)
        {
            currentRequest = TeleportationRequestQueue.Dequeue();
            m_teleportCameraOrigin = xrRig.cameraGameObject.transform.position;
            m_teleportCameraHeight = xrRig.cameraInRigSpaceHeight;
            m_teleportFloorOrigin = new Vector3(m_teleportCameraOrigin.x, m_teleportCameraOrigin.y - m_teleportCameraHeight, m_teleportCameraOrigin.z);
            //teleportFloorOrigin = new Vector3(teleportCameraOrigin.x, xrRig.transform.position.y, teleportCameraOrigin.z);

            m_constMovement_curr_interpolationStep = 0f;
            dbg_timeContMoveBegun = Time.time;
            validRequest = true;
        }

        if (!validRequest || !BeginLocomotion())
            return;

        // ## execute movement
        switch (currentRequest.matchOrientation)
        {
            case MatchOrientation.WorldSpaceUp:
                xrRig.MatchRigUp(Vector3.up);
                break;
            case MatchOrientation.TargetUp:
                xrRig.MatchRigUp(currentRequest.destinationRotation * Vector3.up);
                break;
            case MatchOrientation.TargetUpAndForward:
                xrRig.MatchRigUpCameraForward(currentRequest.destinationRotation * Vector3.up, currentRequest.destinationRotation * Vector3.forward);
                break;
            case MatchOrientation.None:
                // Change nothing. Maintain current rig rotation.
                break;
            default:
                Assert.IsTrue(false, $"Unhandled {nameof(MatchOrientation)}={currentRequest.matchOrientation}.");
                break;
        }

        var heightAdjustment = xrRig.rig.transform.up * xrRig.cameraInRigSpaceHeight;

        var cameraDestination = currentRequest.destinationPosition + heightAdjustment;

        // head positions
        Vector3 pos_camera_from = m_teleportCameraOrigin;
        Vector3 pos_camera_to = cameraDestination;

        // floor positions
        Vector3 pos_floor_from = m_teleportFloorOrigin;
        Vector3 pos_floor_to = currentRequest.destinationPosition;

        switch (MovementMode)
        {
            case TracedTeleportationProviderMode.Instant:
                xrRig.MoveCameraToWorldLocation(cameraDestination);
                validRequest = false;
                break;
            case TracedTeleportationProviderMode.Trace_Line:
                pos_floor_from.y += m_floorClipOffset;
                pos_floor_to.y += m_floorClipOffset;

                TraceManager?.AddLineTrailSegment(pos_floor_from, pos_floor_to);

                xrRig.MoveCameraToWorldLocation(cameraDestination);
                validRequest = false;
                break;
            case TracedTeleportationProviderMode.Trace_Particles:
                Debug.LogWarning($"{nameof(TracedTeleportationProviderMode)}={TracedTeleportationProviderMode.Trace_Particles} not implemented. Teleporting instead");

                //traceManager.CreateParticleTrailSegment(pos_floor_from, pos_floor_to, xrRig.cameraGameObject.transform.rotation);
                xrRig.MoveCameraToWorldLocation(cameraDestination);
                validRequest = false;
                break;
            case TracedTeleportationProviderMode.Continuous:
                float distance = Vector3.Distance(cameraDestination, pos_camera_from);
                if (!UseConstantMovementSpeed)
                {
                    // ## Variant 1 - Reach destination in a set MovementTime (e.g. From A to B in 5 seconds)
                    if (m_constMovement_curr_interpolationStep < 1)
                    {
                        // move while distance in MovementSpeed (e.g. one second)
                        m_constMovement_curr_interpolationStep += (1 / FixedMovementTime) * Time.deltaTime; // sums up to one
                        var newPos = Vector3.Lerp(pos_camera_from, cameraDestination, m_constMovement_curr_interpolationStep);
                        xrRig.MoveCameraToWorldLocation(newPos);
                    }
                    else
                    {
                        dbg_timeContMoveArrived = Time.time;
                        //Debug.Log("CONSTANT TRAVEL TIME: " + "Distance: " + distance + " TravelTime Estimate: " + FixedMovementTime + " Real: " + (dbg_timeContMoveArrived - dbg_timeContMoveBegun));
                        validRequest = false;
                    }
                }
                else
                {
                    // ## Variant 2 - Move at constant speed (e.g. From A to B in 5 meters per second)
                    if (m_constMovement_curr_interpolationStep < 1)
                    {
                        float shouldBeTime = distance / ConstantMovementSpeed; // in m/s

                        // Why this way?
                        // user can move Head around -> this would alter current position -> this can be a problem in Vector3.MoveTowards -> travel time could be faster or slower
                        // thus interpolate movement 
                        //curr_interpolationStep = (curr_interpolationStep * distance + MovementSpeed * Time.deltaTime) / shouldBeTime; // works for speed = 1

                        m_constMovement_curr_interpolationStep += (1 / shouldBeTime) * Time.deltaTime;

                        Vector3 newPosition = Vector3.Lerp(pos_camera_from, cameraDestination, m_constMovement_curr_interpolationStep);

                        xrRig.MoveCameraToWorldLocation(newPosition);
                    }
                    else
                    {
                        // reached target destination
                        dbg_timeContMoveArrived = Time.time;
                        //Debug.Log("CONSTANT SPEED: " + "Distance: " + distance + " TravelTime Estimate: " + (distance / ConstantMovementSpeed) + " Real: " + (dbg_timeContMoveArrived - dbg_timeContMoveBegun));
                        validRequest = false;
                    }
                }

                break;
            default:
                Assert.IsTrue(false, $"Unhandled {nameof(TracedTeleportationProviderMode)}={MovementMode}.");
                validRequest = false;
                break;
        }

        EndLocomotion();
    }
}
