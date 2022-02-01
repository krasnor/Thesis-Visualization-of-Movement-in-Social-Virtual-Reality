using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugPlayerList : MonoBehaviourPunCallbacks
{

    public Text playerlistText;

    public GameObject PrefabPlayerRow;
    public GameObject PlayerListContent;
    public NetworkedPlayerSettings playerSettings;

    // Start is called before the first frame update
    void Start()
    {
    }


    // Update is called once per frame
    void Update()
    {
        string text = "Actor | Master | cProps | pHrScp | UserId";
        //string clientMark = "*";
        foreach (Player p in PhotonNetwork.PlayerList)
        {
            //if(p.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
            //{
            //    Debug.Log($"not different: p.ActorNumber: {p.ActorNumber} PhotonNetwork.LocalPlayer.ActorNumber {PhotonNetwork.LocalPlayer.ActorNumber}");
            //    text += $"\n    {clientMark}";
            //}
            //else
            //{
            //    Debug.Log($"different: p.ActorNumber: {p.ActorNumber} PhotonNetwork.LocalPlayer.ActorNumber {PhotonNetwork.LocalPlayer.ActorNumber}");
            //    text += "\n    ";
            //}
            //    1    True          01291e1e-89b5-4a7b-913a-64425ec2c9c3
            text += $"\n    {p.ActorNumber}      {p.IsMasterClient}          {p.CustomProperties.Count}        {p.CustomProperties["pHrScp"]}        {p.UserId}";
            
            // userId empty -> set PublishUserId = true when creating room
        }
        playerlistText.text = text;
    }

    public void RefreshPlayerList()
    {
        var players = PhotonNetwork.PlayerList;
        ClearUIList();

        for (int i = 0; i < players.Length; i++)
        {
            AddRow(players[i]);
        }
    }

    private List<GameObject> Rows = new List<GameObject>();

    void ClearUIList()
    {
        foreach (GameObject row in Rows)
        {
            row.GetComponent<UIPlayerEntryRow>().OnMasterRequest.RemoveAllListeners();
            row.GetComponent<UIPlayerEntryRow>().OnRoleChange.RemoveAllListeners();
            Destroy(row);
        }
        Rows.Clear();
    }

    void AddRow(Player a_p)
    {
        GameObject row = Instantiate(PrefabPlayerRow, PlayerListContent.transform) as GameObject;
        Rows.Add(row);
        //row.SetActive(true);

        string roleDisplayString = "";
        if (NetworkedPlayerSettings.TryGetRoleOfPlayer(a_p, out var playersRole)){
            switch (playersRole)
            {
                case StudyPlayerRole.VISITOR:
                    roleDisplayString = "Vis";
                    break;
                case StudyPlayerRole.GUIDE:
                    roleDisplayString = "GD";
                    break;
                case StudyPlayerRole.SUPERVISOR:
                    roleDisplayString = "Sup";
                    break;
            }
        }

        string currentColor = a_p.CustomProperties.ContainsKey(NetworkedPlayerSettings.PropertyKeyPlayerColor) ? a_p.CustomProperties[NetworkedPlayerSettings.PropertyKeyPlayerColor].ToString() : "";
        string pid = a_p.CustomProperties.ContainsKey(NetworkedPlayerSettings.PropertyKeyPlayerLoggingParticipantId) ? a_p.CustomProperties[NetworkedPlayerSettings.PropertyKeyPlayerLoggingParticipantId].ToString() : "";

        string hearingScope = "";
        if (NetworkedPlayerSettings.TryGetHearingScopeOfPlayer(a_p, out var playersHearingScope))
        {
            switch (playersHearingScope)
            {
                case PlayerAudioHearingScope.Everyone:
                    hearingScope = "evr";
                    break;
                case PlayerAudioHearingScope.Nobody:
                    hearingScope = "no";
                    break;
                case PlayerAudioHearingScope.SupervisorOnly:
                    hearingScope = "sup";
                    break;
            }
        }

        bool isInvisible = false;
        if(!NetworkedPlayerSettings.TryGetInvisiblityStateOfPlayer(a_p, out isInvisible))
        {
            isInvisible = false; // just to 100% sure
        }

        // TODO instead of parsing here do it in the PlayerRow
        row.GetComponent<UIPlayerEntryRow>().SetValues(a_p.ActorNumber, a_p.UserId, a_p.IsMasterClient, roleDisplayString, currentColor, pid, hearingScope, isInvisible);
        row.GetComponent<UIPlayerEntryRow>().OnMasterRequest.AddListener(PlayerListEntryOnMasterRequest);
        row.GetComponent<UIPlayerEntryRow>().OnRoleChange.AddListener(PlayerListEntryOnRoleRequest);

        //row.transform.SetParent(PlayerListContent.transform);
    }


    void PlayerListEntryOnMasterRequest(int a_actorId)
    {
        Debug.Log("PlayerListEntryOnMasterRequest for Actor " + a_actorId);
        playerSettings.RequestMaster(a_actorId);
    }

    void PlayerListEntryOnRoleRequest(int a_actorId, StudyPlayerRole a_role)
    {
        Debug.Log("PlayerListEntryOnRoleRequest for Actor " + a_actorId + " role: " + a_role);
        playerSettings.RequestRoleChange(a_actorId, a_role);
    }

    public void AddcProp()
    {
        foreach (Player p in PhotonNetwork.PlayerList)
        {
            ExitGames.Client.Photon.Hashtable setFoo = new ExitGames.Client.Photon.Hashtable()
            { { "Test", UnityEngine.Random.Range(1, 1000.0f) },
               { "pSettings", StudyPlayerRole.GUIDE },
               { "pColor", ColorUtility.ToHtmlStringRGBA(Color.red)}
    };
            p.SetCustomProperties(setFoo);
        }
    }

    public void ListProp()
    {
        foreach (Player p in PhotonNetwork.PlayerList)
        {
            ExitGames.Client.Photon.Hashtable setFoo = new ExitGames.Client.Photon.Hashtable() { { "Test", 123 } };
            Debug.Log($"Actor: {p.ActorNumber} Prop'Test': {p.CustomProperties["Test"]}");
        }
    }

    public void setLocalPlayerAsMaster()
    {
        Player localPlayer = PhotonNetwork.LocalPlayer;
        PhotonNetwork.SetMasterClient(localPlayer);
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        // Local player joined room
        RefreshPlayerList();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);

        // Remote player joined room (not this player)
        RefreshPlayerList();
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);

        // Remote player left room (not this player)
        RefreshPlayerList();
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        base.OnPlayerPropertiesUpdate(targetPlayer, changedProps);

        RefreshPlayerList();
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        base.OnMasterClientSwitched(newMasterClient);

        RefreshPlayerList();
    }

}
