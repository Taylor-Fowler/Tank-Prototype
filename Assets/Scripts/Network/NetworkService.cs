using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Network
{
    /// <summary>
    /// Enumerator that defines the possible statuses of a NetworkRequest
    /// NOTE: Timeout was removed as it was not needed at this point in time
    /// </summary>
    public enum NetworkRequestStatus
    {
        Success,
        Error
        //, Timeout
    };

    /// <summary>
    /// A NetworkResponseMessage is the common callback parameter for many NetworkRequests.
    /// The response message contains a response status that indicates whether the request
    /// was successful or not. Depending on the status, the message will either contain the
    /// payload of the request (Success) or the error message (Error).
    /// </summary>
    public struct NetworkResponseMessage
    {
        public NetworkRequestStatus Status;
        public string Message;
    }

    /// <summary>
    /// The NetworkService provides the API to request resources and send game data to the
    /// database API.
    /// </summary>
    public class NetworkService
    {
        private const string WebPath = "http://kunet.kingston.ac.uk/k1612040/Tanks/";



        public IEnumerator DownloadUserData(string deviceIdentifier, Action<NetworkResponseMessage> callback)
        {
            WWWForm form = new WWWForm();
            form.AddField("Device_ID", deviceIdentifier);
            
            UnityWebRequest webRequest = UnityWebRequest.Post(WebPath + "login.php", form);
            yield return webRequest.SendWebRequest();

            callback(IsRequestValid(webRequest));
        }

        public IEnumerator RegisterUserData(string deviceIdentifier, string username, Action<NetworkResponseMessage> callback)
        {
            WWWForm form = new WWWForm();
            form.AddField("Device_ID", deviceIdentifier);
            form.AddField("Username", username);

            UnityWebRequest webRequest = UnityWebRequest.Post(WebPath + "register.php", form);
            yield return webRequest.SendWebRequest();

            callback(IsRequestValid(webRequest));
        }

        public IEnumerator UpdateUserData(UserData userData, Action<NetworkResponseMessage> callback)
        {
            WWWForm form = new WWWForm();
            form.AddField("Device_ID", userData.Device_ID);
            form.AddField("Games_Played", userData.Games_Played);
            form.AddField("Kills", userData.Kills);
            form.AddField("Deaths", userData.Deaths);
            form.AddField("Assists", userData.Assists);
            form.AddField("Wins", userData.Wins);
            form.AddField("Losses", userData.Losses);
            
            UnityWebRequest webRequest = UnityWebRequest.Post(WebPath + "updates.php", form);
            yield return webRequest.SendWebRequest();

            callback(IsRequestValid(webRequest));
        }

        private static NetworkResponseMessage IsRequestValid(UnityWebRequest request)
        {
            NetworkResponseMessage responseMessage = new NetworkResponseMessage();
            
            if (request.isNetworkError || request.isHttpError)
            {
                responseMessage.Status = NetworkRequestStatus.Error;
                responseMessage.Message = request.error;
            }
            else
            {
                responseMessage.Status = NetworkRequestStatus.Success;
                responseMessage.Message = request.downloadHandler.text;
            }
            return responseMessage;
        }
    }
}
