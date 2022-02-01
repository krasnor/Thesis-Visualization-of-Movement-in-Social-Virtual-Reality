using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkPlayerSpawner : MonoBehaviourPunCallbacks
{
    private GameObject spawnedPlayerPrefab;
    private Transform spawnLocation;

    void Start()
    {
        spawnLocation = transform;
    }

    public override void OnJoinedRoom()
    {
        // local Player joined room

        base.OnJoinedRoom();

        PhotonNetwork.Instantiate("Network Player", spawnLocation.position, spawnLocation.rotation);
    }
    public override void OnLeftRoom()
    {
        base.OnLeftRoom();

        PhotonNetwork.Destroy(spawnedPlayerPrefab);
    }
}
