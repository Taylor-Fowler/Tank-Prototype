using UnityEngine;
using UnityEngine.UI;

namespace Network
{
    public class CreateRoom : MonoBehaviour
    {
        private struct RoomSettings
        {
            public string Name;
            public string Password;
            public bool Private;
            public int MaxPlayers;

            public void Create(NetworkManager networkManager)
            {
                if(Private)
                {
                    networkManager.CreatePrivateRoom(Name, MaxPlayers, Password);
                }
                else
                {
                    networkManager.CreatePublicRoom(Name, MaxPlayers);
                }
            }
        }

        public InputField RoomNameInput;
        public InputField RoomPasswordInput;

        private RoomSettings _roomSettings;

        #region UNITY API
        private void Start()
        {
            _roomSettings = new RoomSettings
            {
                MaxPlayers = 2
            };
        }
        #endregion


        /// <summary>
        /// Enables toggling the privacy of a room via a Toggle GUI element.
        /// </summary>
        /// <param name="toggle">The toggle GUI element.</param>
        public void TogglePrivateRoom(Toggle toggle)
        {
            _roomSettings.Private = toggle.isOn;
            RoomPasswordInput.interactable = _roomSettings.Private;
        }

        public void SetMaxPlayers(Dropdown dropdown)
        {
            int.TryParse(dropdown.captionText.text, out _roomSettings.MaxPlayers);
        }

        public void Create()
        {
            _roomSettings.Name = RoomNameInput.text;
            _roomSettings.Password = RoomPasswordInput.text;
            _roomSettings.Create(GameController.Instance.NetworkManager);
        }
    }
}
