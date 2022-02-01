using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhotonConnectionBasedActive : MonoBehaviour
{
    public GameObject objectToToggle;
    public bool lastConnectedState;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (objectToToggle != null)
        {
            bool connected = PhotonNetwork.IsConnected && PhotonNetwork.InRoom;
            lastConnectedState = connected;
            if (objectToToggle.activeSelf == connected)
            {
                // obj visible when not connected
                objectToToggle.SetActive(!connected);
            }
        }
    }
}
