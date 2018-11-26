///////////////////////////////////////////////////////////////////////////
// CI6510 Optimised Game Programming    (Kingston University)            //
// Course Work 1 : Network Game with Plug-ins                            //
// Submission by Max Bryans (K1628007) and Taylor Fowler (K1612040)      //
// December 2018                                                         //
///////////////////////////////////////////////////////////////////////////
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
    #region STATIC MEMBERS & METHODS
    public static PlayerController LocalPlayer;
    public static InGameVariables[] PlayerControllers;

    public delegate void AllPlayersInitialised(double startTime);
    public static AllPlayersInitialised Event_OnAllPlayersInitialised;

    public delegate void LocalPlayerRespawn();
    public static LocalPlayerRespawn Event_OnLocalPlayerRespawn;

    private static void OnAllPlayersInitialised(double startTime)
    {
        if (Event_OnAllPlayersInitialised != null)
        {
            Event_OnAllPlayersInitialised(startTime);
        }
    }

    private static void OnLocalPlayerRespawn()
    {
        if(Event_OnLocalPlayerRespawn != null)
        {
            Event_OnLocalPlayerRespawn();
        }
    }
    #endregion

    public int TankChoice = 1; // default
    public bool IsActive = false;
    public GameObject TankType1;
    public GameObject TankType2;
    public Vector3 _Vcolor = new Vector3(255, 0, 0); // default // public required for TankBase
    public InGameVariables OwnStats;
#if UNITY_EDITOR
    public InGameVariables[] EditorOnlyControllers;
#endif

    [SerializeField] private Transform[] _Spawns;
    [SerializeField] private Transform[] _InitialSpawns;
    [SerializeField] private Vector3 _SpawnPos;
    [SerializeField] private Quaternion _SpawnRot;
    private Color _color = new Color(255, 0, 0); // default
    private GameObject _myTankBody;
    private TankBase _myTankScript;
    private GUIManager _myGUI;
    private TankHelpers Help = new TankHelpers(); // A function Utility Class

    #region UNITY API
    private void Awake()
    {
        GameController.Instance.Event_OnGameStart += OnGameStart;

        if (photonView.IsMine)
        {
            LocalPlayer = this;
            // Static list of ingame variables, only the local client creates the empty list
            // Later on, the Unity Start method has each controller add themselves to the list.
            PlayerControllers = new InGameVariables[PlayerManager.PlayersInRoomCount()];
            // Sound set-up
            this.gameObject.AddComponent<AudioListener>();

            Event_OnLocalPlayerRespawn += Respawn;
        }
        // Populate "OwnStats"
        OwnStats.PlayerID = PlayerManager.PlayerID(photonView.Owner);
        OwnStats.PlayerName = PlayerManager.PlayerNick(photonView.Owner);
        Debug.Log(OwnStats.PlayerName);
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

        // Get Spawn Points
        // If more than one MapController in a scene ... we've done something wrong .....
        MapController[] map = FindObjectsOfType(typeof(MapController)) as MapController[]; // there has to be a better way ....
        _Spawns = map[0].ReportSpawns();
        _InitialSpawns = map[0].ReportInitialSpawns();

        // Find Spawn
        // Developer Note ... initial spawn is based on "Player" ID ... 
        // So each Map has as many Initial Spawn Sites as Players
        _SpawnPos = _InitialSpawns[OwnStats.PlayerID].position;
        _SpawnRot = Quaternion.LookRotation((new Vector3(25, _SpawnPos.y, 25) - _SpawnPos), Vector3.up); // assumes a 50x50 map .. looks at centre
        transform.position = _SpawnPos;
        transform.rotation = _SpawnRot;

        if(PhotonNetwork.IsMasterClient)
        {
            LocalPlayer.CheckAllAreLoaded();
        }
    }

    private void OnDestroy()
    {
        if(photonView.IsMine)
        {
            Event_OnLocalPlayerRespawn -= Respawn;
        }
        GameController.Instance.Event_OnGameStart -= OnGameStart;
    }
    #endregion

    #region CUSTOM EVENT RESPONSES
    private void OnGameStart()
    {
#if UNITY_EDITOR
        EditorOnlyControllers = PlayerControllers;
#endif      
        _myGUI = FindObjectOfType<GUIManager>();

        if (photonView.IsMine)
        {
            Spawn();
            _myGUI.Configure(PlayerControllers.Length, OwnStats.PlayerID);
        }
    }
    #endregion

    public void Fire()
    {
        photonView.RPC("RpcFire", RpcTarget.AllBuffered, OwnStats.PlayerID);
    }

    public void TakeDamage(int ShellOwnerID, float damage)
    {
        if (photonView.IsMine && IsActive && GameController.Instance.GameRunning) // My View AND My Tank was hit .... still in the game?
        {
            OwnStats.Curr_Health -= damage;
            photonView.RPC("RpcUpdateIGVCurrHealth", RpcTarget.AllBuffered, OwnStats.PlayerID, OwnStats.Curr_Health);
            if (OwnStats.Curr_Health <= 0)
            {
                IsActive = false; // now out of the game
                photonView.RPC("RpcUpdateScore", RpcTarget.AllBuffered, ShellOwnerID);
            }
        }
    }

    public void RecievePowerUpHealth (float Health, int PlayerID)
    {
        if (photonView.IsMine && OwnStats.PlayerID == PlayerID)
        {
            OwnStats.Curr_Health = Mathf.Clamp(OwnStats.Curr_Health + Health, 0f, OwnStats.Max_Health);
            photonView.RPC("RpcUpdateIGVCurrHealth", RpcTarget.AllBuffered, OwnStats.PlayerID, OwnStats.Curr_Health);
        }
    }

    public Transform Position()
    {
        if(_myTankBody == null)
        {
            return null;
        }

        return _myTankBody.transform;
    }

    /// <summary>
    /// Should only be called from the local client, instantiates the chosen tank body across the network
    /// and then executes an RPC to inform the instance of this script (on other clients) that the tank body
    /// has been created with the given PhotonView.ViewID
    /// </summary>
    private void Spawn()
    {
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

    private void Respawn()
    {
        MapController[] map = FindObjectsOfType(typeof(MapController)) as MapController[];
        _SpawnPos = map[0].GetSpawn(OwnStats).position;
        _SpawnRot = Quaternion.LookRotation((new Vector3(25, _SpawnPos.y, 25) - _SpawnPos), Vector3.up); // assumes a 50x50 map .. looks at centre
        transform.position = _SpawnPos;
        transform.rotation = _SpawnRot;

        Spawn();
    }

    private void Die(string playerWhoKilled)
    {
        // All clients should update the dead player controllers properties
        IsActive = false;
        _myTankScript.TankDie();

        // Only the dead player should destroy their tank and call the respawn stuff
        if(photonView.IsMine)
        {
            double time = PhotonNetwork.Time;
            _myGUI.Splash_Died(playerWhoKilled);
            StartCoroutine(_myGUI.UpdateTimer(time + 3.0));
            StartCoroutine(WaitToRespawn(time + 3.0));
        }

        _myTankScript = null;
        _myTankBody = null;
    }

    private IEnumerator WaitToRespawn(double waitUntil)
    {
        while(PhotonNetwork.Time < waitUntil)
        {
            yield return null;
        }

        OnLocalPlayerRespawn();
    }

    private void CheckAllAreLoaded()
    {
        for (int i = 0; i < PlayerControllers.Length; i++)
        {
            if (PlayerControllers[i] == null)
            {
                return;
            }
        }
        photonView.RPC("RpcSetStartTime", RpcTarget.AllBuffered, PhotonNetwork.Time + 11.0);
    }

    [PunRPC]
    private void RpcSetStartTime(double time)
    {
        OnAllPlayersInitialised(time);
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

        OwnStats.Curr_Health = OwnStats.Max_Health = _myTankScript.GetHealth();
        _myGUI.UpdateHealth();

        if (photonView.IsMine)
        {
            _myTankScript.SM.PlaySFX(SFX.Start);
        }
    }

    [PunRPC]
    private void RpcFire(int playerID)
    {
        _myTankScript.Fire(playerID);
    }
    #region Pun Update _Player InGameVariables


    [PunRPC]
    private void RpcUpdateIGVCurrHealth(int PlayerID, float CurrHealth)
    {
        PlayerControllers[PlayerID].Curr_Health = CurrHealth;
        _myGUI.UpdateHealth();
    }

    // Note: Update score means a player died, the player who died sent the message to their controller on the other
    //       clients, therefore we can use update score to trigger the die function locally
    [PunRPC]
    private void RpcUpdateScore (int ShellOwnerID)
    {
        InGameVariables shooter = PlayerControllers[ShellOwnerID];
        shooter.Score++;
        _myGUI.UpdateScore();

        if(shooter.Score == GameController.Instance.KillsRequired)
        {
            GameController.Instance.OnGameOver(shooter);
        }
        else
        {
            Die(PlayerControllers[ShellOwnerID].PlayerName);
        }
    }
    #endregion
}