using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIQuickVariousActionHelper : MonoBehaviour
{
    public NetworkedPlayerSettings PlayerSettings;
    public TracedTeleportationProvider tpManager;
    //public AvatarColorManager ColorManager;
    public Button ToggleInvisibilityOfLocalPlayer;
    public Button TP_Top;
    public Button TP_Bot;
    public GameObject TopTPSpawn;
    public GameObject BotTPSpawn;


    void Awake()
    {
        if (PlayerSettings == null)
            throw new MissingComponentException("PlayerSettings component not assigned.");
        //if (ColorManager == null)
        //    throw new MissingComponentException("ColorManager component not assigned.");
        if (ToggleInvisibilityOfLocalPlayer == null)
            throw new MissingComponentException("ToggleInvisibilityOfLocalPlayer component not assigned.");
    }

    // Start is called before the first frame update
    void Start()
    {
        ToggleInvisibilityOfLocalPlayer.onClick.AddListener(OnInvsibilityButtonClicked);
        TP_Top?.onClick.AddListener(TPTop);
        TP_Bot?.onClick.AddListener(TPBot);
    }

    private void OnInvsibilityButtonClicked()
    {
        //ColorManager?.SetInvisibilityState(!ColorManager.IsInvisible);
        PlayerSettings.RequestInvisibilityChange(PhotonNetwork.LocalPlayer.ActorNumber, !PlayerSettings.IsPlayerInvisible);
    }

    private void TPTop()
    {
        if (tpManager != null && BotTPSpawn != null)
        {
            var tp_request = new UnityEngine.XR.Interaction.Toolkit.TeleportRequest
            {
                destinationPosition = TopTPSpawn.transform.position
            };

            tpManager.QueueTeleportRequest(tp_request);
        }
    }

    private void TPBot()
    {
        if (tpManager != null && BotTPSpawn != null)
        {
            var tp_request = new UnityEngine.XR.Interaction.Toolkit.TeleportRequest
            {
                destinationPosition = BotTPSpawn.transform.position
            };
            tpManager.QueueTeleportRequest(tp_request);
        }
    }

}
