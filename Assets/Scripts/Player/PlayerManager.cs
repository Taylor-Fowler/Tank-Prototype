using System.Collections;
using System.Collections.Generic;
using Network;
using UnityEngine;

public class PlayerManager : MonoBehaviour, IManager
{
    public NetworkService NetworkService { get; private set; }
    public ManagerStatus Status { get; private set; }
    public UserData User { get; private set; }

#if UNITY_EDITOR
    // Only included to view the serialised user data inside of the
    // editor without having to change the structure of the code
    [SerializeField]
    private UserData _user;
#endif


    [SerializeField]
    private float _requestPlayerDataDelay = 3.0f;

    public void Startup(NetworkService networkService)
    {
        Status = ManagerStatus.Initializing;

        NetworkService = networkService;
        StartCoroutine(NetworkService.DownloadUserData(GameController.Instance.DeviceID, GetUserData));
    }

    private void GetUserData(NetworkResponseMessage response)
    {
        Debug.Log("Response Status: " + response.Status.ToString());
        Debug.Log("Response Message: " + response.Message);

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
            }
            else
            {
                Messenger<string>.Broadcast("OnUserDataUpdate", User.Username);
            }
        }
        else
        {
            Messenger<string>.Broadcast("OnUserDataDownloadError", response.Message);
        }
    }
}
