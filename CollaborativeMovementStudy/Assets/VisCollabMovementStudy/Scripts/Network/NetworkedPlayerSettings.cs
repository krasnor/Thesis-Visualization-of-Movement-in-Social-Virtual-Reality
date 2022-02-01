using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using UnityEngine;


public class NetworkedPlayerSettings : PlayerSettings, IInRoomCallbacks, IMatchmakingCallbacks
{
    public const string PropertyKeyPlayerColor = "pColor";
    public const string PropertyKeyPlayerInvisibility = "pInv";
    public const string PropertyKeyPlayerRole = "pSettings";
    public const string PropertyKeyPlayerHearingScope = "pHrScp";

    public const string PropertyKeyPlayerLoggingParticipantId = "pLgPId";

    protected override void Awake()
    {
        base.Awake();
        PhotonNetwork.AddCallbackTarget(this);
    }

    public void RequestMaster(int actorId)
    {
        foreach (var player in PhotonNetwork.PlayerList)
        {
            if (player.ActorNumber == actorId)
            {
                PhotonNetwork.SetMasterClient(player);
            }
        }
    }

    public void RequestInvisibilityChange(int actorId, bool a_isInvisble)
    {
        foreach (var player in PhotonNetwork.PlayerList)
        {
            if (player.ActorNumber == actorId)
            {
                ExitGames.Client.Photon.Hashtable newProperties = new ExitGames.Client.Photon.Hashtable()
                {
                    { PropertyKeyPlayerInvisibility, a_isInvisble },
                };
                player.SetCustomProperties(newProperties);
            }
        }
    }

    public void RequestRoleChange(int actorId, StudyPlayerRole a_newRole)
    {
        foreach (var player in PhotonNetwork.PlayerList)
        {
            if (player.ActorNumber == actorId)
            {
                ExitGames.Client.Photon.Hashtable newProperties = new ExitGames.Client.Photon.Hashtable()
                {
                    { PropertyKeyPlayerRole, a_newRole },
                };
                player.SetCustomProperties(newProperties);
            }
        }
    }

    public void RequestAudioHearingScopeChange(int actorId, PlayerAudioHearingScope a_hearScope)
    {
        foreach (var player in PhotonNetwork.PlayerList)
        {
            if (player.ActorNumber == actorId)
            {
                ExitGames.Client.Photon.Hashtable newProperties = new ExitGames.Client.Photon.Hashtable()
                {
                    { PropertyKeyPlayerHearingScope, a_hearScope },
                };
                player.SetCustomProperties(newProperties);
            }
        }
    }

    public void RequestParticipantIdChange(int actorId, string a_participantId)
    {
        foreach (var player in PhotonNetwork.PlayerList)
        {
            if (player.ActorNumber == actorId)
            {
                ExitGames.Client.Photon.Hashtable newProperties = new ExitGames.Client.Photon.Hashtable()
                {
                    { PropertyKeyPlayerLoggingParticipantId, a_participantId },
                };
                player.SetCustomProperties(newProperties);
            }
        }
    }


    #region IInRoomCallbacks

    void IInRoomCallbacks.OnPlayerEnteredRoom(Player newPlayer)
    {
    }

    void IInRoomCallbacks.OnPlayerLeftRoom(Player otherPlayer)
    {
    }

    void IInRoomCallbacks.OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
    }

    void IInRoomCallbacks.OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        //Debug.Log($"IInRoomCallbacks.OnPlayerPropertiesUpdate Player ActorNumber {targetPlayer.ActorNumber} Count Properties Changed: {changedProps.Count}");

        if (targetPlayer.IsLocal)
        {
            foreach (var k in changedProps.Keys)
            {
                switch (k.ToString())
                {
                    case PropertyKeyPlayerRole:
                        if (TryParseRole(changedProps[PropertyKeyPlayerRole].ToString(), out var newRole))
                        {
                            this.UpdateStudyPlayerRole(newRole);
                        }
                        break;
                    case PropertyKeyPlayerColor:
                        if (TryParseColor(changedProps[PropertyKeyPlayerColor].ToString(), out var newColor))
                        {
                            this.UpdateStudyPlayerColor(newColor);
                        }
                        break;
                    case PropertyKeyPlayerInvisibility:
                        if (TryParseInvisibility(changedProps[PropertyKeyPlayerInvisibility].ToString(), out var newIsInvisibleState))
                        {
                            this.UpdateStudyPlayerInvisibilityState(newIsInvisibleState);
                        }
                        break;
                    case PropertyKeyPlayerHearingScope:
                        if (PlayerAudioHearingScope.TryParse(changedProps[PropertyKeyPlayerHearingScope].ToString(), out PlayerAudioHearingScope newHearScope))
                        {
                            this.UpdateStudyPlayerHearingScope(newHearScope);
                        }
                        break;
                    case PropertyKeyPlayerLoggingParticipantId:
                        this.UpdateLoggingParticipantId(changedProps[PropertyKeyPlayerLoggingParticipantId].ToString());
                        break;
                    default:
                        break;
                }
            }
            //if (changedProps.ContainsKey(PropertyKeyPlayerRole))
            //{
            //    //Debug.Log("rRole: " + changedProps[PLAYER_ROLE_SETTINGS_KEY].ToString());
            //    //StudyPlayerRole newRole = (StudyPlayerRole)int.Parse(changedProps[PLAYER_ROLE_SETTINGS_KEY].ToString());
            //    if (TryParseRole(changedProps[PropertyKeyPlayerRole].ToString(), out var newRole))
            //    {
            //        this.UpdateStudyPlayerRole(newRole);
            //    }
            //}
            //if (changedProps.ContainsKey(PropertyKeyPlayerColor))
            //{
            //    if (TryParseColor(changedProps[PropertyKeyPlayerColor].ToString(), out var newColor))
            //    {
            //        this.UpdateStudyPlayerColor(newColor);
            //    }
            //}

            //if (changedProps.ContainsKey(PropertyKeyPlayerInvisibility))
            //{
            //    if (TryParseInvisibility(changedProps[PropertyKeyPlayerInvisibility].ToString(), out var newIsInvisibleState))
            //    {
            //        this.UpdateStudyPlayerInvisibilityState(newIsInvisibleState);
            //    }
            //}

            //if (changedProps.ContainsKey(PropertyKeyPlayerHearingScope))
            //{
            //    if (PlayerAudioHearingScope.TryParse(changedProps[PropertyKeyPlayerHearingScope].ToString(), out PlayerAudioHearingScope newHearScope))
            //    {
            //        this.UpdateStudyPlayerHearingScope(newHearScope);
            //    }
            //}

            //if (changedProps.ContainsKey(PropertyKeyPlayerLoggingParticipantId))
            //{
            //    this.UpdateLoggingParticipantId(changedProps[PropertyKeyPlayerLoggingParticipantId].ToString());
            //}
        }
    }

    void IInRoomCallbacks.OnMasterClientSwitched(Player newMasterClient)
    {
    }
    #endregion

    public static bool TryParseColor(string a_serializedColorProperty, out Color a_parsedColor)
    {
        return ColorUtility.TryParseHtmlString("#" + a_serializedColorProperty, out a_parsedColor);
    }

    public static bool TryParseInvisibility(string a_serializedInvisibilityProperty, out bool a_parsedInvisbilty)
    {
        return bool.TryParse(a_serializedInvisibilityProperty, out a_parsedInvisbilty);
    }

    public static bool TryParseHearingScope(string a_serializedHearingScopeProperty, out PlayerAudioHearingScope a_hearingScope)
    {
        return PlayerAudioHearingScope.TryParse(a_serializedHearingScopeProperty, out a_hearingScope);
    }

    public static bool TryParseRole(string a_serializedRoleProperty, out StudyPlayerRole a_role)
    {
        return StudyPlayerRole.TryParse(a_serializedRoleProperty, out a_role);
    }

    public static bool TryGetColorOfPlayer(Player a_p, out Color a_color)
    {
        a_color = Color.magenta;
        if (a_p.CustomProperties.TryGetValue(PropertyKeyPlayerColor, out var valObj))
        {
            return TryParseColor(valObj.ToString(), out a_color);
        }
        return false;
    }

    public static bool TryGetInvisiblityStateOfPlayer(Player a_p, out bool a_isInvisible)
    {
        a_isInvisible = false;
        if (a_p.CustomProperties.TryGetValue(PropertyKeyPlayerInvisibility, out var valObj))
        {
            return TryParseInvisibility(valObj.ToString(), out a_isInvisible);
        }
        return false;
    }

    public static bool TryGetRoleOfPlayer(Player a_p, out StudyPlayerRole a_role)
    {
        a_role = StudyPlayerRole.VISITOR;
        if (a_p.CustomProperties.TryGetValue(PropertyKeyPlayerRole, out var valObj))
        {
            return TryParseRole(valObj.ToString(), out a_role);
        }
        return false;
    }

    public static bool TryGetHearingScopeOfPlayer(Player a_p, out PlayerAudioHearingScope a_scope)
    {
        a_scope = PlayerAudioHearingScope.Nobody;
        if (a_p.CustomProperties.TryGetValue(PropertyKeyPlayerHearingScope, out var valObj))
        {
            return TryParseHearingScope(valObj.ToString(), out a_scope);
        }
        return false;
    }

    public static bool TryGetParticipantIdOfPlayer(Player a_p, out string a_pid)
    {
        a_pid = DefaultParticipantId;
        if (a_p.CustomProperties.TryGetValue(PropertyKeyPlayerLoggingParticipantId, out var valObj))
        {
            a_pid = valObj.ToString();
            return true;
        }
        return false;
    }

    #region IMatchmakingCallbacks

    void IMatchmakingCallbacks.OnFriendListUpdate(List<FriendInfo> friendList)
    {
    }

    void IMatchmakingCallbacks.OnCreatedRoom()
    {
    }

    void IMatchmakingCallbacks.OnCreateRoomFailed(short returnCode, string message)
    {
    }

    void IMatchmakingCallbacks.OnJoinedRoom()
    {
        RequestParticipantIdChange(PhotonNetwork.LocalPlayer.ActorNumber, this.LoggingParticipantId);
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
