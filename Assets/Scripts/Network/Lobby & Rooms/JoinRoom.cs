using System;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class JoinRoom : MonoBehaviour
{
    [System.Serializable]
    public class JoinRoomAuthentication
    {
        public GameObject Panel;
        public InputField PasswordInput;
        public Button ConfirmButton;
        public Button CloseButton;

        public void Initialise()
        {
            CloseButton.onClick.AddListener(delegate
            {
                Close();
            });
        }

        public void Close()
        {
            Panel.SetActive(false);
            PasswordInput.text = "";
            ConfirmButton.onClick.RemoveAllListeners();
        }

        public void Open(Action callback, string roomPassword)
        {
            ConfirmButton.onClick.AddListener(delegate
            {
                Confirm(callback, roomPassword);
            });

            Panel.SetActive(true);
        }

        private void Confirm(Action callback, string roomPassword)
        {
            if (PasswordInput.text == roomPassword)
            {
                Close();
                callback();
            }
        }
    };

    [SerializeField]
    private JoinRoomAuthentication _authentication;


    public UnityAction RegisterForAuthentication(string roomName, string roomPass = "")
    {
        if(_authentication == null)
        {
            _authentication = new JoinRoomAuthentication();
            _authentication.Initialise();
        }

        if(roomPass == "")
        {
            return delegate 
            {
                Join(roomName);
            };
        }

        return delegate 
        {
            _authentication.Open(
                delegate
                {
                    Join(roomName);
                }, 
                roomPass);
        };
    }

    private void Join(string roomName)
    {
        GameController.Instance.NetworkManager.JoinRoom(roomName);
    }
}
