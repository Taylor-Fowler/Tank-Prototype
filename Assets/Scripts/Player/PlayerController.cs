using UnityEngine;
using UnityEngine.Networking;
using Photon.Pun;

public class PlayerController : MonoBehaviourPun
{
    public static PlayerController LocalPlayer;

    public Transform TankType1;
    public Transform TankType2;
    public int TankChoice;
    private Vector3 _Vcolor;
    private Color _color;
    public int PlayerID;
    public int Score;
    public float Health;
    private MapController _map;
    private Transform[] _Spawns;
    private PlayerController[] _Players;

    [SerializeField]
    //private GameObject CameraPrefab;
    //private GameObject TurretObject;

    private void Awake()
    {
        if(photonView.IsMine)
        {
            LocalPlayer = this;
            //Instantiate(CameraPrefab, transform);
        }
    }

    public void StartGame()
    {
        // Get other player references and sort by ID
        // SERIOUSLY Unity?  Seriously?
        PlayerController[] TempPlayers = FindObjectsOfType(typeof(PlayerController)) as PlayerController[];
        _Players = new PlayerController[TempPlayers.Length];
        foreach (PlayerController pc in TempPlayers)
        {
            _Players[pc.PlayerID] = pc;
        }

        // Get Spawn Points
        _map = 


    }

    private void Update()
    {
        // Only For Active Player
        if (photonView.IsMine == false && PhotonNetwork.IsConnected == true) // IsConnected added to allow off-line testing
        {
            return;
        }

    }
}
