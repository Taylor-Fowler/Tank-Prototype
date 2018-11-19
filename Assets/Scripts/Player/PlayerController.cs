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

public class PlayerController : MonoBehaviourPunCallbacks
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
    public static InGameVariables[] PlayerControllers;
    [SerializeField] private Vector3 _SpawnPos;
    [SerializeField] private Quaternion _SpawnRot;
    private TankBase _myTankScript;
    private GameObject _myTankBody;
    private GUIManager _myGUI;
    public bool IsActive = false;
#if UNITY_EDITOR
    public InGameVariables[] EditorOnlyControllers;
#endif

    #region UNITY API
    private void Awake()
    {
        if (photonView.IsMine)
        {
            LocalPlayer = this;
            // Static list of ingame variables, only the local client creates the empty list
            // Later on, the Unity Start method has each controller add themselves to the list.
            PlayerControllers = new InGameVariables[PlayerManager.PlayersInRoomCount()];
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

    private void Start()
    {
        // Add ourselves to the array
        PlayerControllers[OwnStats.PlayerID] = OwnStats;
    }
    #endregion

    public void StartGame()
    {
#if UNITY_EDITOR
        EditorOnlyControllers = PlayerControllers;
#endif
        // NOTE: 
        //       Maybe in this method, each player controller adds
        //       themselves to the "LocalPlayer" array of player controllers? (We can try that ..... will do after Spawn to ensure all variables present)
        //PlayerController[] TempPlayers = FindObjectsOfType(typeof(PlayerController)) as PlayerController[];
        //_Players = new InGameVariables[TempPlayers.Length];
        //foreach (PlayerController pc in TempPlayers)
        //{
        //    _Players[pc.OwnStats.PlayerID] = pc.OwnStats;
        //}

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
            InitialSpawn();
        }
    }

    // This is guaranteed to only be ran locally, this is called from TankBase.Start(), which only calls
    // this method if it is the local tank base.
    public void RecieveBaseHealth(float C_Health)
    {
        photonView.RPC("RpcUpdateInitialHealth", RpcTarget.AllBuffered, C_Health);
    }

    public void RecievePowerUpHealth (float Health, int PlayerID)
    {
        if (photonView.IsMine && OwnStats.PlayerID == PlayerID)
        {
            OwnStats.Curr_Health = Mathf.Clamp(OwnStats.Curr_Health + Health, 0f, OwnStats.Max_Health);
            photonView.RPC("RpcUpdateIGVCurrHealth", RpcTarget.AllBuffered, OwnStats.PlayerID, OwnStats.Curr_Health);
        }
    }

    public void ChangeTank(int type)
    {
        if (photonView.IsMine)
        {
            Debug.Log("[PC] Change tank Choice called: " + type);
            photonView.RPC("RpcChangeTank", RpcTarget.AllBuffered, type);
        }
    }

    /// <summary>
    /// Should only be called from the local client, instantiates the chosen tank body across the network
    /// and then executes an RPC to inform the instance of this script (on other clients) that the tank body
    /// has been created with the given PhotonView.ViewID
    /// </summary>
    private void InitialSpawn()
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
        _myGUI = FindObjectOfType<GUIManager>();
        _myGUI.Configure(PlayerControllers.Length,OwnStats.PlayerID);
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
        _myTankScript.ChangeColor(OwnStats.Color);
        _myTankScript.SetPlayerID(OwnStats.PlayerID);
        RpcUpdateIGVName(OwnStats.PlayerID, OwnStats.PlayerName);
    }

    public void Fire()
    {
        photonView.RPC("RpcFire", RpcTarget.AllBuffered, OwnStats.PlayerID);
    }

    private void Die()
    {
        IsActive = false;
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
                //    IsActive = false; // now out of the game
                    _myGUI.Splash_Died(PlayerControllers[ShellOwnerID].PlayerName);
                    photonView.RPC("RpcUpdateScore", RpcTarget.AllBuffered, ShellOwnerID);
                    //    _myTankScript.TankDie(); /// Someone died here .... Best respawn .....
                }
            }
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
    private void RpcUpdateIGVCurrHealth(int PlayerID, float CurrHealth)
    {
        PlayerControllers[PlayerID].Curr_Health = CurrHealth;
        // GUI Update
        if (_myGUI == null) _myGUI = FindObjectOfType<GUIManager>(); // Required since (sometimes) the reference has been lost .. for reasons unknown !
        _myGUI.UpdateHealth();

        if (CurrHealth <= 0)
        {
            IsActive = false;
            // We could add reduce health, update score and take damage to one network call....
            // Want to?
        }
        if (photonView.IsMine)
        {
            if (CurrHealth <= 0)
            {
                IsActive = false; // now out of the game
                _myTankScript.TankDie(); /// Someone died here .... Best respawn .....
            }
        }
    }

    [PunRPC]
    private void RpcUpdateScore (int ShellOwnerID)
    {
        PlayerControllers[ShellOwnerID].Score++;
        // GUI Update
        if (_myGUI == null) _myGUI = FindObjectOfType<GUIManager>(); // Required since (sometimes) the reference has been lost .. for reasons unknown !
        _myGUI.UpdateScore();
    }

    [PunRPC]
    private void RpcUpdateIGVMaxHealth (int PlayerID, float MaxHealth)
    {
        PlayerControllers[PlayerID].Max_Health = MaxHealth;
    }

    [PunRPC]
    private void RpcUpdateInitialHealth(float health)
    {
        OwnStats.Curr_Health = OwnStats.Max_Health = health;
    }



    [PunRPC]
    private void RpcUpdateIGVName (int PlayerID, string Name)
    {
        PlayerControllers[PlayerID].PlayerName = Name;
    }
    #endregion




}