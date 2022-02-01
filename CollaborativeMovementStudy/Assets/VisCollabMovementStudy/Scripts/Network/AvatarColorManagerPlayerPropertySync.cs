using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(AvatarColorManager))]
public class AvatarColorManagerPlayerPropertySync : MonoBehaviour, IInRoomCallbacks
{
    private AvatarColorManager m_colorManager;
    private PhotonView photonView;

    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
        m_colorManager = GetComponent<AvatarColorManager>();
        PhotonNetwork.AddCallbackTarget(this);
        //Debug.Log($"AvatarColorManagerPlayerPropertySync.TrySyncColor() -- Awake -- for ActorNumber: {photonView.Owner.ActorNumber} Has PlayerColor: {photonView.Owner.CustomProperties.ContainsKey(PLAYER_COLOR_SETTINGS_KEY)}");
        TrySyncColor();
    }

    void IInRoomCallbacks.OnPlayerEnteredRoom(Player newPlayer)
    {
    }

    void IInRoomCallbacks.OnPlayerLeftRoom(Player otherPlayer)
    {
    }

    void IInRoomCallbacks.OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
    }

    void IInRoomCallbacks.OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (targetPlayer.ActorNumber == photonView.Owner.ActorNumber)
        {
            //Debug.Log($"AvatarColorManagerPlayerPropertySync.TrySyncColor() -- OnPlayerPropertiesUpdate -- for ActorNumber: {photonView.Owner.ActorNumber} Has PlayerColor: {photonView.Owner.CustomProperties.ContainsKey(PLAYER_COLOR_SETTINGS_KEY)}");
            TrySyncColor();
        }
    }

    void IInRoomCallbacks.OnMasterClientSwitched(Player newMasterClient)
    {
    }

    private void TrySyncColor()
    {
        if (NetworkedPlayerSettings.TryGetColorOfPlayer(photonView.Owner, out var parsedColor))
        {
            m_colorManager.UpdateAvatarColor(parsedColor);
        }
        if (!photonView.IsMine)
        {
            // never show own network avatar
            if (NetworkedPlayerSettings.TryGetInvisiblityStateOfPlayer(photonView.Owner, out var isNetworkPlayerInvisible))
            {
                m_colorManager.SetInvisibilityState(isNetworkPlayerInvisible);
            }
        }
    }
}
