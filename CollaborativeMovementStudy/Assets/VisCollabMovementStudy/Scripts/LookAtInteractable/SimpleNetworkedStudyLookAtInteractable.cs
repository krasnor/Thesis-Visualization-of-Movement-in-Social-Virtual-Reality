using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Synchronizes only Color of an StudyLookAtInteractable.
/// Only client who owns the object can interact with the interactable.
/// Thus if this variant is used, interactable must be reset when owner changes!, as only color was synchronized.
/// </summary>
public class SimpleNetworkedStudyLookAtInteractable : StudyLookAtInteractable, IPunObservable
{
    public PhotonView photonView;

    private bool r_hasValidGaze = false;
    private bool r_alreadyTriggerd = false;

    protected override void Awake()
    {
        base.Awake();
        photonView = GetComponent<PhotonView>();
        if (photonView == null)
            throw new MissingComponentException("photonView Component is missing");
    }

    public bool baseUpdateCalled = false;

    protected override void Update()
    {
        if (photonView.IsMine)
        {
            baseUpdateCalled = true;
            // only do update when user is the owner 
            // -> prevent color changes or trigger of events on other clients
            base.Update();
        }
        else
        {
            baseUpdateCalled = false;
            //m_delayTriggered = r_hasValidGaze;
            //m_alreadyTriggered = r_alreadyTriggerd;
            //m_delayTriggered = -;

            // apply color
            // do color sync over booleans, as this is fast & requires not much bandwidth
            if (r_alreadyTriggerd)
            {
                this.SetColor(this.OnTriggeredMaterial);
            }
            else if (r_hasValidGaze)
            {
                this.SetColor(this.ValidGazeMaterial);
            }
            else
            {
                this.SetColor(this.BaseMaterial);
            }
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            bool hasValidGaze = this.HasValidGaze;
            bool alreadyTriggerd = this.IsAlreadyTriggered;
            //bool delayTriggerd = this.DelayTriggered;
            stream.SendNext(hasValidGaze);
            stream.SendNext(alreadyTriggerd);
            //stream.SendNext(delayTriggerd);
        }
        else
        {
            r_hasValidGaze = (bool)stream.ReceiveNext();
            r_alreadyTriggerd = (bool)stream.ReceiveNext();
            //r_delayTriggerd = (bool)stream.ReceiveNext();
        }
    }
}
