using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class WebRequestUserData : MonoBehaviour
{
    public UserData UserData = null;

    private void Start()
    {
        StartCoroutine(TryGetData());    
    }

    private IEnumerator TryGetData()
    {
        WWWForm formData = new WWWForm();
        formData.AddField("id", "1");

        UnityWebRequest www = UnityWebRequest.Post("http://kunet.kingston.ac.uk/k1612040/test.php", formData);
        yield return www.SendWebRequest();

        if(www.isNetworkError)
            Debug.Log(www.error);
        else
        {
            Debug.Log(www.downloadHandler.text);
            UserData = JsonUtility.FromJson<UserData>(www.downloadHandler.text);
        }
    }
}
