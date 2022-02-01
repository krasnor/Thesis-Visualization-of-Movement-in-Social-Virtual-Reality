using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public enum TeleportationType
{
    INSTANT,
    MOVE,
    TRACE,
    TRACE_PARTICLES
}

public class DemoTeleport : MonoBehaviour
{
    public GameObject[] markers;

    public int currentMarkerTargetIdx = 0;
    public float MovementSpeed = 3;
    public float TravelTimeWhenFixed = 1;
    public bool ConstantMovement = true;
    public float curr_interpolationStep = 0;
    //public int TraceHeight = 1;
    public float TraceHeightOffset = 0;
    //private LineRenderer lineRenderer;




    //private float AvatarHeight = 1f; // FIXME

    private TeleportationType currentTeleportationType;
    private TeleportRequest currentRequest; // TODO expand for TeleportationType
    private bool isCurrentRequestValid = false;

    public AvatarTraceManager traceManager;
    [Space]
    public InputAction dbg_InstantInstantAction;
    public InputAction dbg_InstantMoveAction;
    public InputAction dbg_InstantTraceAction;
    public InputAction dbg_InstantTraceParticleAction;

    // Start is called before the first frame update
    void Start()
    {
        dbg_InstantInstantAction.Enable();
        dbg_InstantMoveAction.Enable();
        dbg_InstantTraceAction.Enable();
        dbg_InstantTraceParticleAction.Enable();

        dbg_InstantInstantAction.performed += InstantTrigger;
        dbg_InstantMoveAction.performed += MoveTrigger;
        dbg_InstantTraceAction.performed += TraceTrigger;
        dbg_InstantTraceParticleAction.performed += TraceParticleTrigger;

        //lineRenderer = gameObject.GetComponent<LineRenderer>();
        //lineRenderer.enabled = false;

        SetAllMarkersInvisible();

        if (markers.Length == 0)
            return;

        //calcAvatarHeightDiff(markers[0].transform.position);
        MoveToPoint(markers[0].transform);
    }

    //void calcAvatarHeightDiff(Vector3 groundPos)
    //{
    //    //var posAvatar = this.gameObject.transform.position;
    //    //AvatarHeight = posAvatar.y - groundPos.y;
    //    AvatarHeight = 1.0f; // FIXME
    //}

    private void SetAllMarkersInvisible()
    {
        foreach(GameObject marker in markers){
            marker.GetComponent<Renderer>().enabled = false;
        }
    }

    private void InstantTrigger(InputAction.CallbackContext obj)
    {
        //Debug.Log("InstantTrigger");
        DoTeleport(TeleportationType.INSTANT);
    }
    private void MoveTrigger(InputAction.CallbackContext obj)
    {
        //Debug.Log("MoveTrigger");
        DoTeleport(TeleportationType.MOVE);
    }
    private void TraceTrigger(InputAction.CallbackContext obj)
    {
        //Debug.Log("TraceTrigger");
        DoTeleport(TeleportationType.TRACE);
    }
    private void TraceParticleTrigger(InputAction.CallbackContext obj)
    {
        //Debug.Log("TraceParticleTrigger");
        DoTeleport(TeleportationType.TRACE_PARTICLES);
    }

    // Update is called once per frame
    void Update()
    {
        //transform.position += new Vector3(1 * Time.deltaTime, 0, 0);

        if (isCurrentRequestValid)
        {
            int previousMarkerTargetIdx = (currentMarkerTargetIdx + (markers.Length-1) ) % markers.Length;
            Vector3 destination = markers[currentMarkerTargetIdx].transform.position;
            Vector3 start = markers[previousMarkerTargetIdx].transform.position;
            float distance = Vector3.Distance(start, destination);
            Vector3 direction = (destination - start).normalized;

            switch (currentTeleportationType)
            {
                case TeleportationType.INSTANT:
                    MoveToPoint(markers[currentMarkerTargetIdx].transform);
                    isCurrentRequestValid = false;
                    break;
                case TeleportationType.MOVE:
                    //transform.position += new Vector3(MovementSpeed * Time.deltaTime, 0, 0);

                    if (!ConstantMovement)
                    {
                        // ## Variant 1 - Reach destination in MovementTime
                        if (curr_interpolationStep < 1)
                        {
                            curr_interpolationStep +=  (1/TravelTimeWhenFixed) * Time.deltaTime; // sums up to one
                            var newPos = Vector3.Lerp(start, destination, curr_interpolationStep);
                            MoveToPoint(newPos);
                        }
                        else
                        {
                            dbg_timeContMoveArrived = Time.time;
                            Debug.Log("CONSTANT TRAVEL TIME: " + "Distance: " + distance + " TravelTime Estimate: " + TravelTimeWhenFixed + " Real: " + (dbg_timeContMoveArrived - dbg_timeContMoveBegun));
                            isCurrentRequestValid = false;
                        }
                    }
                    else
                    {
                        // ## Variant 2 - MoveAtConstant speed
                        if (curr_interpolationStep < 1)
                        {
                            float shouldBeTime = distance / MovementSpeed; // in m/s

                            //Vector3 step = direction * MovementSpeed * Time.deltaTime;
                            //Vector3 newPos1 = currPos + step;                                              
                            //Vector3 newPos2 = Vector3.MoveTowards(currPos, destination, MovementSpeed * Time.deltaTime); // recommended

                            // Why this way?
                            // user can move Head around -> this would alter current position -> this can be a problem in Vector3.MoveTowards -> travel time could be faster or slower
                            // thus interpolate movement 
                            //curr_interpolationStep = (curr_interpolationStep * distance + MovementSpeed * Time.deltaTime) / shouldBeTime; // works for speed = 1

                            curr_interpolationStep += (1 / shouldBeTime) * Time.deltaTime;

                            Vector3 newPos3 = Vector3.Lerp(start, destination, curr_interpolationStep);

                            MoveToPoint(newPos3);
                        }
                        else
                        {
                            dbg_timeContMoveArrived = Time.time;
                            Debug.Log("CONSTANT SPEED: "+"Distance: " + distance+ " TravelTime Estimate: " + (distance / MovementSpeed) + " Real: " + (dbg_timeContMoveArrived - dbg_timeContMoveBegun));
                            isCurrentRequestValid = false;
                        }
                    }

                    break;
                case TeleportationType.TRACE:
                    Vector3 pos_from = markers[previousMarkerTargetIdx].transform.position;
                    Vector3 pos_to = markers[currentMarkerTargetIdx].transform.position; // To position
                    pos_from.y += TraceHeightOffset;
                    pos_to.y += TraceHeightOffset;

                    traceManager.LineRenderer.enabled = true;
                    traceManager.AddLineTrailSegment(pos_from, pos_to);

                    MoveToPoint(markers[currentMarkerTargetIdx].transform);
                    isCurrentRequestValid = false;
                    break;
                case TeleportationType.TRACE_PARTICLES:

                    //var pLineStart = markers[previousMarkerTargetIdx].transform.position;
                    //var pLineEnd = markers[currentMarkerTargetIdx].transform.position; // To position
                    //pLineStart.y += TraceHeightOffset;
                    //pLineEnd.y += TraceHeightOffset;
                    //traceManager.CreateParticleTrailSegment(pLineStart, pLineEnd, gameObject.transform.rotation);
                    Debug.LogWarning("TeleportationType.TRACE_PARTICLES not implemented");

                    MoveToPoint(markers[currentMarkerTargetIdx].transform);
                    isCurrentRequestValid = false;
                    break;
            }
        }
        // do other update stuff
    }

    float dbg_timeContMoveBegun = 0;
    float dbg_timeContMoveArrived = 0;

    void DoTeleport(TeleportationType tp_type = TeleportationType.INSTANT)
    {
        if (markers.Length == 0)
            return;

        curr_interpolationStep = 0f;
        dbg_timeContMoveBegun = Time.time;
        currentMarkerTargetIdx = (currentMarkerTargetIdx + 1) % markers.Length;
        currentTeleportationType = tp_type;
        traceManager.LineRenderer.enabled = false;

        // Rotate the forward vector towards the target direction by one step
        Vector3 tpos = markers[currentMarkerTargetIdx].transform.position;
        //tpos.y += TraceHeight;
        Vector3 targetDirection = tpos - transform.position;

        //float singleStep = 1.0f * Time.deltaTime;
        //Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, 70, 1.0f); // FIXME
        transform.rotation = Quaternion.LookRotation(targetDirection);

        isCurrentRequestValid = true;
    }

    void MoveToPoint(Transform t_pos)
    {
        MoveToPoint(t_pos.position);
    }
    void MoveToPoint(Vector3 t_pos)
    {
        var posAvatar = this.gameObject.transform.position;
        var pos1 = t_pos;

        //pos1.y += AvatarHeight;
        this.gameObject.transform.position = pos1;
    }

}
