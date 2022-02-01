using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;


/// <summary>
/// Idea: Networked Player is completly separate from the local Player.
/// 
/// The Network Player Object Mirrors the local Player by observing the XR Rig.
/// The Network Player itself is synchronized with all players.
/// 
/// Only rarely changing properties such as player color will be synced by Photon Custom Player Property.
/// </summary>
public class NetworkPlayer : MonoBehaviour
{
    private PhotonView photonView;
    [Header("Network Player Components")]
    public Transform head;
    public Transform body;
    public Transform leftHand;
    public Transform rightHand;

    public Animator animator_leftHand;
    public Animator animator_rightHand;

    public LineRenderer lineRenderer;
    public AvatarTraceManager traceManager;
    public AvatarColorManager colorManager;

    // [Space]
    [Header("To be Mirrored Components")]
    public string src_headPath = "Camera Offset/Main Camera"; // "Camera Offset/Main Camera/Avatar_Head_v1"
    public string src_bodyPath = "Camera Offset/Body";  // "Camera Offset/Body/Avatar_body_v2"
    public string src_lineRendererPath = "Camera Offset/Body";  // "Camera Offset/Body/Avatar_body_v2"
    public string src_avatarTraceManagerPath = "Camera Offset/Body";  // "Camera Offset/Body/Avatar_body_v2"
    public string src_leftHandPath = "Camera Offset/LeftHand Controller";
    public string src_rightHandPath = "Camera Offset/RightHand";
    public string src_avatarColorManagerPath = "";
    private Transform src_head;
    private Transform src_body;
    private Transform src_leftHand;
    private Transform src_rightHand;

    private LineRenderer src_lineRenderer;
    private AvatarTraceManager src_avatarTraceManager;
    private AvatarColorManager src_avatarColorManager;

    [Header("To be Mirrored By Custom Player Property")]
    public bool SyncPlayerColorByCustomPlayerProperty = true;
    public bool SyncPlayerHearingScopeByCustomPlayerProperty = true;
    private AvatarColorManagerPlayerPropertySync src_AvatarColorManagerPlayerPropertySync;

    //[Space]
    [Header("Debug")]
    public bool DebugShowThisLocalPlayer = false;
    //public bool DebugSyncByEvent = false;


    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
        src_AvatarColorManagerPlayerPropertySync = GetComponent<AvatarColorManagerPlayerPropertySync>();
        src_AvatarColorManagerPlayerPropertySync.enabled = SyncPlayerColorByCustomPlayerProperty;

        var hearableSync = GetComponent<HearingScopeProperySync>();
        hearableSync.enabled = SyncPlayerHearingScopeByCustomPlayerProperty;
    }

    // Start is called before the first frame update
    void Start()
    {

        XRRig rig = FindObjectOfType<XRRig>();
        src_head = rig.transform.Find(src_headPath);
        src_body = rig.transform.Find(src_bodyPath);
        src_lineRenderer = rig.transform.Find(src_lineRendererPath).GetComponent<LineRenderer>();
        src_avatarTraceManager = rig.transform.Find(src_avatarTraceManagerPath).GetComponent<AvatarTraceManager>();
        src_avatarColorManager = rig.transform.Find(src_avatarColorManagerPath).GetComponent<AvatarColorManager>();
        src_leftHand = rig.transform.Find(src_leftHandPath);
        src_rightHand = rig.transform.Find(src_rightHandPath);

        if (photonView.IsMine && !DebugShowThisLocalPlayer)
        {
            colorManager.SetInvisibilityState(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine)
        {
            // update positions
            MapPosition(head, src_head);
            MapPosition(body, src_body);
            MapPosition(leftHand, src_leftHand);
            MapPosition(rightHand, src_rightHand);

            // update hand animation
            UpdateHandAnimation(InputDevices.GetDeviceAtXRNode(XRNode.LeftHand), animator_leftHand);
            UpdateHandAnimation(InputDevices.GetDeviceAtXRNode(XRNode.RightHand), animator_rightHand);

            // update trace
            //if (!DebugSyncByEvent)
            //{
                UpdateLineTrace();
            //}
        }
        else
        {
            // other player
            // nothing to do, let Photon scripts do the updates
        }
    }

    void MapPosition(Transform target, Transform src)
    {
        target.position = src.position;
        target.rotation = src.rotation;
    }

    void UpdateHandAnimation(InputDevice targetDevice, Animator handAnimator)
    {
        if (targetDevice.TryGetFeatureValue(CommonUsages.trigger, out float triggerValue))
        {
            handAnimator.SetFloat("Pinch", triggerValue);
        }
        else
        {
            handAnimator.SetFloat("Pinch", 0);
        }
        if (targetDevice.TryGetFeatureValue(CommonUsages.grip, out float gripValue))
        {
            handAnimator.SetFloat("Flex", gripValue);
        }
        else
        {
            handAnimator.SetFloat("Flex", 0);
        }
    }

    void UpdateLineTrace()
    {
        // sync all points manually
        if (src_lineRenderer != null)
        {
            Vector3[] positions = new Vector3[src_lineRenderer.positionCount];
            src_lineRenderer.GetPositions(positions);
            SyncLinePoints(src_lineRenderer.positionCount, positions); // update local
        }
    }

    public void SyncLinePoints(int length, Vector3[] points)
    {
        // works
        // depending on network lag this approach may stutter
        // actual network sync will happen in PhotonSimpleAvatarTraceView.cs

        if (lineRenderer != null)
        {
            lineRenderer.positionCount = length;
            lineRenderer.SetPositions(points);
        }
    }

    private void OnLineSegmentAdded(object sender, AvatarTraceManager.LineSegmentAddedEventArgs e)
    {
        //if (DebugSyncByEvent)
        //{
        //    RPC_AddSegmentPoint(e.PositionFrom, e.PositionTo, e.CoordinateType);
        //}
    }

    #region RPCs

    //[PunRPC]
    //public void RPC_AddSegmentPoint(Vector3 a_from, Vector3 a_to, AvatarTraceManager.CoordinateType coordType)
    //{
    //    // may stutter less, but is not guarantued to be in sync for late joining players
    //    if (traceManager != null)
    //    {
    //        traceManager.AddLineTrailSegment(a_from, a_to, coordType);
    //    }
    //}
    #endregion
}
