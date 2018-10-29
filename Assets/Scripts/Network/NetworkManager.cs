using System.Collections.Generic;
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

        public List<RoomInfo> CachedRooms { get; private set; }

        public delegate void RoomCacheUpdate(List<RoomInfo> rooms);
        public RoomCacheUpdate OnRoomCacheUpdate;

        #region PUN2 API
        public override void OnConnectedToMaster()
        {
            Debug.Log("[NetworkManager] OnConnectedToMaster");
            Status = ManagerStatus.Started;

            PhotonNetwork.JoinLobby();
        }

        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            Debug.Log("[NetworkManager] OnRoomListUpdate");

            // Loop through all the currently cached rooms and REMOVE,
            // UPDATE or ADD rooms based on the updated room list given.
            foreach (var room in roomList)
            {
                int cachedIndex = CachedRooms.IndexOf(room);
                bool valid = room.PlayerCount != 0;

                if (cachedIndex != -1)
                {
                    if (valid)
                    {
                        CachedRooms[cachedIndex] = room;
                    }
                    else
                    {
                        CachedRooms.RemoveAt(cachedIndex);
                    }
                }
                else if (valid)
                {

                    CachedRooms.Add(room);
                }
            }

            if(OnRoomCacheUpdate != null)
            {
                OnRoomCacheUpdate(CachedRooms);
            }
        }

        public override void OnCreatedRoom()
        {
            Debug.Log("[NetworkManager] OnCreatedRoom");
        }

        public override void OnJoinedRoom()
        {
            Debug.Log("[NetworkManager] OnJoinedRoom");
        }

        public override void OnLeftRoom()
        {
            Debug.Log("[NetworkManager] OnLeftRoom");
        }
        #endregion

        public void Startup(NetworkService networkService)
        {
            Status = ManagerStatus.Initializing;

            NetworkService = networkService;
            CachedRooms = new List<RoomInfo>();
            ConnectToServer();
        }

        public void CreatePublicRoom(string roomName, int maxPlayers)
        {
            UserData callingUser = GameController.Instance.PlayerManager.User;
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

        public void CreatePrivateRoom(string roomName, int maxPlayers, string roomPassword)
        {
            UserData callingUser = GameController.Instance.PlayerManager.User;
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

        public void JoinRoom(string roomName)
        {
            PhotonNetwork.JoinRoom(roomName);
        }

        public void StartGame()
        {
            PhotonNetwork.LoadLevel(1);
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