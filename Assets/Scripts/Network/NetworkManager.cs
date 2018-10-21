using UnityEngine;

using Photon.Pun;
using Photon.Realtime;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;

namespace Network
{
    public class NetworkManager : MonoBehaviourPunCallbacks, IManager
    {
        public NetworkService NetworkService { get; private set; }
        public ManagerStatus Status { get; private set; }
        
        public void Startup(NetworkService networkService)
        {
            Status = ManagerStatus.Initializing;

            NetworkService = networkService;
            ConnectToServer();
        }

        public override void OnConnectedToMaster()
        {
            Debug.Log("Connected to Master");

            Status = ManagerStatus.Started;
            PhotonNetwork.JoinLobby();
        }

        public override void OnCreatedRoom()
        {
            Debug.Log("OnCreatedRoom");
        }

        public void CreatePublicRoom(UserData callingUser, string roomName, int maxPlayers)
        {
            RoomOptions roomOptions = new RoomOptions
            {
                CustomRoomPropertiesForLobby = new string[] { "room_host", "room_name" },
                CustomRoomProperties = new PhotonHashtable()
                {
                    { "room_host", callingUser.Username },
                    { "room_name", roomName }
                },
                MaxPlayers = (byte)maxPlayers,
                IsVisible = true,
                IsOpen = true
            };


            PhotonNetwork.JoinOrCreateRoom(callingUser.Username + callingUser.Device_ID, roomOptions, PhotonNetwork.CurrentLobby);
        }

        public void CreatePrivateRoom(UserData callingUser, string roomName, int maxPlayers, string roomPassword)
        {
            RoomOptions roomOptions = new RoomOptions
            {
                CustomRoomPropertiesForLobby = new string[] { "room_host", "room_name", "room_pass" },
                CustomRoomProperties = new PhotonHashtable()
                {
                    { "room_host", callingUser.Username },
                    { "room_name", roomName },
                    { "room_pass", roomPassword }
                },
                MaxPlayers = (byte)maxPlayers,
                IsVisible = true,
                IsOpen = true
            };


            PhotonNetwork.JoinOrCreateRoom(callingUser.Username + callingUser.Device_ID, roomOptions, PhotonNetwork.CurrentLobby);
        }

        public void StartGame()
        {
            PhotonNetwork.LoadLevel(1);
        }

        public override void OnJoinedRoom()
        {
            Debug.Log("OnJoinedRoom");
        }

        public override void OnLeftRoom()
        {
            Debug.Log("OnLeftRoom");
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