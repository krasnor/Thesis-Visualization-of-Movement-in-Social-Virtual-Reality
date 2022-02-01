using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(PhotonView))]
public class NetworkedShovel : XRBaseInteractable, IPunObservable
{
    private PhotonView photonView;
    [Space]
    [Header("Networked Shovel Settings")]
    public Color DefaultColor = Color.grey;

    public Renderer ColoredShovelPart;
    public int MaterialIndexToEdit = 0;

    private PlayerSettings localPlayerSettings;
    public Color CurrentColor;              // synced over network
    public bool IsShovelActivated = false;  // synced over network

    protected bool m_hasUnappliedNetworkData = false;
    private bool r_IsShovelActivated;
    private Color r_CurrentColor;

    public NetworkedShovel() { }

    protected override void Awake()
    {
        base.Awake();

        photonView = GetComponent<PhotonView>();
        if (photonView == null)
            throw new MissingComponentException("photonView Component not assigned");

        Assert.IsTrue(photonView.OwnershipTransfer == OwnershipOption.Takeover, "Script only configured for OwnershipOption.Takeover");

        if (ColoredShovelPart == null)
            throw new MissingComponentException("ColoredShovelPart Component not assigned");
    }



    // Start is called before the first frame update
    void Start()
    {
        localPlayerSettings = PlayerSettings.Instance;
        if (localPlayerSettings == null)
            throw new MissingComponentException("playerSettings Component not assigned");
        ResetShovel();
    }

    public void RequestOwnershipAndResetShovel()
    {
        photonView.RequestOwnership();
        ResetShovel();
    }

    public void ResetShovel()
    {
        CurrentColor = DefaultColor;
        IsShovelActivated = false;
        UpdateColor();
    }

    // Update is called once per frame
    void Update()
    {
        if (!this.photonView.IsMine) // only if not owned. -> apply data from network
        {
            if (m_hasUnappliedNetworkData)
            {
                m_hasUnappliedNetworkData = false;

                IsShovelActivated = r_IsShovelActivated;
                CurrentColor = r_CurrentColor;
                UpdateColor();
            }
        }
    }

    //private void hoverenteredSubscription(HoverEnterEventArgs arg0)
    //{
    //    Debug.Log("hoverenteredSubscription");
    //}
    //private void selectSubscription(SelectEnterEventArgs arg0)
    //{
    //    Debug.Log("selectSubscription");
    //}
    protected void UpdateColor()
    {
        if (MaterialIndexToEdit < ColoredShovelPart.materials.Length)
        {
            ColoredShovelPart.materials[MaterialIndexToEdit].color = CurrentColor;
        }
        else
        {
            // edit first entry
            Debug.LogWarning($"Material Index not found. Material.length: {ColoredShovelPart.materials.Length} - MaterialIndexToEdit: {MaterialIndexToEdit}");
            ColoredShovelPart.material.color = CurrentColor;
        }
    }

    public void ActivateShovel(Color a_playerColor)
    {
        CurrentColor = a_playerColor;
        IsShovelActivated = true;
    }

    protected override void OnSelectEntering(SelectEnterEventArgs args)
    {
        base.OnSelectEntering(args);

        photonView.RequestOwnership();

        Color playerColor = localPlayerSettings.StudyPlayerColor;

        Debug.Log("Player wants to activate Shovel: color " + playerColor);
        ActivateShovel(playerColor);
        UpdateColor();
    }

    protected override void OnSelectExiting(SelectExitEventArgs args)
    {
        base.OnSelectExiting(args);
        // do nothing
    }

    protected override void OnHoverEntered(HoverEnterEventArgs args)
    {
        base.OnHoverEntered(args);
        // do nothing
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            Vector4 colorVec = CurrentColor;
     
            stream.SendNext(IsShovelActivated);
            //stream.SendNext(colorVec);
            stream.SendNext(CurrentColor.r);
            stream.SendNext(CurrentColor.g);
            stream.SendNext(CurrentColor.b);
            stream.SendNext(CurrentColor.a);

        }
        else
        {

            r_IsShovelActivated = (bool)stream.ReceiveNext();

            //Vector4 r_colorVec = (Vector4)stream.ReceiveNext(); // vector 4 seems not to be supported by default from photon
            //r_CurrentColor = new Color(
            //    r_colorVec.x,
            //    r_colorVec.y,
            //    r_colorVec.z,
            //    r_colorVec.w
            //    );
            float r_r = (float)stream.ReceiveNext();
            float r_g = (float)stream.ReceiveNext();
            float r_b = (float)stream.ReceiveNext();
            float r_a = (float)stream.ReceiveNext();
            r_CurrentColor = new Color(
               r_r,
               r_g,
               r_b,
               r_a
               );

            m_hasUnappliedNetworkData = true;
        }


    }
}
