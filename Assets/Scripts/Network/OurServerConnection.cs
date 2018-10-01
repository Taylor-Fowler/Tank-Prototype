using System.Collections;

using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class OurServerConnection : MonoBehaviourPunCallbacks
{
    private Room _connectedRoom;

    private void Start()
    {
        DontDestroyOnLoad(this);
        // Debug.Log(PhotonNetwork.CurrentLobby); CurrentLobby -> Null
        ConnectToServer();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master");
        PhotonNetwork.JoinLobby();
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("OnCreatedRoom");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("OnJoinedRoom");
        _connectedRoom = PhotonNetwork.CurrentRoom;
    }

    public override void OnLeftRoom()
    {
        Debug.Log("OnLeftRoom");
    }

    private void ConnectToServer()
    {
        if (!PhotonNetwork.ConnectUsingSettings())
        {
            Debug.LogError("Error connecting to Server");
        }
    }
}