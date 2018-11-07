﻿using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using Photon.Pun;

public class PlayerController : MonoBehaviourPun
{
    public static PlayerController LocalPlayer;
    public static PlayerManager MyManager;

    public GameObject TankType1;
    public GameObject TankType2;
    public int TankChoice = 1; // default
    private Vector3 _Vcolor = new Vector3(255,0,0); // default
    private Color _color = new Color(255,0,0); // default
    public int PlayerID;
    public string PlayerNick;
    public int Score = 0;
    public float Health = 0;
    private Transform[] _Spawns;
    private PlayerController[] _Players;
    private Vector3 _SpawnPos;
    private Quaternion _SpawnRot;
    private TankBase _myTankScript;
    private GameObject _myTankBody;
    public bool IsActive = false;
    // public Transform myHull;
    // public Transform myTurret;
    public Vector3 myHullPos;
    public Quaternion myHullRot;
    public Vector3 myTurrPos;
    public Quaternion myTurrRot;


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

    public void DelayStartGame()
    {
        PauseStart(2f);
    }

    IEnumerator PauseStart(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartGame();
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
        // If more than one MapController in a scene ... we've done something wrong .....
        MapController[] map = FindObjectsOfType(typeof(MapController)) as MapController[]; // there has to be a better way ....
        _Spawns = map[0].ReportSpawns();

        // Find Spawn
        // Developer Note ... initial spawn will be based on "Player" ID ... 
        // so MUST ensure enough Spawn Points on map to cover max number of players
        // something to be considered for level design (maybe randomise after map creation?)
        _SpawnPos = _Spawns[PlayerID].position;
        _SpawnRot = Quaternion.LookRotation((new Vector3(25,_SpawnPos.y,25) - _SpawnPos), Vector3.up); // assumes a 50x50 map .. looks at centre

        // let's make a tank
        // Move Controller to Spawn
        transform.position = _SpawnPos;
        transform.rotation = _SpawnRot;
        switch (TankChoice)
        {
            case 1: _myTankBody = Instantiate(TankType1, transform.position, _SpawnRot);
                break;
            case 2:
                _myTankBody = Instantiate(TankType2, transform.position, _SpawnRot);
                break;
        }
        _myTankBody.transform.parent = transform;
        _myTankScript = _myTankBody.GetComponent<TankBase>();

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
