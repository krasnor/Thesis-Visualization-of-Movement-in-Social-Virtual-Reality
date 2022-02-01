using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkedPlayerColorManager : MonoBehaviour, IInRoomCallbacks, IMatchmakingCallbacks
{
    public Color[] playerColorCycle = new Color[]
    {
        Color.red,
        Color.green,
        //Color.yellow,
        Color.blue,
    };

    void Awake()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    private void AssignPlayerColor(Player newPlayer)
    {
        Color newPlayerColor = playerColorCycle[newPlayer.ActorNumber % playerColorCycle.Length];

        // Set initial Player Settings
        ExitGames.Client.Photon.Hashtable initialPlayerSettings = new ExitGames.Client.Photon.Hashtable()
            {
               { NetworkedPlayerSettings.PropertyKeyPlayerColor, ColorUtility.ToHtmlStringRGBA(newPlayerColor)},
               { NetworkedPlayerSettings.PropertyKeyPlayerRole, StudyPlayerRole.VISITOR },
               { NetworkedPlayerSettings.PropertyKeyPlayerHearingScope, PlayerAudioHearingScope.Everyone},
            };
        newPlayer.SetCustomProperties(initialPlayerSettings);
    }

    #region IMatchmakingCallbacks

    void IMatchmakingCallbacks.OnCreatedRoom()
    {
    }

    void IMatchmakingCallbacks.OnCreateRoomFailed(short returnCode, string message)
    {
    }

    void IMatchmakingCallbacks.OnFriendListUpdate(List<FriendInfo> friendList)
    {
    }

    void IMatchmakingCallbacks.OnJoinedRoom()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            AssignPlayerColor(PhotonNetwork.LocalPlayer);
        }
    }

    void IMatchmakingCallbacks.OnJoinRandomFailed(short returnCode, string message)
    {
    }

    void IMatchmakingCallbacks.OnJoinRoomFailed(short returnCode, string message)
    {
    }

    void IMatchmakingCallbacks.OnLeftRoom()
    {
    }
    #endregion

    #region IInRoomCallbacks

    void IInRoomCallbacks.OnMasterClientSwitched(Player newMasterClient)
    {
    }

    void IInRoomCallbacks.OnPlayerEnteredRoom(Player newPlayer)
    {
        // a player (except the room creator entered the room)
        Debug.Log("IInRoomCallbacks.OnPlayerEnteredRoom");
        if (PhotonNetwork.IsMasterClient)
        {
            AssignPlayerColor(newPlayer);
        }
    }

    void IInRoomCallbacks.OnPlayerLeftRoom(Player otherPlayer)
    {
    }

    void IInRoomCallbacks.OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
    }

    void IInRoomCallbacks.OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
    }
    #endregion

}
