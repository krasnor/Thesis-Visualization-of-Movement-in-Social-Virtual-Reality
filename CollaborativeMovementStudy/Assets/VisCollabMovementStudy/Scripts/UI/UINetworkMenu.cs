using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UINetworkMenu : MonoBehaviour
{
    public NetworkConnectionManager NetworkConnectionManager;
    public Button BtnConnectMaster;
    public Button BtnConnectRoom;
    public Text ConnectionState;

    void Awake()
    {
        if (NetworkConnectionManager == null)
            throw new MissingComponentException("NetworkConnectionManager component not assigned.");
        if (BtnConnectMaster == null)
            throw new MissingComponentException("BtnConnectMaster component not assigned.");
        if (BtnConnectRoom == null)
            throw new MissingComponentException("BtnConnectRoom component not assigned.");
        if (ConnectionState == null)
            throw new MissingComponentException("ConnectionState component not assigned.");
    }

    // Start is called before the first frame update
    void Start()
    {
        BtnConnectMaster.gameObject.SetActive(false);
        BtnConnectRoom.gameObject.SetActive(false);

        BtnConnectMaster.onClick.AddListener(NetworkConnectionManager.DoConnectToMasterServer);
        BtnConnectRoom.onClick.AddListener(NetworkConnectionManager.DoConnectToRoom);
    }

    // Update is called once per frame
    void Update()
    {
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            ConnectionState.text = $"RoomId: {PhotonNetwork.CurrentRoom.Name}  Connected and in Room: true";
        }
        else
        {
            ConnectionState.text = "Connected and in Room: false";
        }

        // show as  long not connected && hide if connected or while trying to connect
        bool showBtnConnectMaster = !PhotonNetwork.IsConnected && !NetworkConnectionManager.TriesToConnectToMaster;

        // show when connected & not in room & not while trying to connect to room
        bool showBtnConnectRoom = (!NetworkConnectionManager.InRoom && PhotonNetwork.IsConnected && !NetworkConnectionManager.TriesToConnectToMaster && !NetworkConnectionManager.TriesToConnectToRoom);

        if (BtnConnectMaster.gameObject.activeSelf != showBtnConnectMaster)
            BtnConnectMaster.gameObject.SetActive(showBtnConnectMaster);
        if (BtnConnectRoom.gameObject.activeSelf != showBtnConnectRoom)
            BtnConnectRoom.gameObject.SetActive(showBtnConnectRoom);
    }
}
