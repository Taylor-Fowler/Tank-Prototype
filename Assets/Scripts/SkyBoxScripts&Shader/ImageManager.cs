using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ImageManager : MonoBehaviour, IGameManager {

	public ManagerStatus status { get; private set; }

    private NetService _network;

    private Texture2D _webImage;

    public void StartUp(NetService service)
    {
        Debug.Log("Images Manager starting ....");

        _network = service;

        status = ManagerStatus.Started;
    }

    public void GetWebImages(Action<Texture2D> callback)
    {
        if (_webImage == null)
        {
            StartCoroutine(_network.DownloadImage(callback));
        }
        else
        {
            callback(_webImage);
        }
    }

}
