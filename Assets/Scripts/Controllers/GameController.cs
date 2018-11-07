using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Network;

public class GameController : MonoBehaviourPun
{

    // TBA variables placeholders
    [Header("Necessary Tranforms, Cameras and Objects")]
    public Camera PlayerCamera;

    [Header("Master Game Variables")]
    public int PlayerCount = 0;
    public string DeviceID { get; private set; }
    public NetworkManager NetworkManager { get; private set; }
    public PlayerManager PlayerManager { get; private set; }

    [Header("Serialized Fields - for debug reference only")]
    [SerializeField] CommsManager Comms;

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
    
    void Awake()
    {
        Debug.Log("[GameController] Awake");
        if (instance)
        {
            Debug.Log("Already a GameController running - going to die now .....");
            DestroyImmediate(gameObject);
            return;
        }
        instance = this;

        // add Comms
        Comms = gameObject.AddComponent<CommsManager>();
        NetworkManager = gameObject.AddComponent<NetworkManager>();
        PlayerManager = gameObject.AddComponent<PlayerManager>();

        _managers = new List<IManager>();
        _managers.Add(NetworkManager);
        _managers.Add(PlayerManager);
    }
    //---------------------------//
    // Finished Singleton set up //
    // --------------------------//

    // Doesn't do much at the mo ... probably will configure via a Plug-in eventually
    void Start ()
    {
        Debug.Log("[GameController] Start");
        DontDestroyOnLoad(this);
        // Get Unique Device ID N.B. was attached to a "if null" condition .... fell over on the second run (I'm guessing it was internally saves as something)
        DeviceID = SystemInfo.deviceUniqueIdentifier;
        Debug.Log("DevID: " + DeviceID + " Length : " + DeviceID.Length + " Chars");
        InjectServices();
    }

    void InjectServices()
    {
        _networkService = new NetworkService();
        
        foreach (var manager in _managers)
        {
            manager.Startup(_networkService);
        }
    }
}
