using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using Photon.Realtime;

public class ViewLobby : MonoBehaviour
{
    public GameObject RoomNamesAnchor;
    public GameObject RoomCreatorsAnchor;
    public GameObject RoomPlayersAnchor;
    public GameObject RoomLockAnchor;
    public GameObject RoomJoinAnchor;

    public GameObject RoomDetailsPrefab;
    public GameObject RoomJoinPrefab;
    public GameObject RoomLockPrefab;

    [SerializeField]
    private JoinRoom _joinRoom;

    private List<GameObject> _spawnedRoomDetails = new List<GameObject>();

    #region UNITY API
    public void OnEnable()
    {
        GameController.Instance.NetworkManager.OnRoomCacheUpdate += OnRoomCacheUpdate;
        OnRoomCacheUpdate(GameController.Instance.NetworkManager.CachedRooms);
    }

    public void OnDisable()
    {
        GameController.Instance.NetworkManager.OnRoomCacheUpdate -= OnRoomCacheUpdate;
    }
    #endregion

    #region CUSTOM EVENTS
    private void OnRoomCacheUpdate(List<RoomInfo> cachedRooms)
    {
        ClearRoomDetails();
        foreach (var room in cachedRooms)
        {
            UpdateRoomDetails(room);
        }
    }
    #endregion

    private void ClearRoomDetails()
    {
        foreach (var go in _spawnedRoomDetails)
            Destroy(go);

        _spawnedRoomDetails.Clear();
    }

    private void UpdateRoomDetails(RoomInfo room)
    {
        Debug.Log("updateRoomDetails");
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
        if (room.CustomProperties.ContainsKey("room_pass"))
        {
            joinButton.GetComponent<Button>().onClick.AddListener(
                _joinRoom.RegisterForAuthentication(room.Name, (string)room.CustomProperties["room_pass"])
            );
        }
        else
        {
            joinButton.GetComponent<Button>().onClick.AddListener(
                _joinRoom.RegisterForAuthentication(room.Name)
            );
        }

        _spawnedRoomDetails.Add(joinButton);
    }
}
