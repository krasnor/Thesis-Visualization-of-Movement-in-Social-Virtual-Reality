using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OwnerBasedPassthrough : MonoBehaviour
{
    public GameObject PassthroughWarning;
    public GameObject ColliderParent;
    public PhotonView PhotonViewToObserve;
    public bool ShowPassthroughWarning = false;

    // Start is called before the first frame update
    void Awake()
    {
        if (PassthroughWarning == null)
            throw new MissingComponentException("PassthroughWarning component not set");
        if (ColliderParent == null)
            throw new MissingComponentException("ColliderParent component not set");
        if (PhotonViewToObserve == null)
            throw new MissingComponentException("PhotonViewToObserve component not set");
    }

    void Update()
    {
        bool toBlock = true;
        if (PhotonViewToObserve.OwnerActorNr == PhotonNetwork.LocalPlayer.ActorNumber && PhotonNetwork.InRoom)
        {
            //permit passthrough
            toBlock = false;
        }

        if (ColliderParent.activeSelf != toBlock)
            ColliderParent.SetActive(toBlock);


        if (ShowPassthroughWarning == false)
            toBlock = false;

        if (PassthroughWarning.activeSelf != toBlock)
            PassthroughWarning.SetActive(toBlock);
    }
}
