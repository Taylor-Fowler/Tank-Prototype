///////////////////////////////////////////////////////////////////////////
// CI6510 Optimised Game Programming    (Kingston University)            //
// Course Work 1 : Network Game with Plug-ins                            //
// Submission by Max Bryans (K1628007) and Taylor Fowler (K1612040)      //
// December 2018                                                         //
///////////////////////////////////////////////////////////////////////////
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using Photon.Realtime;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;

namespace Network
{
    public class NetworkManager : MonoBehaviourPunCallbacks, IManager
    {
        #region IMANAGER IMPLEMENTATION
        public NetworkService NetworkService { get; private set; }
        public ManagerStatus Status { get; private set; }

        public void Startup(NetworkService networkService)
        {
            Debug.Log("[NetworkManager] Startup");
            Status = ManagerStatus.Initializing;

            NetworkService = networkService;
            CachedRooms = new List<RoomInfo>();

            GameController.Instance.Event_OnGameOver += OnGameOver;
            ConnectToServer();
        }

        public void Restart()
        {
            if(!PhotonNetwork.IsConnected)
            {
                ConnectToServer();
            }
        }

        public void Shutdown()
        {
            GameController.Instance.Event_OnGameOver -= OnGameOver;
        }
        #endregion

        #region STATIC METHODS
        public static string RoomName(RoomInfo room)
        {
            if (room.CustomProperties.ContainsKey("room_name"))
            {
                return (string)room.CustomProperties["room_name"];
            }
            return "";
        }

        public static bool RoomJoinable(RoomInfo room)
        {
            return room.IsOpen && room.IsVisible && room.PlayerCount != room.MaxPlayers && !room.CustomProperties.ContainsKey("room_pass");
        }
        #endregion  

        public List<RoomInfo> CachedRooms { get; private set; }

        public delegate void RoomCacheUpdate(List<RoomInfo> rooms);
        public RoomCacheUpdate OnRoomCacheUpdate;

        public delegate void NetworkManagerStarted();
        private NetworkManagerStarted _onNetworkManagerStarted;

        public PlayerController PlayerPreFab;

        public bool DevAutoJoin = false;


        #region PUN2 API
        public override void OnConnectedToMaster()
        {
            Debug.Log("[NetworkManager] OnConnectedToMaster");

            Status = ManagerStatus.Started;

            if (_onNetworkManagerStarted != null)
            {
                _onNetworkManagerStarted();
                _onNetworkManagerStarted = null;
            }

            if (DevAutoJoin)
            {
                Debug.Log("[NetworkManager] AUTOJOIN: Trying to Join Random Room");
                PhotonNetwork.AutomaticallySyncScene = true;
                PhotonNetwork.JoinRandomRoom(); // failure will call OnJoinRandomFailed() ... where we will create one
            }
            else
            {
                Debug.Log("[NetworkManager] Trying to Join Lobby");
                PhotonNetwork.JoinLobby();
            }
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            Debug.Log("[Network Manager] AUTOJOIN: No random room: so ... \nCalling: CreatePublicRoom()");
            CreatePublicRoom("Dev for 2", 2);
        }


        public override void OnJoinedLobby()
        {
            Debug.Log("[NetworkManager] OnJoinedLobby");
            PhotonNetwork.AutomaticallySyncScene = true;
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

        public override void OnLeftRoom()
        {
            GameController.Instance.LeftPostGame();
        }

        #endregion
        public void JoinRoom(string roomName)
        {
            PhotonNetwork.JoinRoom(roomName);
        }

        public void StartGame()
        {
            PhotonNetwork.LoadLevel("Map for " + PhotonNetwork.CurrentRoom.MaxPlayers.ToString());
        }

        public void Started(NetworkManagerStarted callback)
        {
            if(Status != ManagerStatus.Started)
            {
                _onNetworkManagerStarted += callback;
            }
            else
            {
                callback();
            }
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
                PublishUserId = true,
                MaxPlayers = (byte)maxPlayers,
                IsVisible = true,
                IsOpen = true
            };

            PhotonNetwork.JoinOrCreateRoom(callingUser.Username + callingUser.Device_ID + roomName, roomOptions, PhotonNetwork.CurrentLobby);
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
                PublishUserId = true,
                MaxPlayers = (byte)maxPlayers,
                IsVisible = true,
                IsOpen = true
            };

            PhotonNetwork.JoinOrCreateRoom(callingUser.Username + callingUser.Device_ID + roomName, roomOptions, PhotonNetwork.CurrentLobby);
        }

        public Action SearchForRoom(int roomSize, Action<bool> callback)
        {
            Coroutine routine = StartCoroutine(SearchForRoom(roomSize, 5f, callback));
            return delegate { StopCoroutine(routine); };
        }

        private IEnumerator SearchForRoom(int roomSize, float timeTillGiveUp, Action<bool> callback)
        {
            do
            {
                yield return null;

                foreach(var room in CachedRooms)
                {
                    if(RoomJoinable(room) && room.MaxPlayers == roomSize)
                    {
                        JoinRoom(room.Name);
                        callback(true);
                        yield break;
                    }
                }

                timeTillGiveUp -= Time.deltaTime;
            } while(timeTillGiveUp > 0f);

            callback(false);
        }

        #region CUSTOM EVENT RESPONSES
        private void OnGameOver(InGameVariables winningPlayer)
        {
            StartCoroutine(Wait(PhotonNetwork.Time + 5.0, delegate
            {
                GameController.Instance.Reset();
            }));
        }
        #endregion

        private IEnumerator Wait(double waitUntil, Action callback)
        {
            while(PhotonNetwork.Time < waitUntil)
            {
                yield return null;
            }
            callback();
        }

        private static bool ConnectToServer()
        {
            Debug.Log("[NetworkManager] ConnectToServer");

            if (!PhotonNetwork.ConnectUsingSettings())
            {
                Debug.LogError("Error connecting to Server");
                return false;
            }
            return true;
        }
    }
}