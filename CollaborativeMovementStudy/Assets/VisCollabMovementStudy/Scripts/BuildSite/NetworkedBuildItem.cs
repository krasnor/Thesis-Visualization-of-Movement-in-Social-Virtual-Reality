using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Assertions;

[RequireComponent(typeof(Rigidbody))]
public class NetworkedBuildItem : XRGrabInteractable, IPunObservable
{
    [Space]
    [Header("NetworkedBuildItem Settings")]
    public PhotonView photonView;

    private Rigidbody m_rigidbody;
    private LayerMask m_defaultLayerMask; // initial state of XRBaseInteractable.interactionLayerMask
    private LayerMask m_deactivatedStateLayerMask = 0; // 0 = Nothing; -1 = Everthing

    public int BuildModelId = 0;
    public int BuildPartId = 0;
    public bool IsInteractable = false; // synced over network

    protected bool m_hasUnappliedNetworkData = false;
    private bool r_IsInteractable;

    protected override void Awake()
    {
        base.Awake();
        m_defaultLayerMask = base.interactionLayerMask;

        photonView = GetComponent<PhotonView>();
        m_rigidbody = GetComponent<Rigidbody>();

        if (photonView == null)
            throw new MissingReferenceException("Could not find photonView Component");
        Assert.IsTrue(photonView.OwnershipTransfer == OwnershipOption.Takeover, "Script only configured for OwnershipOption.Takeover");
    }

    protected virtual void FixedUpdate()
    {
        if (!this.photonView.IsMine && m_hasUnappliedNetworkData)
        {
            IsInteractable = r_IsInteractable;

            m_hasUnappliedNetworkData = false;
        }

        if (IsInteractable)
        {
            if (m_rigidbody.isKinematic)
            {
                m_rigidbody.isKinematic = false;
            }
            base.interactionLayerMask = m_defaultLayerMask;
        }
        else
        {
            if (!m_rigidbody.isKinematic)
            {
                // Don't change rigidbody.velocity uneccesary, 
                // as sleep state would then be toggling each frame between 'asleep' and 'awake' 
                // (as changes to velocity or angularVelocity trigger other updates to the rigidbody)

                m_rigidbody.isKinematic = true;
                // Set them to zero, just to be 100% sure. So there won't be any problems when unstoring the item.
                m_rigidbody.velocity = Vector3.zero;
                m_rigidbody.angularVelocity = Vector3.zero;
            }
            base.interactionLayerMask = m_deactivatedStateLayerMask;
        }
    }

    protected override void OnSelectEntering(SelectEnterEventArgs interactor)
    {
        photonView.RequestOwnership();
        base.OnSelectEntering(interactor);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(IsInteractable);
        }
        else
        {
            r_IsInteractable = (bool)stream.ReceiveNext();
            m_hasUnappliedNetworkData = true;
        }
    }
}
