///////////////////////////////////////////////////////////////////////////
// CI6510 Optimised Game Programming    (Kingston University)            //
// Course Work 1 : Network Game with Plug-ins                            //
// Submission by Max Bryans (K1628007) and Taylor Fowler (K1612040)      //
// December 2018                                                         //
///////////////////////////////////////////////////////////////////////////
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;

public class MainMenuNav : MonoBehaviourPunCallbacks
{
    [System.Serializable]
    public class TextFieldAction
    {
        public GameObject Panel;
        public InputField TextField;
        public Text Feedback;
        public Button ConfirmButton;
        public Button CloseButton;

        private Coroutine _closeRoutine;

        public void Initialise()
        {
            ConfirmButton.onClick.AddListener(delegate 
            {
                ConfirmButton.interactable = false;
                CloseButton.interactable = false;
                GameController.Instance.PlayerManager.RegisterNewUser(TextField.text, ReceiveFeedback);
            });
            CloseButton.onClick.AddListener(delegate
            {
                Close();
            });
        }

        public void Open()
        {
            Panel.SetActive(true);
        }

        private void Close()
        {
            Panel.SetActive(false);
            TextField.text = "";
            Feedback.text = "";
            if(_closeRoutine != null)
            {
                GameController.Instance.StopCoroutine(_closeRoutine);
                _closeRoutine = null;
            }
        }

        private void ReceiveFeedback(string message, bool successful)
        {
            Feedback.text = message;
            CloseButton.interactable = true;

            if(!successful)
            {
                Feedback.color = Color.red;
                ConfirmButton.interactable = true;
            }
            else
            {
                Feedback.color = Color.green;
                _closeRoutine = GameController.Instance.StartCoroutine(GameController.Delay(2f, Close));
            }
        }
    };

    [SerializeField]
    private TextFieldAction _textFieldAction;

    public GameObject RegisterBlinker;
    public Text UserDataConnectionText;
    public Text ServerConnectionText;
    public Text ServerPlayerCountText;
    public Text ServerPlayersInRoomsCountText;
    public Text ServerPlayersOnMasterCountText;
    public LoadingPanel LoadingPanel;

    public Text KillsValue, DeathsValue, GamesPlayedValue, WinsValue, LossesValue;

    public GameObject ViewRoomCanvas;
    public GameObject ViewLobbyCanvas;
    public GameObject[] AllCanvases;

    #region UNITY API
    private void Awake()
    {
        if (PhotonNetwork.IsConnected)
        {
            OnConnectedToMaster();
            UpdateOnlinePlayerStatistics();

            if(GameController.Instance.PlayerManager.User.Player_ID == -1)
            {
                OnDeviceIdNotRegistered();
            }
            else
            {
                OnUserDataUpdate(GameController.Instance.PlayerManager.User.Username);
            }

            if(PhotonNetwork.InRoom)
            {
                OnJoinedRoom();
            }
            else
            {
                ActivateCanvas(ViewLobbyCanvas);
            }
        }
    }

    private void Start()
    {
        Messenger.AddListener("OnDeviceIdNotRegistered", OnDeviceIdNotRegistered);
        Messenger<string>.AddListener("OnUserDataUpdate", OnUserDataUpdate);
        Messenger<string>.AddListener("OnUserDataDownloadError", OnUserDataDownloadError);

        _textFieldAction.Initialise();
        RegisterBlinker.GetComponent<Button>().onClick.AddListener(_textFieldAction.Open);
        StartCoroutine(UpdateStatistics());
    }

    private void OnDestroy()
    {
        Messenger.RemoveListener("OnDeviceIdNotRegistered", OnDeviceIdNotRegistered);
        Messenger<string>.RemoveListener("OnUserDataUpdate", OnUserDataUpdate);
        Messenger<string>.RemoveListener("OnUserDataDownloadError", OnUserDataDownloadError);
        StopAllCoroutines();
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
        UpdateOnlinePlayerStatistics();
    }

    public override void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics)
    {
        UpdateOnlinePlayerStatistics();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        UpdateOnlinePlayerStatistics();
    }

    public override void OnJoinedRoom()
    {
        ActivateCanvas(ViewRoomCanvas);
    }

    public override void OnLeftRoom()
    {
        ActivateCanvas(ViewLobbyCanvas);
    }
    #endregion

    #region CUSTOM EVENTS
    private void OnDeviceIdNotRegistered()
    {
        UserDataConnectionText.text = "Guest";
        UserDataConnectionText.color = new Color(135f / 255f, 126f / 255f, 20f / 255f);
        RegisterBlinker.SetActive(true);
    }

    private void OnUserDataUpdate(string username)
    {
        UserDataConnectionText.text = username;
        UserDataConnectionText.color = Color.green;
        RegisterBlinker.SetActive(false);

        UserData user = GameController.Instance.PlayerManager.User;
        KillsValue.text = user.Kills.ToString();
        DeathsValue.text = user.Deaths.ToString();
        GamesPlayedValue.text = user.Games_Played.ToString();
        WinsValue.text = user.Wins.ToString();
        LossesValue.text = user.Losses.ToString();
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

    public void SearchForMatch(int roomSize)
    {
        Action cancelAction = GameController.Instance.NetworkManager.SearchForRoom(roomSize, FinishedSearching);
        LoadingPanel.StartLoading("Searching for a " + roomSize.ToString() + " player game", "Cancelled searching for a game", 3f, cancelAction);
    }

    private void FinishedSearching(bool found)
    {
        LoadingPanel.LoadingRoutineExternallyEnded();
        if(found)
        {
            LoadingPanel.TurnOffPanel("Found game, joining room", 1.5f);
        }
        else
        {
            LoadingPanel.TurnOffPanel("Could not find a game, returning to lobby", 2f);
        }
    }

    private void ActivateCanvas(GameObject canvas)
    {
        canvas.SetActive(true);

        foreach(var otherCanvas in AllCanvases)
        {
            if(otherCanvas != canvas)
            {
                otherCanvas.SetActive(false);
            }
        }
    }

    private IEnumerator UpdateStatistics()
    {
        while(true)
        {
            yield return new WaitForSeconds(5f);
            if(PhotonNetwork.IsConnected)
            {
                UpdateOnlinePlayerStatistics();
            }
        }
    }

    private void UpdateOnlinePlayerStatistics()
    {
        ServerPlayerCountText.text = "Total Players Online: " + PhotonNetwork.CountOfPlayers.ToString();
        ServerPlayersInRoomsCountText.text = "Players In Rooms: " + PhotonNetwork.CountOfPlayersInRooms.ToString();
        ServerPlayersOnMasterCountText.text = "Players In Lobbies: " + PhotonNetwork.CountOfPlayersOnMaster.ToString();
    }

}
