using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class NetworkedDigsite : MonoBehaviour, IPunObservable
{
    private PhotonView photonView;
    [Space]
    public GameObject DirtHill;
    public NetworkedShovel Shovel1;
    public NetworkedShovel Shovel2;
    public Transform BuriedItemSpawnLocation;
    public NetworkedBuildItem BuriedItem;

    public bool IsExcavated = false; // synced var
    private bool IsExcavatedSwitchProcessed = false;
    [SerializeField]
    bool debug_OverrideTwoColorCondition = false;
    [SerializeField]
    bool debug_WarnIfNoItemAssigned = false;

    protected bool m_hasUnappliedNetworkData = false;
    private bool r_IsExcavated;

    public void RequestOwnershipAndResetDigsite()
    {
        if(photonView.OwnershipTransfer == OwnershipOption.Fixed && !PhotonNetwork.IsMasterClient)
        {
            Debug.LogWarning("Only Masterclient can reset this object", this.gameObject);
            return;
        }

        photonView.RequestOwnership();

        Shovel1.RequestOwnershipAndResetShovel();
        Shovel2.RequestOwnershipAndResetShovel();
        ResetDigsite();
    }

    public void ResetDigsite()
    {
        IsExcavated = false;
        IsExcavatedSwitchProcessed = false;

        DirtHill.SetActive(true); // visible
        Shovel1.ResetShovel();
        Shovel2.ResetShovel();

        //if (BuriedItem != null && BuriedItem.gameObject != null)
        //{
        //    BuriedItem.gameObject.SetActive(false);
        //}
    }

    protected virtual void Awake()
    {
        if (DirtHill == null)
            throw new MissingComponentException("DirtHill Component not assigned");
        if (Shovel1 == null)
            throw new MissingComponentException("Shovel 1 Component not assigned");
        if (Shovel2 == null)
            throw new MissingComponentException("Shovel 2 Component not assigned");
        if(BuriedItemSpawnLocation == null)
            throw new MissingComponentException("BuriedItemSpawnLocation not set");

        if (debug_OverrideTwoColorCondition)
            Debug.LogWarning("Two Color Condition for Digsite is overridden. Is this intentional?", this.gameObject);
        if (debug_WarnIfNoItemAssigned && BuriedItem == null)
            Debug.LogWarning("Digsite has no buried item. Is this intentional?", this.gameObject);
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        photonView = GetComponent<PhotonView>();
        if (photonView == null)
            throw new MissingReferenceException("Could not find photonView Component");

        ResetDigsite();
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (photonView.IsMine)
        {
            if (!IsExcavatedSwitchProcessed && CheckExcavationStatus())
            {
                IsExcavated = true;
            }
        }
        else
        {  // a syncing client
            if (m_hasUnappliedNetworkData)
            {
                m_hasUnappliedNetworkData = false;

                IsExcavated = r_IsExcavated;
            }
        }

        if (IsExcavated) // <- synced over network
        {
            if (!IsExcavatedSwitchProcessed) // done only once
            {
                DirtHill.SetActive(false); // invisible
                if (BuriedItem != null && BuriedItem.gameObject != null) 
                {
                    // move item from storage location to the spawn location
                    UnhideBuildItem(BuriedItem);
                }
                IsExcavatedSwitchProcessed = true;
            }
        }
    }

    public bool CheckExcavationStatus()
    {
        bool bothActivated = Shovel1.IsShovelActivated && Shovel2.IsShovelActivated;
        bool differentColors = debug_OverrideTwoColorCondition ? true : Shovel1.CurrentColor != Shovel2.CurrentColor;

        return bothActivated && differentColors;
    }

    private void UnhideBuildItem(NetworkedBuildItem a_NetworkedBuildItem)
    {
        a_NetworkedBuildItem.transform.SetPositionAndRotation(BuriedItemSpawnLocation.position, BuriedItemSpawnLocation.rotation);
        a_NetworkedBuildItem.IsInteractable = true;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(IsExcavated);
        }
        else
        {
            r_IsExcavated = (bool)stream.ReceiveNext();

            m_hasUnappliedNetworkData = true;
        }
    }
}
