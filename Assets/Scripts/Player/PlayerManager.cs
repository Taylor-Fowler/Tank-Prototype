using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;
using Network;

public class PlayerManager : MonoBehaviourPunCallbacks, IManager
{
#if UNITY_EDITOR
    // Only included to view the serialised user data inside of the
    // editor without having to change the structure of the code
    [SerializeField]
    private UserData _user;
#endif

    public UserData User { get; private set; }

    #region IMANAGER IMPLEMENTATION
    public NetworkService NetworkService { get; private set; }
    public ManagerStatus Status { get; private set; }

    public void Startup(NetworkService networkService)
    {
        Status = ManagerStatus.Initializing;

        PlayerController.MyManager = this;
        NetworkService = networkService;
        StartCoroutine(NetworkService.DownloadUserData(GameController.Instance.DeviceID, GetUserData));
        Messenger<Color, int>.AddListener("OnChangePlayerColour", OnChangePlayerColour);
    }
    #endregion

    #region STATIC METHODS
    public static Color PlayerColour()
    {
        if(!PhotonNetwork.InRoom)
        {
            return Color.black;
        }

        return PlayerColour(PhotonNetwork.LocalPlayer);
    }

    public static Color PlayerColour(Player player)
    {
        return GetColour(player.CustomProperties);
    }

    public static int PlayerID()
    {
        if (!PhotonNetwork.InRoom)
        {
            return -1;
        }

        return (int)PhotonNetwork.LocalPlayer.CustomProperties["NumberID"];
    }

    public static int PlayerID(Player player)
    {
        return (int)player.CustomProperties["NumberID"];
    }

    public static string PlayerNick(Player player)
    {
        return PhotonNetwork.NickName;
    }

    public static int PlayerColourIndex()
    {
        if(!PhotonNetwork.InRoom)
        {
            return -1;
        }

        return (int)PhotonNetwork.LocalPlayer.CustomProperties["ColourIndex"];
    }

    public static int PlayerColourIndex(Player player)
    {
        return (int)player.CustomProperties["ColourIndex"];
    }

    private static Color GetColour(PhotonHashtable hashtable)
    {
        return new Color
            (
                (float)hashtable["Colour.r"],
                (float)hashtable["Colour.g"],
                (float)hashtable["Colour.b"],
                (float)hashtable["Colour.a"]
            );
    }
    #endregion

    private void GetUserData(NetworkResponseMessage response)
    {
        Debug.Log("[PlayerManager] GetUserData - response.status: " + response.Status.ToString());
        Debug.Log("[PlayerManager] GetUserData - response.message: " + response.Message);

        if(response.Status == NetworkRequestStatus.Success)
        {
            Status = ManagerStatus.Started;
            User = JsonUtility.FromJson<UserData>(response.Message);

#if UNITY_EDITOR
            _user = User;
#endif
            // Means that a player was not found with the current device ID
            if(User.Player_ID == -1)
            {
                Messenger.Broadcast("OnDeviceIdNotRegistered");
                // TODO: Popup with Register or Quit Options
            }
            else
            {
                Messenger<string>.Broadcast("OnUserDataUpdate", User.Username);
                GameController.Instance.NetworkManager.Started(OnNetworkManagerStarted);
            }
        }
        else
        {
            Messenger<string>.Broadcast("OnUserDataDownloadError", response.Message);
        }
    }

    

    private void SetPlayerNumberID(int numberID)
    {
        bool alreadySet = PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("NumberID");

        PhotonNetwork.LocalPlayer.SetCustomProperties(new PhotonHashtable
        {
            { "NumberID" , numberID }
        });

        if(alreadySet)
        {
            Messenger<int>.Broadcast("OnChangePlayerNumberID", numberID);
        }
    }

    #region PUN2 API
    public override void OnJoinedRoom()
    {
        SetPlayerNumberID(PhotonNetwork.CurrentRoom.PlayerCount-1);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        // Only change the local player's `NumberID` if they were after the player who left
        int localPlayerIndex = (int)PhotonNetwork.LocalPlayer.CustomProperties["NumberID"];
        if ((int)otherPlayer.CustomProperties["NumberID"] < localPlayerIndex)
        {
            localPlayerIndex--;
            SetPlayerNumberID(localPlayerIndex);
        }
    }

    public override void OnPlayerPropertiesUpdate(Player target, PhotonHashtable changedProps)
    {
        if(target == PhotonNetwork.LocalPlayer)
        {
            return;
        }

        if(changedProps.ContainsKey("Colour.r") && changedProps.ContainsKey("ColourIndex"))
        {
            Color selectedColour = GetColour(changedProps);
            Messenger<Player, Color>.Broadcast("OnRemoteChangePlayerColour", target, selectedColour);
        }

        if(changedProps.ContainsKey("NumberID"))
        {
            Messenger<int, int>.Broadcast("OnRemoteChangePlayerID", PlayerID(target), target.ActorNumber);
        }
    }
    #endregion

    #region CUSTOM EVENTS
    private void OnChangePlayerColour(Color playerColour, int colourIndex)
    {
        if (!PhotonNetwork.InRoom)
        {
            return;
        }

        PhotonNetwork.LocalPlayer.SetCustomProperties(new PhotonHashtable
        {
            { "Colour.r", playerColour.r },
            { "Colour.g", playerColour.g },
            { "Colour.b", playerColour.b },
            { "Colour.a", playerColour.a },
            { "ColourIndex", colourIndex }
        });
    }

    private void OnNetworkManagerStarted()
    {
        // TODO: Guest Nickname if the device ID is already signed in (i.e)
        //       STORE Online players in DB
        //       IF Device ID already online, then add
        PhotonNetwork.NickName = User.Username;
    }
    #endregion
}
