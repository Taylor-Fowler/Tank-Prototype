///////////////////////////////////////////////////////////////////////////
// CI6510 Optimised Game Programming    (Kingston University)            //
// Course Work 1 : Network Game with Plug-ins                            //
// Submission by Max Bryans (K1628007) and Taylor Fowler (K1612040)      //
// December 2018                                                         //
///////////////////////////////////////////////////////////////////////////
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class ColourButtons : MonoBehaviourPunCallbacks
{
    [System.Serializable]
    private class BITCombo
    {
        public Button Button;
        public Image Image;
        public Text Text;
        public int ActorID { get; private set; }

        public BITCombo()
        {
            ActorID = -1;
        }

        public void Select(int playerID, int actorID)
        {
            Debug.Log("Setting Actor/Player ID Color: " + actorID + "/" + playerID + "\nLocal Actor ID: " + PhotonNetwork.LocalPlayer.ActorNumber);
            ActorID = actorID;
            Text.text = (playerID + 1).ToString();
            Image.enabled = true;
            Button.interactable = false;
        }

        public void Unselect()
        {
            if(ActorID != -1)
            {
                Debug.Log("Unsetting Actor ID Color: " + ActorID + "\nLocal Actor ID: " + PhotonNetwork.LocalPlayer.ActorNumber);
            }
            ActorID = -1;
            Text.text = "";
            Image.enabled = false;
            Button.interactable = true;
        }
    }

    public GameObject ButtonsAnchor;

    [SerializeField] private BITCombo[] _buttons;
    private BITCombo _selectedButton = null;

    #region UNITY API
    private void Start()
    {
        for(int i = 0; i <_buttons.Length; i++)
        {
            int iValue = i;
            _buttons[i].Button.onClick.AddListener(delegate
            {
                SelectButton(iValue);
            });
        }

        Messenger<Player, Color>.AddListener("OnRemoteChangePlayerColour", OnRemoteChangePlayerColour);
        Messenger<int>.AddListener("OnChangePlayerNumberID", OnChangePlayerNumberID);
        Messenger<int, int>.AddListener("OnRemoteChangePlayerID", OnRemoteChangePlayerID);

        if(GameController.Instance.PostGameLobby && PhotonNetwork.InRoom)
        {
            ReturnToRoomAfterGameEnd();
        }
    }

    private void OnDestroy()
    {
        Messenger<Player, Color>.RemoveListener("OnRemoteChangePlayerColour", OnRemoteChangePlayerColour);
        Messenger<int>.RemoveListener("OnChangePlayerNumberID", OnChangePlayerNumberID);
        Messenger<int, int>.RemoveListener("OnRemoteChangePlayerID", OnRemoteChangePlayerID);
    }
    #endregion

    #region PUN2 API
    public override void OnJoinedRoom()
    {
        StartCoroutine(SelectNextAvailableColour());
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        foreach(var button in _buttons)
        {
            if(button.ActorID == otherPlayer.ActorNumber)
            {
                button.Unselect();
                return;
            }
        }
    }

    public override void OnLeftRoom()
    {
        foreach(var button in _buttons)
        {
            button.Unselect();
        }

        _selectedButton = null;
    }
    #endregion

    #region CUSTOM EVENT RESPONSES
    /// <summary>
    /// 
    /// </summary>
    /// <param name="player"></param>
    /// <param name="color"></param>
    private void OnRemoteChangePlayerColour(Player player, Color color)
    {
        int playerID = PlayerManager.PlayerID(player);

        foreach(var button in _buttons)
        {
            if(button.ActorID == player.ActorNumber)
            {
                button.Unselect();
                break;
            }
        }

        _buttons[PlayerManager.PlayerColourIndex(player)].Select(playerID, player.ActorNumber);
    }

    /// <summary>
    /// Called when the local player's room ID changes, if the player has a selected
    /// colour button (technically they should), then the button is updated to match
    /// the players new ID.
    /// </summary>
    /// <param name="id">The ID assigned to the local player</param>
    private void OnChangePlayerNumberID(int id)
    {
        if(_selectedButton != null)
        {
            _selectedButton.Select(id, PhotonNetwork.LocalPlayer.ActorNumber);
        }
    }

    /// <summary>
    /// Called when a remote players room ID changes, loops through all buttons
    /// looking for a colour button registered to the players (the one with the new ID)
    /// actor ID. If a button is registered to the player, the player ID is updated to
    /// match their new ID, otherwise nothing is done.
    /// </summary>
    /// <param name="id">The new ID of the player</param>
    /// <param name="actorID">The actor ID of the player, used to identify their button</param>
    private void OnRemoteChangePlayerID(int id, int actorID)
    {
        foreach(var button in _buttons)
        {
            if(button.ActorID == actorID)
            {
                button.Select(id, actorID);
                return;
            }
        }
    }
    #endregion

    private void ReturnToRoomAfterGameEnd()
    {
        foreach(var player in PhotonNetwork.CurrentRoom.Players)
        {
            int playerColourIndex = PlayerManager.PlayerColourIndex(player.Value);
            if(playerColourIndex == -1)
            {
                continue;
            }

            int playerID = PlayerManager.PlayerID(player.Value);
            if(playerID == -1)
            {
                continue;
            }

            BITCombo button = _buttons[playerColourIndex];
            button.Select(
                        playerID,
                        player.Value.ActorNumber
                        );

            if(player.Value == PhotonNetwork.LocalPlayer)
            {
                _selectedButton = button;
            }
        }
    }

    private void SelectButton(int index)
    {
        BITCombo button = _buttons[index];

        // Somebody else has selected it, cannot select it
        if(button.ActorID != -1)
        {
            return;
        }
        // Unselect the previously selected button
        if(_selectedButton != null)
        {
            _selectedButton.Unselect();
        }

        _selectedButton = button;
        _selectedButton.Select(PlayerManager.PlayerID(), PhotonNetwork.LocalPlayer.ActorNumber);

        Messenger<Color, int>.Broadcast("OnChangePlayerColour", button.Button.targetGraphic.color, index);
    }


    private IEnumerator SelectNextAvailableColour()
    {
        while(_buttons.Length != 9 || PlayerManager.PlayerID() == -1)
        {
            yield return null;
        }

        ReturnToRoomAfterGameEnd();
        int nextAvailableColour = 0;

        if (!PhotonNetwork.IsMasterClient)
        {
            foreach (var player in PhotonNetwork.CurrentRoom.Players)
            {
                if (player.Value == PhotonNetwork.LocalPlayer)
                {
                    continue;
                }

                int playerSelection = PlayerManager.PlayerColourIndex(player.Value);
                int playerID = PlayerManager.PlayerID(player.Value);

                if (playerSelection == nextAvailableColour)
                {
                    nextAvailableColour++;
                }
            }
        }

        SelectButton(nextAvailableColour);
    }
}
