///////////////////////////////////////////////////////////////////////////
// CI6510 Optimised Game Programming    (Kingston University)            //
// Course Work 1 : Network Game with Plug-ins                            //
// Submission by Max Bryans (K1628007) and Taylor Fowler (K1612040)      //
// December 2018                                                         //
///////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

using Photon.Pun;

using Network;

public class GameController : MonoBehaviourPun
{
    // TBA variables placeholders
    [Header("Necessary Tranforms, Cameras and Objects")]
    public Camera PlayerCamera;
    public GameObject PlayerPrefab;

    [Header("Master Game Variables")]
    public int PlayerCount = 0;
    public int KillsRequired = 2;
    public string DeviceID { get; private set; }
    public bool GameRunning { get; private set; }
    public NetworkManager NetworkManager { get; private set; }
    public PlayerManager PlayerManager { get; private set; }

    #region CUSTOM EVENT DEFINITIONS
    public delegate void GameSceneInitialised();
    public GameSceneInitialised Event_OnGameSceneInitialised;

    public delegate void GameStart();
    public GameStart Event_OnGameStart;

    public delegate void GameOver(InGameVariables winningPlayer);
    public GameOver Event_OnGameOver;
    #endregion

    private List<IManager> _managers;
    private NetworkService _networkService;

    // --------------------//
    // establish Singleton //
    // ------------------- //
    public static GameController Instance
    {
        get
        {
            return instance;
        }
    }
    private static GameController instance = null;

    #region UNITY API
    private void Awake()
    {
        Debug.Log("[GameController] Awake");
        if (instance)
        {
            Debug.Log("Already a GameController running - going to die now .....");
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(this);
        GameRunning = false;

        NetworkManager = gameObject.AddComponent<NetworkManager>();
        PlayerManager = gameObject.AddComponent<PlayerManager>();

        _managers = new List<IManager>
        {
            NetworkManager,
            PlayerManager
        };

        PlayerController.Event_OnAllPlayersInitialised += OnAllPlayersInitialised;
    }

    //---------------------------//
    // Finished Singleton set up //
    // --------------------------//

    // Doesn't do much at the mo ... probably will configure via a Plug-in eventually
    private void Start ()
    {
        Debug.Log("[GameController] Start");

        // Get Unique Device ID N.B. was attached to a "if null" condition .... fell over on the second run (I'm guessing it was internally saves as something)
        DeviceID = SystemInfo.deviceUniqueIdentifier;
        Debug.Log("DevID: " + DeviceID + " Length : " + DeviceID.Length + " Chars");

        SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        InjectServices();
    }

    private void OnDestroy()
    {
        if(instance != this)
        {
            return;
        }

        foreach(var manager in _managers)
        {
            manager.Shutdown();
        }
        PlayerController.Event_OnAllPlayersInitialised -= OnAllPlayersInitialised;
        SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
    }

    #endregion
    public void Reset()
    {
        PhotonNetwork.LoadLevel(1);
        foreach(var manager in _managers)
        {
            manager.Restart();
        }
    }

    private void InjectServices()
    {
        _networkService = new NetworkService();
        
        foreach (var manager in _managers)
        {
            manager.Startup(_networkService);
        }
    }

    private void SceneManager_sceneLoaded(Scene loadedScene, LoadSceneMode loadSceneMode)
    {
        if (loadedScene.buildIndex > 1)
        {
            GameRunning = true;
            GameObject player = PhotonNetwork.Instantiate(PlayerPrefab.name, Vector3.zero, Quaternion.identity);
        }
    }

    #region CUSTOM EVENT RESPONSES
    private void OnAllPlayersInitialised(double startTime)
    {
        StartCoroutine(CountdownGameStart(startTime));
    }
    #endregion

    #region CUSTOM EVENT TRIGGERS
    public void OnGameOver(InGameVariables winningPlayer)
    {
        GameRunning = false;

        if(Event_OnGameOver != null)
        {
            Event_OnGameOver(winningPlayer);
        }
    }

    private void OnGameSceneInitialised()
    {
        if(Event_OnGameSceneInitialised != null)
        {
            Event_OnGameSceneInitialised();
        }
    }

    private void OnGameStart()
    {
        if(Event_OnGameStart != null)
        {
            Event_OnGameStart();
        }
    }
    #endregion

    private IEnumerator CountdownGameStart(double startTime)
    {
        do
        {
            yield return null;
        } while (startTime > PhotonNetwork.Time);

        OnGameStart();
    }

    public static IEnumerator Delay(float delay, Action callback)
    {
        yield return new WaitForSeconds(delay);
        callback();
    }
}
