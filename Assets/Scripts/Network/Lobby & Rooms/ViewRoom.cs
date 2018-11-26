///////////////////////////////////////////////////////////////////////////
// CI6510 Optimised Game Programming    (Kingston University)            //
// Course Work 1 : Network Game with Plug-ins                            //
// Submission by Max Bryans (K1628007) and Taylor Fowler (K1612040)      //
// December 2018                                                         //
///////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;

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

    #region UNITY API
    private void Awake()
    {
        if (PhotonNetwork.InRoom)
        {
            OnJoinedRoom();
        }
    }
    #endregion

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
        Debug.Log("[ViewRoom] OnPlayerLeftRoom");
        Destroy(_playerObjects[otherPlayer.ActorNumber]);
        _playerObjects.Remove(otherPlayer.ActorNumber);
    }
    #endregion

    private void ChangeRoomName()
    {
        RoomName.text = Network.NetworkManager.RoomName(PhotonNetwork.CurrentRoom);
    }

    private void AddPlayer(Player player)
    {
        GameObject playerDetails = Instantiate(PlayerInListPrefab, RoomPlayersListAnchor);
        playerDetails.GetComponent<Text>().text = player.NickName;

        _playerObjects.Add(player.ActorNumber, playerDetails);
    }
}
