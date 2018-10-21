using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


using Photon.Pun;
using Photon.Realtime;

public class RoomDisplay : MonoBehaviourPunCallbacks
{
    [System.Serializable]
    public class RoomAuthenticationPrefab
    {
        public GameObject Prefab;
        public InputField PasswordInput;
        public Button ConfirmButton;
        public Button CloseButton;

        public void Initialise()
        {
            CloseButton.onClick.AddListener(delegate
            {
                Close();
            });
        }

        public void Close()
        {
            Prefab.SetActive(false);
            PasswordInput.text = "";
            ConfirmButton.onClick.RemoveAllListeners();
        }

        public void Open(UnityAction callback, string roomPassword)
        {
            ConfirmButton.onClick.AddListener(delegate 
            {
                Confirm(callback, roomPassword);
            });

            Prefab.SetActive(true);
        }

        private void Confirm(UnityAction callback, string roomPassword)
        {
            if(PasswordInput.text == roomPassword)
            {
                Close();
                callback();
            }
        }
    };


    public GameObject RoomNamesAnchor;
    public GameObject RoomCreatorsAnchor;
    public GameObject RoomPlayersAnchor;
    public GameObject RoomLockAnchor;
    public GameObject RoomJoinAnchor;

    public GameObject RoomDetailsPrefab;
    public GameObject RoomJoinPrefab;
    public GameObject RoomLockPrefab;

    public RoomAuthenticationPrefab RoomAuthenticationObjects;

    public InputField RoomNameInput;
    public InputField PrivateRoomPassword;

    private List<GameObject> _spawnedRoomDetails;
    private List<RoomInfo> _roomList;

    private int _maxPlayers = 2;
    private bool _isPrivateRoom = false;

    private void Start()
    {
        _spawnedRoomDetails = new List<GameObject>();
        _roomList = new List<RoomInfo>();
        RoomAuthenticationObjects.Initialise();
    }

    public void TogglePrivateRoom(Toggle toggle)
    {
        _isPrivateRoom = toggle.isOn;

        if(_isPrivateRoom)
        {
            PrivateRoomPassword.interactable = true;
        }
        else
        {
            PrivateRoomPassword.interactable = false;
        }
    }

    public void SetMaxPlayers(Dropdown dropdown)
    {
        int.TryParse(dropdown.captionText.text, out _maxPlayers);
    }

    public void CreateRoom()
    {
        if(_isPrivateRoom)
        {
            GameController.Instance.NetworkManager.CreatePrivateRoom
            (
                GameController.Instance.PlayerManager.User,
                RoomNameInput.text,
                _maxPlayers,
                PrivateRoomPassword.text
            );
        }
        else
        {
            GameController.Instance.NetworkManager.CreatePublicRoom
            (
                GameController.Instance.PlayerManager.User,
                RoomNameInput.text,
                _maxPlayers
            );
        }
    }

    public void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log("OnRoomListUpdate");

        ClearRoomDetails();

        foreach(var room in roomList)
        {
            int index = 0;
            if((index = _roomList.IndexOf(room)) != -1)
            {
                if(room.PlayerCount == 0)
                {
                    _roomList.RemoveAt(index);
                }
                else
                {
                    _roomList[index] = room;
                }
            }
            else
            {
                _roomList.Add(room);
            }
        }
       
        foreach(var room in _roomList)
        {
            SpawnRoomDetails(room);
        }
    }


    private void ClearRoomDetails()
    {
        foreach(var go in _spawnedRoomDetails)
            Destroy(go);

        _spawnedRoomDetails.Clear();
    }

    private void SpawnRoomDetails(RoomInfo room)
    {
        GameObject name = Instantiate(RoomDetailsPrefab, RoomNamesAnchor.transform);
        name.GetComponent<Text>().text = (string)room.CustomProperties["room_name"];
        
        _spawnedRoomDetails.Add(name);

        GameObject user = Instantiate(RoomDetailsPrefab, RoomCreatorsAnchor.transform);
        user.GetComponent<Text>().text = (string)room.CustomProperties["room_host"];

        _spawnedRoomDetails.Add(user);

        GameObject playerCount = Instantiate(RoomDetailsPrefab, RoomPlayersAnchor.transform);
        playerCount.GetComponent<Text>().text = room.PlayerCount.ToString() + "/" + room.MaxPlayers.ToString();

        _spawnedRoomDetails.Add(playerCount);

        GameObject joinButton = Instantiate(RoomJoinPrefab, RoomJoinAnchor.transform);
        if(room.CustomProperties.ContainsKey("room_pass"))
        {
            joinButton.GetComponent<Button>().onClick.AddListener(delegate
            {
                RoomAuthenticationObjects.Open(
                    delegate
                    {
                        JoinRoom(room.Name);
                    },
                    (string)room.CustomProperties["room_pass"]
                );
            });
        }
        else
        {
            joinButton.GetComponent<Button>().onClick.AddListener(delegate
            {
                JoinRoom(room.Name);
            });
        }

        _spawnedRoomDetails.Add(joinButton);
    }


}
