using Assets.VisCollabMovementStudy.Scripts;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NetworkConnectionManager : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private bool m_automaticConnectOnStartup = false;

    private bool m_triesToConnectToMaster = false;
    private bool m_triesToConnectToRoom = false;
    private bool m_inRoom = false;

    public bool TriesToConnectToMaster { get => m_triesToConnectToMaster; private set => m_triesToConnectToMaster = value; }
    public bool TriesToConnectToRoom { get => m_triesToConnectToRoom; private set => m_triesToConnectToRoom = value; }
    public bool InRoom { get => m_inRoom; private set => m_inRoom = value; }
    public bool AutomaticConnectOnStartup { get => m_automaticConnectOnStartup; private set => m_automaticConnectOnStartup = value; }

    void Start()
    {
        m_triesToConnectToMaster = false;
        m_triesToConnectToRoom = false;
        if (m_automaticConnectOnStartup)
        {
            DoConnectToMasterServer();
        }
    }

    public override void OnEnable()
    {
        base.OnEnable();
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
    }

    public void DoConnectToMasterServer()
    {
        m_triesToConnectToMaster = true;

        //Settings (all optional and only for tutorial purpose)
        PhotonNetwork.OfflineMode = false;           //true would "fake" an online connection
        PhotonNetwork.NickName = "PlayerName";       //to set a player name
        PhotonNetwork.AutomaticallySyncScene = true; //to call PhotonNetwork.LoadLevel()
        PhotonNetwork.GameVersion = "v1";            //only people with the same game version can play together

        //PhotonNetwork.ConnectToMaster(ip,port,appid); //manual connection
        PhotonNetwork.ConnectUsingSettings();           //automatic connection based on the config file in Photon/PhotonUnityNetworking/Resources/PhotonServerSettings.asset
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        m_triesToConnectToMaster = false;
        Debug.Log("NetworkConnectionManager:  Connected to Master Server");

        if (m_automaticConnectOnStartup)
        {
            DoConnectToRoom();
        }
    }

    public void DoConnectToRoom()
    {
        if (!PhotonNetwork.IsConnected)
            return;

        m_triesToConnectToRoom = true;

        Debug.Log($"NetworkConnectionManager: Connecting to Room. Count Current Rooms: {PhotonNetwork.CountOfRooms}. Offline Mode: {PhotonNetwork.OfflineMode}");

        //PhotonNetwork.CreateRoom("Peter's Game 1"); //Create a specific Room - Error: OnCreateRoomFailed
        //PhotonNetwork.JoinRoom("Peter's Game 1");   //Join a specific Room   - Error: OnJoinRoomFailed  
        PhotonNetwork.JoinRandomRoom();               //Join a random Room     - Error: OnJoinRandomRoomFailed 
    }

    public void TriggerRemoteCloseForPlayer(int a_actorId)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }
        Debug.LogWarning("NetworkConnectionManager: TriggerRemoteCloseForPlayer ActorId: " + a_actorId);
        foreach (var p in PhotonNetwork.PlayerList)
        {
            if (p.ActorNumber == a_actorId)
            {
                // PhotonNetwork.CloseConnection() will only close connection, but not close Application on Client side (as currently OnPlayerLeft-Callback is not handeled)
                //PhotonNetwork.CloseConnection(p);

                // requires PhotonView
                //this.photonView.RPC("RemoteCloseApp", p, new object[] { a_actorId });

                object[] content = new object[] { a_actorId };
                RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All }; // You would have to set the Receivers to All in order to receive this event on the local client as well
                PhotonNetwork.RaiseEvent(CollabStudyNetworkEventCodes.RequestRemoteAppCloseEventCode, content, raiseEventOptions, SendOptions.SendReliable);

                return;
            }
        }
    }

    #region PunCallbacks

    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
        m_triesToConnectToMaster = false;
        m_triesToConnectToRoom = false;
        Debug.Log("NetworkConnectionManager: Disconnected cause: " + cause);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        base.OnJoinRandomFailed(returnCode, message);
        //no room available
        //create a room (null as a name means "does not matter")
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 20 });
        m_inRoom = false;
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        base.OnCreateRoomFailed(returnCode, message);
        Debug.Log(message);
        base.OnCreateRoomFailed(returnCode, message);
        m_triesToConnectToRoom = false;
        m_inRoom = false;
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        m_inRoom = true;
        m_triesToConnectToRoom = false;
        Debug.Log("NetworkConnectionManager: # Joined Room Master: " + PhotonNetwork.IsMasterClient + " | Players In Room: " + PhotonNetwork.CurrentRoom.PlayerCount + " | RoomName: " + PhotonNetwork.CurrentRoom.Name);
        //SceneManager.LoadScene("Network");
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("NetworkConnectionManager: # New Player joined the room");
        base.OnPlayerEnteredRoom(newPlayer);
    }



    #endregion

    #region Event

    private void OnEvent(EventData photonEvent)
    {
        try
        {
            byte eventCode = photonEvent.Code;

            if (eventCode == CollabStudyNetworkEventCodes.RequestRemoteAppCloseEventCode)
            {
                object[] data = (object[])photonEvent.CustomData;
                int actorId = (int)data[0];

                if (PhotonNetwork.LocalPlayer.ActorNumber == actorId)
                {
                    Debug.LogWarning("NetworkConnectionManager: This client is told to close its application");
                    Application.Quit();
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("NetworkConnectionManager: An error occured while handling remote close. Error: " + ex);
        }
    }
    #endregion
}
