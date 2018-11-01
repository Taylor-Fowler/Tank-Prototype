﻿using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using Photon.Pun;
using Photon.Realtime;

public class ViewRoom : MonoBehaviourPunCallbacks
{
    public Text RoomName;
    public Transform RoomPlayersListAnchor;
    public GameObject PlayerInListPrefab;

    private readonly Dictionary<int, GameObject> _playerObjects;

    public ViewRoom()
    {
        _playerObjects = new Dictionary<int, GameObject>();
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();

        foreach(var playerPair in _playerObjects)
        {
            Destroy(playerPair.Value);
        }

        _playerObjects.Clear();
    }

    #region PUN2 API
    public override void OnJoinedRoom()
    {
        ChangeRoomName();
        
        foreach(var playerPair in PhotonNetwork.CurrentRoom.Players)
        {
            AddPlayer(playerPair.Value);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        AddPlayer(newPlayer);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Destroy(_playerObjects[otherPlayer.ActorNumber]);
        _playerObjects.Remove(otherPlayer.ActorNumber);
    }
    #endregion

    private void ChangeRoomName()
    {
        RoomName.text = PhotonNetwork.CurrentRoom.CustomProperties["room_name"].ToString();
    }

    private void AddPlayer(Player player)
    {
        GameObject playerDetails = Instantiate(PlayerInListPrefab, RoomPlayersListAnchor);
        playerDetails.GetComponent<Text>().text = player.NickName;

        _playerObjects.Add(player.ActorNumber, playerDetails);
    }
}
