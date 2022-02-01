using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using UnityEngine;

public class NetworkedGameSettings : GameSettings, IInRoomCallbacks, IMatchmakingCallbacks
{

    public static readonly string RoomTeleportationModeSettingsKey = "rTPMode";
    public static readonly string DoorOpenStateRoomsKey = "rOpnRms";
    //public static readonly string DoorOpenStateRoom1 = "rOpnR1";
    //public static readonly string DoorOpenStateRoom2 = "rOpnR2";
    public static readonly string DoorOpenOutsideWorldKey = "rOpnW";
    public static readonly string LoggingSessionIdKey = "rSsnId";


    public bool SetDefaultSettingsOnRoomCreate = true;

    void Awake()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    public void SetRoomTeleportationMode(TracedTeleportationProviderMode a_tpMode)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            ExitGames.Client.Photon.Hashtable newTPSetting = new ExitGames.Client.Photon.Hashtable()
            {
               { RoomTeleportationModeSettingsKey, a_tpMode},
            };
            PhotonNetwork.CurrentRoom.SetCustomProperties(newTPSetting);
            // ! do not set GameSettings.TeleportationMode here. 
            // ! This should be done in the callback (IInRoomCallbacks.OnRoomPropertiesUpdate) -> so all Clients have the same Setting
        }
    }

    public void SetDoorRoomsOpen(bool a_open)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            ExitGames.Client.Photon.Hashtable newDoorstate = new ExitGames.Client.Photon.Hashtable()
            {
               { DoorOpenStateRoomsKey, a_open},
            };
            PhotonNetwork.CurrentRoom.SetCustomProperties(newDoorstate);
        }
    }

    public void SetDoorWorldOpen(bool a_open)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            ExitGames.Client.Photon.Hashtable newDoorstate = new ExitGames.Client.Photon.Hashtable()
            {
               { DoorOpenOutsideWorldKey, a_open},
            };
            PhotonNetwork.CurrentRoom.SetCustomProperties(newDoorstate);
        }
    }

    public void SetLoggingSessionId(string a_sessionId)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            ExitGames.Client.Photon.Hashtable newProperty = new ExitGames.Client.Photon.Hashtable()
            {
               { LoggingSessionIdKey, a_sessionId},
            };
            PhotonNetwork.CurrentRoom.SetCustomProperties(newProperty);
        }
    }


    #region IInRoomCallbacks

    void IInRoomCallbacks.OnMasterClientSwitched(Player newMasterClient)
    {
    }

    void IInRoomCallbacks.OnPlayerEnteredRoom(Player newPlayer)
    {
    }

    void IInRoomCallbacks.OnPlayerLeftRoom(Player otherPlayer)
    {
    }

    void IInRoomCallbacks.OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
    }

    void IInRoomCallbacks.OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        SyncRoomProperties(propertiesThatChanged);
    }

    private void SyncRoomProperties(Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey(RoomTeleportationModeSettingsKey))
        {
            TracedTeleportationProviderMode newTPMode = (TracedTeleportationProviderMode)Enum.Parse(typeof(TracedTeleportationProviderMode), propertiesThatChanged[RoomTeleportationModeSettingsKey].ToString());
            this.UpdateTeleportationMode(newTPMode);
        }
        if (propertiesThatChanged.ContainsKey(DoorOpenStateRoomsKey))
        {
            if (bool.TryParse(propertiesThatChanged[DoorOpenStateRoomsKey].ToString(), out bool newOpenState))
            {
                this.UpdateOpenStateDoorRooms(newOpenState);
            }
        }
        if (propertiesThatChanged.ContainsKey(DoorOpenOutsideWorldKey))
        {
            if (bool.TryParse(propertiesThatChanged[DoorOpenOutsideWorldKey].ToString(), out bool newOpenState))
            {
                this.UpdateOpenStateDoorOutsideWorld(newOpenState);
            }
        }

        if (propertiesThatChanged.ContainsKey(LoggingSessionIdKey))
        {
            this.UpdateLoggingSessionIdKey(propertiesThatChanged[LoggingSessionIdKey].ToString());
        }
    }
    #endregion


    #region IMatchmakingCallbacks

    void IMatchmakingCallbacks.OnFriendListUpdate(List<FriendInfo> friendList)
    {
    }

    void IMatchmakingCallbacks.OnCreatedRoom()
    {
        if (PhotonNetwork.IsMasterClient && SetDefaultSettingsOnRoomCreate)
        {
            // set initial room settings
            ExitGames.Client.Photon.Hashtable initialRoomSettings = new ExitGames.Client.Photon.Hashtable()
            {
               { RoomTeleportationModeSettingsKey, this.TeleportationMode},
               { DoorOpenStateRoomsKey, this.OpenStateDoorRooms},
               { DoorOpenOutsideWorldKey, this.OpenStateDoorOutsideWorld},
               { LoggingSessionIdKey, GameSettings.DefaultLoggingSessionId},
            };
            PhotonNetwork.CurrentRoom.SetCustomProperties(initialRoomSettings);
        }
    }

    void IMatchmakingCallbacks.OnCreateRoomFailed(short returnCode, string message)
    {
    }

    void IMatchmakingCallbacks.OnJoinedRoom()
    {
        var currentRoom = PhotonNetwork.CurrentRoom;
        if (currentRoom != null)
        {
            SyncRoomProperties(currentRoom.CustomProperties);
        }
    }

    void IMatchmakingCallbacks.OnJoinRoomFailed(short returnCode, string message)
    {
    }

    void IMatchmakingCallbacks.OnJoinRandomFailed(short returnCode, string message)
    {
    }

    void IMatchmakingCallbacks.OnLeftRoom()
    {
    }
    #endregion
}
