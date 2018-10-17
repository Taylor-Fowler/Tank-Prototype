using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class NetService {

    private const string xmlApi = "http://api.openweathermap.org/data/2.5/weather?q=London,uk&appid=80a09c862ab49e7c22f74b2716e392c3&mode=xml";
    private const string webImage = "http://upload.wikimedia.org/wikipedia/commons/c/c5/Moraine_Lake_17092005.jpg";

    private bool IsResponseValid(WWW www)
    {
        if(!string.IsNullOrEmpty(www.error))
        {
            Debug.Log("Bad Connection: "+ www.error.ToString());
            return false;
        }
        else if (string.IsNullOrEmpty(www.text))
        {
            Debug.Log("Bad Data");
            return false;
        }
        else
        {
            return true;
        }
    }

    private IEnumerator CallAPI(string url, Action<string> callback)
    {
        WWW www = new WWW(url);
        Debug.Log("CallAPI ... pre-yield");
        double Start = Time.realtimeSinceStartup;
        yield return www;
        Debug.Log("CallAPI ... post yield -- " + (Time.realtimeSinceStartup - Start).ToString() + " seconds");
        Debug.Log("CallAPI ... www = " + www.text);


        if (!IsResponseValid(www))
            yield break;

        callback(www.text);
    }
    public IEnumerator DownloadImage (Action<Texture2D> callback)
    {
        WWW www = new WWW(webImage);
        yield return www;
        callback(www.texture);
    }

    public IEnumerator GetWeatherXML(Action<string> callback)
    {
        return CallAPI(xmlApi, callback);
    }

}
