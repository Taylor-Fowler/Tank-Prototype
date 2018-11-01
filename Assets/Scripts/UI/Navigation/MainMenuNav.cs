using UnityEngine;
using UnityEngine.UI;

using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;

public class MainMenuNav : MonoBehaviourPunCallbacks
{
    public Text UserDataConnectionText;
    public Text ServerConnectionText;
    public Text ServerPlayerCountText;
    public Text ServerPlayersInRoomsCountText;
    public Text ServerPlayersOnMasterCountText;

    public GameObject ViewRoomCanvas;
    public GameObject ViewLobbyCanvas;
    public GameObject[] AllCanvases;

    #region UNITY API
    private void Start()
    {
        Messenger.AddListener("OnDeviceIdNotRegistered", OnDeviceIdNotRegistered);
        Messenger<string>.AddListener("OnUserDataUpdate", OnUserDataUpdate);
        Messenger<string>.AddListener("OnUserDataDownloadError", OnUserDataDownloadError);
    }

    private void OnDestroy()
    {
        Messenger.RemoveListener("OnDeviceIdNotRegistered", OnDeviceIdNotRegistered);
        Messenger<string>.RemoveListener("OnUserDataUpdate", OnUserDataUpdate);
        Messenger<string>.RemoveListener("OnUserDataDownloadError", OnUserDataDownloadError);
    }
    #endregion

    #region PUN2 API
    public override void OnConnectedToMaster()
    {
        ServerConnectionText.text = "Connected to Server";
        ServerConnectionText.color = Color.green;
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("[MainMenuNav] OnJoinedLobby");

        UpdateOnlinePlayerStatistics();
    }

    public override void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics)
    {
        Debug.Log("[MainMenuNav] OnLobbyStatisticsUpdate");

        UpdateOnlinePlayerStatistics();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log("[MainMenuNav] OnRoomListUpdate");

        UpdateOnlinePlayerStatistics();
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("[MainMenuNav] OnJoinedRoom");

        ViewRoomCanvas.SetActive(true);

        foreach(var canvas in AllCanvases)
        {
            if(canvas != ViewRoomCanvas)
            {
                canvas.SetActive(false);
            }
        }
    }

    public override void OnLeftRoom()
    {
        ViewLobbyCanvas.SetActive(true);

        foreach (var canvas in AllCanvases)
        {
            if (canvas != ViewLobbyCanvas)
            {
                canvas.SetActive(false);
            }
        }
    }
    #endregion

    #region CUSTOM EVENTS
    private void OnDeviceIdNotRegistered()
    {
        // TODO: Add a registration icon/button that takes to a canvas to
        //       enter a username for the device ID.
        UserDataConnectionText.text = "Not Registered";
        UserDataConnectionText.color = Color.yellow;
    }

    private void OnUserDataUpdate(string username)
    {
        UserDataConnectionText.text = username;
        UserDataConnectionText.color = Color.green;
    }

    private void OnUserDataDownloadError(string error)
    {
        // TODO: Add an error icon after the text that can be hovered
        //       which when hovered will contain the error message.
        UserDataConnectionText.text = "Error";
        UserDataConnectionText.color = Color.red;
    }
    #endregion

    public void QuitApplication()
    {
        Application.Quit();
    }

    private void UpdateOnlinePlayerStatistics()
    {
        ServerPlayerCountText.text = "Total Players Online: " + PhotonNetwork.CountOfPlayers.ToString();
        ServerPlayersInRoomsCountText.text = "Players In Rooms: " + PhotonNetwork.CountOfPlayersInRooms.ToString();
        ServerPlayersOnMasterCountText.text = "Players In Lobbies: " + PhotonNetwork.CountOfPlayersOnMaster.ToString();
    }

}
