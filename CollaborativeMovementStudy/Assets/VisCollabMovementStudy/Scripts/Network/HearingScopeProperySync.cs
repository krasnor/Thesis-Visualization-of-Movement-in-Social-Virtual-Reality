using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HearingScopeProperySync : MonoBehaviour, IInRoomCallbacks
{
    public AudioSource NetworkPlayerSpeaker;
    private PhotonView photonView;

    public bool SupervisorIgnoresHearingScope = true;
    public bool debugUseHearingVisual = true;
    public GameObject debugVisual_Hearable;
    public GameObject debugVisual_Muted;

    void Awake()
    {
        photonView = GetComponent<PhotonView>();
        PhotonNetwork.AddCallbackTarget(this);

        if (NetworkPlayerSpeaker == null)
            throw new MissingComponentException("NetworkPlayerSpeaker component not assigned.");
    }
    // Start is called before the first frame update
    void Start()
    {
        // Avatar created -> update hearing scope
        UpdateAudioHearingScope();
    }
    void OnDestroy()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }


    void FixedUpdate()
    {
        // avoid calling this too often on systems with high fps (50 fps check is enough)
        if (debugUseHearingVisual && debugVisual_Hearable != null && debugVisual_Muted != null)
        {
            if (debugVisual_Hearable.activeSelf != !NetworkPlayerSpeaker.mute)
                debugVisual_Hearable.SetActive(!NetworkPlayerSpeaker.mute);
            if (debugVisual_Muted.activeSelf != NetworkPlayerSpeaker.mute)
                debugVisual_Muted.SetActive(NetworkPlayerSpeaker.mute);
        }
    }

    public void UpdateAudioHearingScope()
    {
        if (gameObject != null && gameObject.activeInHierarchy)
        {
            //int localPlayer = PhotonNetwork.LocalPlayer;
            PlayerAudioHearingScope localPlayerHearingScope = PlayerSettings.Instance.StudyPlayerHearingScope;
            StudyPlayerRole localPlayerRole = PlayerSettings.Instance.StudyPlayerRole;

            if (SupervisorIgnoresHearingScope && localPlayerRole == StudyPlayerRole.SUPERVISOR)
            {
                // supervisor can always hear despite hearing scope
                NetworkPlayerSpeaker.mute = false;
                return;
            }

            Player networkedPlayerOwner = photonView.Owner;
            if (NetworkedPlayerSettings.TryGetRoleOfPlayer(networkedPlayerOwner, out StudyPlayerRole ownersRole))
            {
                switch (localPlayerHearingScope)
                {
                    case PlayerAudioHearingScope.Everyone:
                        NetworkPlayerSpeaker.mute = false;
                        break;
                    case PlayerAudioHearingScope.Nobody:
                        NetworkPlayerSpeaker.mute = true;
                        break;
                    case PlayerAudioHearingScope.SupervisorOnly:
                        if (ownersRole == StudyPlayerRole.SUPERVISOR)
                        {
                            NetworkPlayerSpeaker.mute = false;
                        }
                        else
                        {
                            // cannot hear visitors and guides
                            NetworkPlayerSpeaker.mute = true;
                        }
                        break;
                    default:
                        Debug.LogWarning("Unhandled PlayerAudioHearingScope " + localPlayerHearingScope + ". Did set Muted to false.");
                        NetworkPlayerSpeaker.mute = false;
                        break;
                }
            }
        }
        else
        {
            // no update, as object is null or not active in scene
        }

    }


    public void OnMasterClientSwitched(Player newMasterClient)
    {
    }

    public void OnPlayerEnteredRoom(Player newPlayer)
    {
    }

    public void OnPlayerLeftRoom(Player otherPlayer)
    {
    }

    public void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        // a role or hearing scope of an player could have changed -> update
        UpdateAudioHearingScope();
    }

    public void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
    }
}
