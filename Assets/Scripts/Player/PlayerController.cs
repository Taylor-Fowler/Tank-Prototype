using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using Photon.Pun;

public struct InGameVariables
{
    public int PlayerID;
    public string PlayerName;
    public Vector3 Color;
    public PlayerController Controller;
    public float Max_Health;
    public float Curr_Health;
    public int Score;
}

public class PlayerController : MonoBehaviourPun
{
    public static PlayerController LocalPlayer;
    public static PlayerManager MyManager;

    private TankHelpers Help = new TankHelpers(); // A function Utility Class

    public GameObject TankType1;
    public GameObject TankType2;
    public int TankChoice = 1; // default
    public Vector3 _Vcolor = new Vector3(255, 0, 0); // default // public required for TankBase
    private Color _color = new Color(255, 0, 0); // default
    [SerializeField] private Transform[] _Spawns;
    public InGameVariables OwnStats;
    [SerializeField] public InGameVariables[] _Players;
    [SerializeField] private Vector3 _SpawnPos;
    [SerializeField] private Quaternion _SpawnRot;
    private TankBase _myTankScript;
    private GameObject _myTankBody;
    public bool IsActive = false;
    [Header("OwnStats Reporting")] // can delete once debugged and working
    [SerializeField] private int PlayerID;
    [SerializeField] private string Name;
    [SerializeField] private float Max_Health;
    [SerializeField]  private float Curr_Health;
    [SerializeField]  private int Score;


    // Will re-visit synching this way if Transform view continued to be laggy
    //public Vector3 myHullPos;
    //public Quaternion myHullRot;
    //public Vector3 myTurrPos;
    //public Quaternion myTurrRot;

    #region UNITY API
    private void Awake()
    {
        if (photonView.IsMine)
        {
            LocalPlayer = this;
            //Instantiate(CameraPrefab, transform);
            // NOTE:
            //       Moved // MyManager = FindObjectOfType(typeof(PlayerManager)) as PlayerManager;
            //       It now resides in PlayerManager.Startup
        }

        GameController.Instance.Event_OnGameSceneInitialised += StartGame;
        // Populate "OwnStats"
        OwnStats.PlayerID = PlayerManager.PlayerID(photonView.Owner);
        OwnStats.PlayerName = PlayerManager.PlayerNick(photonView.Owner);
        OwnStats.Color = Help.ColorToV3(PlayerManager.PlayerColour(photonView.Owner));
        OwnStats.Controller = this;
        OwnStats.Score = 0;
            // configured when Tank body added
        OwnStats.Max_Health = 0; 
        OwnStats.Curr_Health = 0;

        // For Debugging ...
        PlayerID = OwnStats.PlayerID;
        Name = OwnStats.PlayerName;

    }

    private void Update()
    {
        // Only For Active Player
        if (photonView.IsMine == false && PhotonNetwork.IsConnected == true) // IsConnected added to allow off-line testing
        {
            return;
        }
        UpdateInspectorOwnStats(); // required as can't readily "see" an Struct in the Inspector
    }

    private void UpdateInspectorOwnStats()
    {
        Max_Health = OwnStats.Max_Health;
        Curr_Health = OwnStats.Curr_Health;
        Score = OwnStats.Score;
    }

    public void StartGame()
    {
        // NOTE: 
        //       Maybe in this method, each player controller adds
        //       themselves to the "LocalPlayer" array of player controllers?
        PlayerController[] TempPlayers = FindObjectsOfType(typeof(PlayerController)) as PlayerController[];
        _Players = new InGameVariables[TempPlayers.Length];
        foreach (PlayerController pc in TempPlayers)
        {
            _Players[pc.OwnStats.PlayerID] = pc.OwnStats;
        }

        // Get Spawn Points
        // If more than one MapController in a scene ... we've done something wrong .....
        MapController[] map = FindObjectsOfType(typeof(MapController)) as MapController[]; // there has to be a better way ....
        _Spawns = map[0].ReportSpawns();

        // Find Spawn
        // Developer Note ... initial spawn will be based on "Player" ID ... 
        // so MUST ensure enough Spawn Points on map to cover max number of players
        // something to be considered for level design (maybe randomise after map creation?)
        _SpawnPos = _Spawns[OwnStats.PlayerID].position;
        _SpawnRot = Quaternion.LookRotation((new Vector3(25, _SpawnPos.y, 25) - _SpawnPos), Vector3.up); // assumes a 50x50 map .. looks at centre
        transform.position = _SpawnPos;
        transform.rotation = _SpawnRot;

        if (photonView.IsMine)
        {
            Spawn();
        }
    }
    #endregion


    public int ReportID()
    {
        return OwnStats.PlayerID;
    }

    public void  RecieveBaseHealth( float C_Health)
    {
        OwnStats.Curr_Health = OwnStats.Max_Health = C_Health;
        if (photonView.IsMine)
        {
            foreach (InGameVariables GV in _Players)
            {
                // maybe do something
            }
        }
    }

    public void ChangeTank(int type)
    {
        if (photonView.IsMine)
        {
            photonView.RPC("RpcChangeTank", RpcTarget.AllBuffered, type);
        }
    }

    /// <summary>
    /// Should only be called from the local client, instantiates the chosen tank body across the network
    /// and then executes an RPC to inform the instance of this script (on other clients) that the tank body
    /// has been created with the given PhotonView.ViewID
    /// </summary>
    private void Spawn()
    {
        // let's make a tank
        // Move Controller to Spawn
        switch (TankChoice)
        {
            case 1:
                _myTankBody = PhotonNetwork.Instantiate(TankType1.name, transform.position, _SpawnRot);
                break;
            case 2:
                _myTankBody = PhotonNetwork.Instantiate(TankType2.name, transform.position, _SpawnRot);
                break;
        }

        photonView.RPC("RpcSetTankBody", RpcTarget.AllBuffered, _myTankBody.GetComponent<PhotonView>().ViewID);
    }




    [PunRPC]
    private void RpcChangeTank(int type)
    {
        TankChoice = type;
    }

    [PunRPC]
    private void RpcSetTankBody(int viewID)
    {
        if (!photonView.IsMine)
        {
            _myTankBody = PhotonView.Find(viewID).gameObject;
        }
        _myTankBody.transform.parent = transform;
        _myTankScript = _myTankBody.GetComponent<TankBase>();
        _myTankScript.MyV3Color = OwnStats.Color;
        _myTankScript.ChangeColor();
    }

    public void Fire()
    {
        //photonView.RPC("RpcFire", RpcTarget.AllBuffered, _myTankBody.GetComponent<PhotonView>().ViewID);
        photonView.RPC("RpcFire", RpcTarget.AllBuffered, _myTankBody.GetComponent<PhotonView>().ViewID); // do we need the view ID ?
    }

    [PunRPC]
    private void RpcFire(int viewID)
    {
        if (!photonView.IsMine)
        {
            _myTankBody = PhotonView.Find(viewID).gameObject;
        }
        _myTankScript = _myTankBody.GetComponent<TankBase>();
        _myTankScript.Fire();
    }

    public void TakeDamage(int ShellOwnerID, float damage, int TankHitPlayerID)
    {
        photonView.RPC("RpcTakeDamage", RpcTarget.AllBuffered,ShellOwnerID,damage,TankHitPlayerID);
    }

    [PunRPC]
    private void RpcTakeDamage (int ShellOwnerID, float damage, int TankHitPlayerID)
    {
        if (!photonView.IsMine)
        {
            // do nothing
        }
        _Players[TankHitPlayerID].Curr_Health -= damage;
        if (OwnStats.Curr_Health <= 0)
        {
            // add death script ... for Dev purposes will throw a cube into the game
            Debug.Log("[PlayerController] PlayerID " + OwnStats.PlayerID + " ("+ OwnStats.PlayerName+ ") DIED !!");
            photonView.RPC("RpcUpdateScore", RpcTarget.AllBuffered, ShellOwnerID);
        }
    }

    [PunRPC]
    private void RpcUpdateScore (int OwnerID)
    {
       if (!photonView.IsMine)
       {
            // do nothing
       }
        _Players[OwnerID].Score++;
        Debug.Log("[PlayerController] PlayerID " + OwnStats.PlayerID + " (" + OwnStats.PlayerName + ") SCORES");
    }
}