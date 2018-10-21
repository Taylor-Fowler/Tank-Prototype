using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class RoomDisplay : MonoBehaviourPunCallbacks
{
    public GameObject RoomNamesAnchor;
    public GameObject RoomCreatorsAnchor;
    public GameObject RoomPlayersAnchor;

    public GameObject RoomDetailsPrefab;

    public Dropdown NumberOfPlayersDropdown;
    public InputField RoomNameInput;

    private List<RoomInfo> _roomList;
    private List<GameObject> _spawnedRoomDetails;

    private void Start()
    {
        _spawnedRoomDetails = new List<GameObject>();
        _roomList = new List<RoomInfo>();
    }

    public void CreateRoom()
    {
        int maxPlayers = 2;
        int.TryParse(NumberOfPlayersDropdown.captionText.text, out maxPlayers);

        GameController.Instance.NetworkManager.CreatePublicRoom
        (
            GameController.Instance.PlayerManager.User, 
            RoomNameInput.text, 
            maxPlayers
        );
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
    }


}
