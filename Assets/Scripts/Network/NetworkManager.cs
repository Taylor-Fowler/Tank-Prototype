using System.Collections;

using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

namespace Network
{
    public class NetworkManager : MonoBehaviourPunCallbacks, IManager
    {
        public NetworkService NetworkService { get; private set; }
        
        public void Startup(NetworkService networkService)
        {
            NetworkService = networkService;
            ConnectToServer();
        }

        public override void OnConnectedToMaster()
        {
            Debug.Log("Connected to Master");
            StartCoroutine(NetworkService.DownloadUserData(GameController.Instance.DeviceID, GetUserData));
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

        private void GetUserData(NetworkResponseMessage response)
        {
            Debug.Log("Response Status" + response.Status.ToString());
            Debug.Log("Response Message" + response.Message);
        }

        
        private static bool ConnectToServer()
        {
            if (!PhotonNetwork.ConnectUsingSettings())
            {
                Debug.LogError("Error connecting to Server");
                return false;
            }

            return true;
        }


    }
}