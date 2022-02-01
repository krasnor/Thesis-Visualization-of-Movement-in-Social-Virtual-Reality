using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

public class NetworkedConstructionSite : MonoBehaviour, IPunObservable
{
    public PhotonView photonView;

    [SerializeField]
    private int m_currentBuildModelId = 0;
    public int CurrentBuildModelId { get { return m_currentBuildModelId; } }


    public List<ConstructedBuildItem> ConstructionParts = new List<ConstructedBuildItem>();
    public List<ConstructedBuildItem> AlwaysVisibleConstructionParts = new List<ConstructedBuildItem>();
    /// <summary>
    /// Status if the building part has been delivered/constructed or not. (int: partId, bool: status)
    /// </summary>
    private Dictionary<int, bool> BuildPartDeliveryStatus = new Dictionary<int, bool>();
    private Dictionary<int, bool> m_buildPartDeliveredEventStatus = new Dictionary<int, bool>();
    /// <summary>
    /// Build Parts have an Part Id corresponding to the part of the build Model.
    /// With this option this can be ignored and the items will be constructed in ascending PartId order (e.g. 0 to 3).
    /// May cause problems if there are gaps in part ids e.g. [0,1,3].
    /// </summary>
    [Tooltip(" Build Parts have an Part Id corresponding to the part of the build Model. With this option this can be ignored and the items will be constructed in ascending PartId order (e.g. 0 to 3). May cause problems if there are gaps in part ids e.g. [0,1,3].")]
    [SerializeField]
    private bool m_IgnoreBuildPartIdAndBuildConsecutively = true;
    public bool IgnoreBuildPartIdAndBuildConsecutively { get => m_IgnoreBuildPartIdAndBuildConsecutively; private set => m_IgnoreBuildPartIdAndBuildConsecutively = value; }
    /// <summary>
    /// Counter for the last delivered PartId.
    /// Has only a logical value when IgnoreBuildPartIdAndBuildConsecutively == true.
    /// </summary>
    [SerializeField]
    private int m_highestPartIdDelivered = -1;

    public bool IsBuildComplete = false;
    private bool IsBuildCompleteProcessed = false;
    /// <summary>
    ///// Parameter is CurrentBuildModelId
    /// </summary>
    public UnityEvent<int> OnBuildSiteCompleted;
    ///// <summary>
    ///// Parameter is CurrentBuildModelId, BuildPartId
    ///// </summary>
    public UnityEvent<int, int> OnBuildPartDelivered;

    /// <summary>
    /// Location where all build items are stored when buried or after being constructed
    /// </summary>
    public Transform BuildItemHiddenStorageSpot;
    public List<NetworkedBuildItem> BuildItems = new List<NetworkedBuildItem>();

    protected bool m_hasUnappliedNetworkData = false;
    protected bool r_IsBuildComplete = false;
    protected int r_currentBuildModelVariant = 0;
    protected int r_highestPartIdDelivered = -1;
    protected Dictionary<int, bool> r_collectedItems = new Dictionary<int, bool>();

    protected virtual void Awake()
    {
        photonView = GetComponent<PhotonView>();
        Assert.IsTrue(photonView.OwnershipTransfer == OwnershipOption.Takeover, "Script only configured for OwnershipOption.Takeover");
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        ResetConstructionSite();
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (!this.photonView.IsMine) // only if not owned. -> apply data from network
        {
            if (m_hasUnappliedNetworkData)
            {
                m_hasUnappliedNetworkData = false;

                m_currentBuildModelId = r_currentBuildModelVariant;
                IsBuildComplete = r_IsBuildComplete;
                BuildPartDeliveryStatus = r_collectedItems;
                m_highestPartIdDelivered = r_highestPartIdDelivered;

                RefreshContructedPartsVisibility();
            }
        }

        // check build part delivery 
        foreach (var bps in BuildPartDeliveryStatus)
        {
            int partId = bps.Key;
            bool isDelivered = bps.Value;
            if (m_buildPartDeliveredEventStatus.ContainsKey(partId))
            {
                if (isDelivered == false)
                {
                    // reset delivery event status: 
                    // if an item has not been delivered yet -> event also has not been sent yet (important as not all clients do execute reset() (only the owner))
                    m_buildPartDeliveredEventStatus[partId] = false;
                }
                else if (isDelivered == true && m_buildPartDeliveredEventStatus[partId] == false)
                {
                    // part id has been delivered, but event has not yet been triggered -> trigger event
                    OnBuildPartDelivered.Invoke(CurrentBuildModelId, partId);
                    m_buildPartDeliveredEventStatus[partId] = true;
                }
            }
        }

        // check build complete condition
        if (IsBuildComplete)
        {
            if (!IsBuildCompleteProcessed)
            {
                OnBuildSiteCompleted.Invoke(CurrentBuildModelId);
                IsBuildCompleteProcessed = true;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        NetworkedBuildItem networkedBuildItem = other.GetComponentInChildren<NetworkedBuildItem>();
        if (networkedBuildItem != null)
        {
            if (networkedBuildItem.BuildModelId != m_currentBuildModelId)
            {
                // not for this construction site -> do nothing
                Debug.LogWarning($"build item delivered that does not belong to this site. Site Model variant: {m_currentBuildModelId} Part Model variant: {m_currentBuildModelId}", this.gameObject);
                return;
            }

            // Variant A - Owner processes delivery, carrier hides item
            // -> possible that owner never sees item delivered, as item is instantly stored

            // Variant B - item owner drops item and gives ownership to site owner.
            // -> why not simply take ownership of site? -> Variant C

            // => Variant C - item owner takes ownership of site

            if (networkedBuildItem.photonView.IsMine)
            {
                // owner of build Item should process storage -> request ownership of construction site
                photonView.TransferOwnership(PhotonNetwork.LocalPlayer);

                if (networkedBuildItem.IsInteractable)
                {
                    //Debug.Log($"BuildItem entered DropArea partId: {networkedBuildItem.BuildPartId} Frame: {Time.frameCount} IsInteractable: {networkedBuildItem.IsInteractable} - do build");

                    // release item from hand & deactivate
                    ReleaseFromHand(networkedBuildItem);
                    HideBuildItem(networkedBuildItem);

                    HandleBuildPartDelivery(networkedBuildItem);
                }
                else
                {
                    //Debug.Log($"BuildItem entered DropArea partId: {networkedBuildItem.BuildPartId} Frame: {Time.frameCount} IsInteractable: {networkedBuildItem.IsInteractable} - do nothing");
                    // item has already been triggered a few frames before. 
                    // (ReleaseFromHand and HideBuildItem do cause mutliple triggers of OnTriggerEnter())

                    // item should not be here -> hide it
                    HideBuildItem(networkedBuildItem);
                }
            }
        }
    }

    /// <summary>
    /// Currently only consecutive mode is supported.
    /// </summary>
    public void OverrideAdvanceProgress()
    {
        try
        {
            // request ownership of construction site
            photonView.TransferOwnership(PhotonNetwork.LocalPlayer);

            if (m_IgnoreBuildPartIdAndBuildConsecutively)
            {
                // mark next item as delivered
                int nextPartId = ++m_highestPartIdDelivered;
                Debug.Log($"OverrideAdvanceProgress. Consecutive Build Mode, enabling partId: {nextPartId}");
                if (BuildPartDeliveryStatus.ContainsKey(nextPartId))
                {
                    BuildPartDeliveryStatus[nextPartId] = true;
                }
            }
            else
            {
                Debug.LogWarning($"OverrideAdvanceProgress does not support non-consecutive mode.");
                // TODO implement if needed
            }

            RefreshContructedPartsVisibility();
            IsBuildComplete = CheckCompletion();
        }
        catch (System.Exception)
        {
            Debug.LogError("An error occured while trying to advance progress by override.");
        }
    }

    public void HandleBuildPartDelivery(NetworkedBuildItem a_NetworkedBuildItem)
    {
        int buildModelId = a_NetworkedBuildItem.BuildModelId;
        int buildPartId = a_NetworkedBuildItem.BuildPartId;

        if (m_IgnoreBuildPartIdAndBuildConsecutively)
        {
            // mark next item as delivered
            int nextPartId = ++m_highestPartIdDelivered;
            Debug.Log($"Consecutive Build, enabling partId: {nextPartId}");
            if (BuildPartDeliveryStatus.ContainsKey(nextPartId))
            {
                BuildPartDeliveryStatus[nextPartId] = true;
            }
        }
        else
        {
            Debug.Log($"Normal Enabling partId: {buildPartId}");
            // mark item as delivered
            if (BuildPartDeliveryStatus.ContainsKey(buildPartId))
            {
                BuildPartDeliveryStatus[buildPartId] = true;
            }
        }

        RefreshContructedPartsVisibility();
        IsBuildComplete = CheckCompletion();
    }

    private void ReleaseFromHand(NetworkedBuildItem a_NetworkedBuildItem)
    {
        a_NetworkedBuildItem.selectingInteractor?.EndManualInteraction();
        a_NetworkedBuildItem.IsInteractable = false;
    }

    private void HideBuildItem(NetworkedBuildItem a_NetworkedBuildItem)
    {
        a_NetworkedBuildItem.transform.SetPositionAndRotation(BuildItemHiddenStorageSpot.position, BuildItemHiddenStorageSpot.rotation);
        a_NetworkedBuildItem.IsInteractable = false;
    }


    public void RequestOwnershipAndReset(int a_buildModelId)
    {
        DoRequestOwnerShipForReset();
        SwitchBuildModelIdAndReset(a_buildModelId);
    }

    /// <summary>
    /// Only Resets ConstructionSite and BuildItems not the digsites!
    /// </summary>
    public void RequestOwnershipAndReset()
    {
        DoRequestOwnerShipForReset();
        ResetConstructionSite();
    }
    private void DoRequestOwnerShipForReset()
    {
        photonView.TransferOwnership(PhotonNetwork.LocalPlayer);
        foreach (var bi in BuildItems)
        {
            bi.photonView.TransferOwnership(PhotonNetwork.LocalPlayer);
        }
    }

    /// <summary>
    /// Switches BuildModelId and performs a Reset.
    /// 
    /// Note: Ownership of ConstructionSite and BuildItems required!
    /// </summary>
    private void SwitchBuildModelIdAndReset(int a_buildModelId)
    {
        Debug.Log("SwitchBuildModelIdAndReset + " + a_buildModelId, this.gameObject);
        if (m_currentBuildModelId != a_buildModelId)
        {
            m_currentBuildModelId = a_buildModelId;
        }

        ResetConstructionSite();
    }

    /// <summary>
    /// Ownership of Construction Site and BuildItems required for bug free handling!
    /// Only Resets ConstructionSite and BuildItems not the digsites!
    /// </summary>
    private void ResetConstructionSite()
    {
        IsBuildComplete = false;
        IsBuildCompleteProcessed = false;
        m_highestPartIdDelivered = -1;

        BuildPartDeliveryStatus.Clear();
        m_buildPartDeliveredEventStatus.Clear();

        // hide all construction parts and mark their build DeliveryStatus Dictionary as undelivered
        foreach (var constructionPart in ConstructionParts)
        {
            constructionPart.Visible = false;
            if (constructionPart.BuildModelId == m_currentBuildModelId)
            {
                BuildPartDeliveryStatus[constructionPart.BuildPartId] = false;
                m_buildPartDeliveredEventStatus[constructionPart.BuildPartId] = false;
            }
        }

        foreach (var constructionPart in AlwaysVisibleConstructionParts)
        {
            constructionPart.Visible = false;
            if (constructionPart.BuildModelId == m_currentBuildModelId)
            {
                constructionPart.Visible = true;
            }
        }

        // Debug Checks
        DoDebugChecks();

        // Hide all build items
        foreach (var bItem in BuildItems)
        {
            ReleaseFromHand(bItem);
            HideBuildItem(bItem);
        }
    }

    private void DoDebugChecks()
    {
        // check if there are any build parts for this variant
        if (BuildPartDeliveryStatus.Count == 0)
        {
            Debug.LogWarning($"No construction parts for this build model variant found. Variant: {m_currentBuildModelId}. Is this intentional?", this.gameObject);
        }

        // check if there is for each building step an item
        int missingItems = 0;
        foreach (var toBeDeliveredPart in BuildPartDeliveryStatus)
        {
            bool foundItemForPart = false;
            foreach (var bItem in BuildItems)
            {
                if (toBeDeliveredPart.Key == bItem.BuildModelId)
                    foundItemForPart = true;
            }
            if (!foundItemForPart)
                missingItems++;
        }
        if (missingItems > 0)
        {
            Debug.LogWarning($"Missing some build items for this model. Variant: {m_currentBuildModelId}. Count missing items: {missingItems}. Are items missing or maybe just not registed with the construction site?", this.gameObject);
        }
    }

    private void RefreshContructedPartsVisibility()
    {
        foreach (var cp in ConstructionParts)
        {
            if (cp.BuildModelId != m_currentBuildModelId)
            {
                cp.Visible = false;
                continue;
            }
            if (cp.BuildModelId == m_currentBuildModelId && BuildPartDeliveryStatus.ContainsKey(cp.BuildPartId) && BuildPartDeliveryStatus[cp.BuildPartId] == true)
            {
                cp.Visible = true;
            }
        }
    }

    public bool CheckCompletion()
    {
        return BuildPartDeliveryStatus.Values.All(cPartStatus => cPartStatus == true);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(m_currentBuildModelId);
            stream.SendNext(IsBuildComplete);
            stream.SendNext(BuildPartDeliveryStatus);
            stream.SendNext(m_highestPartIdDelivered);
        }
        else
        {
            r_currentBuildModelVariant = (int)stream.ReceiveNext();
            r_IsBuildComplete = (bool)stream.ReceiveNext();
            r_collectedItems = (Dictionary<int, bool>)stream.ReceiveNext();
            r_highestPartIdDelivered = (int)stream.ReceiveNext();
            m_hasUnappliedNetworkData = true;
        }
    }
}
