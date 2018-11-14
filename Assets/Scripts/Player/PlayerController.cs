using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using Photon.Pun;

[System.Serializable]
public class InGameVariables
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
    }

    //private void Update()
    //{
    //    // Only For Active Player
    //    //if (photonView.IsMine == false && PhotonNetwork.IsConnected == true) // IsConnected added to allow off-line testing
    //    //{
    //    //    return;
    //    //}
    //}



    public void StartGame()
    {
        // NOTE: 
        //       Maybe in this method, each player controller adds
        //       themselves to the "LocalPlayer" array of player controllers? (We can try that ..... will do after Spawn to ensure all variables present)
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

    public void RecieveBaseHealth(float C_Health)
    {
        OwnStats.Curr_Health = OwnStats.Max_Health = C_Health;
        if (photonView.IsMine)
        {
            RpcSynchAllIGV(OwnStats.PlayerID);
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
        IsActive = true;
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
        RpcUpdateIGVName(OwnStats.PlayerID, OwnStats.PlayerName);
    }

    public void Fire()
    {
        photonView.RPC("RpcFire", RpcTarget.AllBuffered, OwnStats.PlayerID);
    }

    [PunRPC]
    private void RpcFire(int playerID)
    {
        _myTankScript.Fire(playerID);
    }

    public void TakeDamage(int ShellOwnerID, float damage)
    {
        if (photonView.IsMine) // My View AND My Tank was hit ....
        {
            if (IsActive) // still in the game?
            {       
                OwnStats.Curr_Health -= damage;
                photonView.RPC("RpcUpdateIGVCurrHealth", RpcTarget.AllBuffered, OwnStats.PlayerID, OwnStats.Curr_Health);
                if (OwnStats.Curr_Health <= 0)
                {
                    IsActive = false; // now out of the game
                    photonView.RPC("RpcUpdateScore", RpcTarget.AllBuffered, ShellOwnerID);
                    _myTankScript.TankDie(); /// Someone died here .... Best respawn .....
                }
            }
        }
    }

    [PunRPC]
    private void RpcTakeDamage (int ShellOwnerID, float damage, int TankHitPlayerID)
    {
        RpcUpdateIGVCurrHealth(TankHitPlayerID, OwnStats.Curr_Health);

        _Players[TankHitPlayerID].Curr_Health -= damage;
        if (OwnStats.Curr_Health <= 0)
        {
            // add death script ... for Dev purposes will throw a cube into the game
            Debug.Log("[PlayerController] PlayerID " + OwnStats.PlayerID + " ("+ OwnStats.PlayerName+ ") DIED !!");
            photonView.RPC("RpcUpdateScore", RpcTarget.AllBuffered, ShellOwnerID);
        }
    }

    #region Pun Update _Player InGameVariables
    /// <summary>
    /// All called by Local Client when their OwnStats are modified, to synch up their and all other client's _Players[] record
    /// </summary>
    [PunRPC]
    private void RpcSynchAllIGV (int OwnerID)
    {
        RpcUpdateIGVMaxHealth(OwnerID,OwnStats.Max_Health);
        RpcUpdateIGVCurrHealth(OwnerID, OwnStats.Curr_Health);
        RpcUpdateIGVName(OwnerID, OwnStats.PlayerName);
    }
   
    [PunRPC]
    private void RpcUpdateScore (int OwnerID)
    {
        _Players[OwnerID].Score++;
    }

    [PunRPC]
    private void RpcUpdateIGVMaxHealth (int PlayerID, float MaxHealth)
    {
        _Players[PlayerID].Max_Health = MaxHealth;
    }

    [PunRPC]
    private void RpcUpdateIGVCurrHealth(int PlayerID, float CurrHealth)
    {
        _Players[PlayerID].Curr_Health = CurrHealth;
    }

    [PunRPC]
    private void RpcUpdateIGVName (int PlayerID, string Name)
    {
        _Players[PlayerID].PlayerName = Name;
    }
    #endregion




}