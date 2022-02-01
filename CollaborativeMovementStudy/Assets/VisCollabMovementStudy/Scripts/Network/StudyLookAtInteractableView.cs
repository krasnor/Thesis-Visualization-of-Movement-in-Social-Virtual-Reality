using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Synchronizes Color of an StudyLookAtInteractable.
/// Thus if this variant is used, interactable must be reset when owner changes!, as only color was synchronized.
/// </summary>
[RequireComponent(typeof(StudyLookAtInteractable))]
public class StudyLookAtInteractableColorView : MonoBehaviour, IPunObservable
{
    StudyLookAtInteractable m_studyLookAtInteractable;
    PhotonView m_photonView;

    private bool m_hasUnappliedNetworkData = false;
    private bool r_hasValidGaze = false;
    private bool r_alreadyTriggerd = false;

    // Start is called before the first frame update
    void Start()
    {
        this.m_studyLookAtInteractable = GetComponent<StudyLookAtInteractable>();
        this.m_photonView = GetComponent<PhotonView>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!m_photonView.IsMine)
        {
            if (m_hasUnappliedNetworkData)
            {
                // do color sync over booleans, as this is the fastes & resource
                if (r_hasValidGaze)
                {
                    m_studyLookAtInteractable.SetColor(m_studyLookAtInteractable.ValidGazeMaterial);
                }
                else if(r_alreadyTriggerd)
                {
                    m_studyLookAtInteractable.SetColor(m_studyLookAtInteractable.OnTriggeredMaterial);
                }
                else
                {
                    m_studyLookAtInteractable.SetColor(m_studyLookAtInteractable.OnTriggeredMaterial);
                }
            }
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            bool hasValidGaze = m_studyLookAtInteractable.HasValidGaze;
            bool alreadyTriggerd = m_studyLookAtInteractable.IsAlreadyTriggered;
            stream.SendNext(hasValidGaze);
            stream.SendNext(alreadyTriggerd);
        }
        else
        {
            r_hasValidGaze = (bool)stream.ReceiveNext();
            r_alreadyTriggerd = (bool)stream.ReceiveNext();

            m_hasUnappliedNetworkData = true;
        }
    }
}
