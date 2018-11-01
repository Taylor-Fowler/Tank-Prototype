using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using Photon.Realtime;

public class ViewLobby : MonoBehaviour
{
    public Transform RoomNamesAnchor;
    public Transform RoomCreatorsAnchor;
    public Transform RoomPlayersAnchor;
    public Transform RoomLockAnchor;
    public Transform RoomJoinAnchor;

    public GameObject RoomDetailsPrefab;
    public GameObject RoomDetailsBackgroundPrefab;
    public GameObject RoomJoinPrefab;
    public GameObject RoomLockPrefab;

    public Color DefaultBackgroundColour;
    public Color AlternateBackgroundColour;

    [SerializeField]
    private JoinRoom _joinRoom;

    private List<GameObject> _spawnedRoomDetails = new List<GameObject>();

    #region UNITY API
    private void Start()
    {
        GameController.Instance.NetworkManager.Started(OnNetworkManagerStarted);
    }

    private void OnDestroy()
    {
        if(GameController.Instance.NetworkManager.Status == ManagerStatus.Started)
        {
            GameController.Instance.NetworkManager.OnRoomCacheUpdate -= OnRoomCacheUpdate;
        }
    }
    #endregion

    #region CUSTOM EVENTS
    private void OnRoomCacheUpdate(List<RoomInfo> cachedRooms)
    {
        ClearRoomDetails();

        int i = 0;
        foreach (var room in cachedRooms)
        {
            UpdateRoomDetails(room, 
                (
                    i % 2 == 0 ? 
                    DefaultBackgroundColour : 
                    AlternateBackgroundColour
                )
            );

            i++;
        }
    }
    #endregion

    private void OnNetworkManagerStarted()
    {
        GameController.Instance.NetworkManager.OnRoomCacheUpdate += OnRoomCacheUpdate;
        OnRoomCacheUpdate(GameController.Instance.NetworkManager.CachedRooms);
    }

    private void ClearRoomDetails()
    {
        foreach (var go in _spawnedRoomDetails)
        {
            DestroyImmediate(go);
        }

        _spawnedRoomDetails.Clear();
    }

    private void UpdateRoomDetails(RoomInfo room, Color colour)
    {
        string roomName = (string)room.CustomProperties["room_name"];
        string roomHost = (string)room.CustomProperties["room_host"];
        //string deviceID = (string)room.CustomProperties["room_host_device_id"];


        AddRoomName(AddBackground(colour, RoomNamesAnchor), roomName);
        AddRoomCreator(AddBackground(colour, RoomCreatorsAnchor), roomHost);
        AddPlayerCount(AddBackground(colour, RoomPlayersAnchor), room.PlayerCount, room.MaxPlayers);

        if (room.CustomProperties.ContainsKey("room_pass"))
        {
            AddRoomPassword(AddBackground(colour, RoomLockAnchor));
            AddJoinButton(AddBackground(colour, RoomJoinAnchor), room.Name, (string)room.CustomProperties["room_pass"]);
        }
        else
        {
            AddBackground(colour, RoomLockAnchor);
            AddJoinButton(AddBackground(colour, RoomJoinAnchor), room.Name);
        }
    }

    private void AddRoomName(Transform parent, string roomName)
    {
        GameObject name = Instantiate(RoomDetailsPrefab, parent);
        name.GetComponent<Text>().text = roomName;
    }

    private void AddRoomCreator(Transform parent, string creator)
    {
        GameObject user = Instantiate(RoomDetailsPrefab, parent);
        user.GetComponent<Text>().text = creator;
    }

    private void AddPlayerCount(Transform parent, int playersInRoom, int maxPlayers)
    {
        GameObject playerCount = Instantiate(RoomDetailsPrefab, parent);
        playerCount.GetComponent<Text>().text = playersInRoom.ToString() + "/" + maxPlayers.ToString();
    }

    private void AddRoomPassword(Transform parent)
    {
        GameObject lockIcon = Instantiate(RoomLockPrefab, parent);
    }

    private void AddJoinButton(Transform parent, string roomName)
    {
        GameObject joinButton = Instantiate(RoomJoinPrefab, parent);
        joinButton.GetComponent<Button>().onClick.AddListener(
                _joinRoom.RegisterForAuthentication(roomName)
            );
    }

    private void AddJoinButton(Transform parent, string roomName, string password)
    {
        GameObject joinButton = Instantiate(RoomJoinPrefab, parent);
        joinButton.GetComponent<Button>().onClick.AddListener
            (
                _joinRoom.RegisterForAuthentication(roomName, password)
            );
    }

    private Transform AddBackground(Color colour, Transform parent)
    {
        GameObject background = Instantiate(RoomDetailsBackgroundPrefab, parent);
        background.GetComponent<Image>().color = colour;
        _spawnedRoomDetails.Add(background);

        return background.transform;
    }
}
