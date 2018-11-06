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
            ActorID = actorID;
            Text.text = (playerID + 1).ToString();
            Image.enabled = true;
            Button.interactable = false;
        }

        public void Unselect()
        {
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
        Button[] allButtons = ButtonsAnchor.GetComponentsInChildren<Button>();
        _buttons = new BITCombo[allButtons.Length];

        for(int i = 0; i < allButtons.Length; i++)
        {
            _buttons[i] = new BITCombo
            {
                Button = allButtons[i],
                Image = allButtons[i].GetComponentsInChildren<Image>()[0],
                Text = allButtons[i].GetComponentInChildren<Text>()
            };
            _buttons[i].Unselect();
            
            int iValue = i;
            _buttons[i].Button.onClick.AddListener(delegate
            {
                SelectButton(iValue);
            });
        }

        Messenger<Player, Color>.AddListener("OnRemoteChangePlayerColour", OnRemoteChangePlayerColour);
        Messenger<int>.AddListener("OnChangePlayerNumberID", OnChangePlayerNumberID);
        Messenger<int, int>.AddListener("OnRemoteChangePlayerID", OnRemoteChangePlayerID);
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

    #region CUSTOM EVENTS
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

    private void OnChangePlayerNumberID(int id)
    {
        if(_selectedButton != null)
        {
            _selectedButton.Select(id, PhotonNetwork.LocalPlayer.ActorNumber);
        }
    }

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

    private void SelectButton(int index)
    {
        BITCombo button = _buttons[index];

        // Somebody else has selected it
        if(button.ActorID != -1)
        {
            return;
        }

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
        yield return new WaitForSeconds(1f);
        int nextAvailableColour = 0;

        if (!PhotonNetwork.IsMasterClient)
        {
            foreach (var player in PhotonNetwork.PlayerList)
            {
                if (player == PhotonNetwork.LocalPlayer)
                {
                    continue;
                }

                int playerSelection = PlayerManager.PlayerColourIndex(player);
                int playerID = PlayerManager.PlayerID(player);

                if (playerSelection == nextAvailableColour)
                {
                    nextAvailableColour++;
                }
            }
        }

        SelectButton(nextAvailableColour);
    }
}
