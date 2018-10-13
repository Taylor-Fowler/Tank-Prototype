using System.Collections;

using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class OurServerConnection : MonoBehaviourPunCallbacks
{
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

    public void MyJoinOrCreateRoom()
    {
        RoomOptions roomOptions = new RoomOptions();
        PhotonNetwork.JoinOrCreateRoom("a room", roomOptions, PhotonNetwork.CurrentLobby);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("OnJoinedRoom");
        PhotonNetwork.LoadLevel(1);
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