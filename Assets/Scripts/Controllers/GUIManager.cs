﻿///////////////////////////////////////////////////////////////////////////
// CI6510 Optimised Game Programming    (Kingston University)            //
// Course Work 1 : Network Game with Plug-ins                            //
// Submission by Max Bryans (K1628007) and Taylor Fowler (K1612040)      //
// December 2018                                                         //
///////////////////////////////////////////////////////////////////////////
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

using Photon.Pun;

public class GUIManager : MonoBehaviour
{

    // SUMMARY
    // GuiManager has a set of 4 GUIPanels (from prefabs, in Scene and Inspector defined)
    // when Configure() called by Local Player ...
    //      _myPanels[] populated with the appropriate number of panels for players (excess ones are DeActivated)
    //      _myPlayers[] populated with references to the Local Players Copy of the InGameVariables
    // thereafter ... Local PlayerController calls UpdateXYZ(PlayerID) methods which in turn ....
    // ...  makes the panel in _myPanels[PlayerID] update to reflect the stats in _myPlayers[PlayerID]
    //
    #region PUBLIC / INSPECTOR MEMBERS
    [Header("Player Panels (in scene)")]
    public PanelScript P0;
    public PanelScript P1;
    public PanelScript P2;
    public PanelScript P3;

    [Header("GUI SplashScreen (in scene)")]
    public SplashScript Splash;

    [Header("GUI Background Border (in scene)")]
    public RectTransform BG0;
    public RectTransform BG1;
    public RectTransform BG2;
    public RectTransform BG3;

    [Header("Time for damage flash")]
    public float FlashTime = 0.1f;

    public Text TimeToStartText;
    #endregion

    #region PRIVATE MEMBERS
    private PanelScript[] _myPanels;
    private TankHelpers _Help = new TankHelpers();
    private InGameVariables[] _myPlayers;
    private int _PlayerCount = 0;     // default until configured by local Player Manager
    private int _OwnPlayerNumber = 0; // as above
    #endregion

    #region Public Methods (Tank Choice)
    public void PickTankOne ()
    {
        PlayerController.LocalPlayer.TankChoice = 1;
        Debug.Log("[GUI Manager] Change tank Choice called: 1" );
    }

    public void PickTankTwo()
    {
        PlayerController.LocalPlayer.TankChoice = 2;
        Debug.Log("[GUI Manager] Change tank Choice called: 2");
    }

    public IEnumerator UpdateTimer(double startTime)
    {
        return UpdateTimer(startTime, DisableTimeToStart);
    }
    #endregion

    #region GUI Player Panels - Public Methods (called by PlayerController, as stats change)

    // Called by Local Player's PlayerController ... once tank has spawned.
    public void Configure(int NumberPlayers, int OwnPlayer)
    {
        _PlayerCount = NumberPlayers;
        _OwnPlayerNumber = OwnPlayer;

        // populate Panels's Array (and deactivate redundant Panels)
        _myPanels = new PanelScript[_PlayerCount];
        _myPanels[0] = P0;
        _myPanels[1] = P1;
        if (_PlayerCount == 4)
        {
            _myPanels[2] = P2;
            _myPanels[3] = P3;
        }
        else
        {
            P2.DeActivate();
            P3.DeActivate();
        }

        // Populate IGV's References
        _myPlayers = new InGameVariables[_PlayerCount];
        _myPlayers[0] = PlayerController.PlayerControllers[0];
        _myPlayers[1] = PlayerController.PlayerControllers[1];
        if (_PlayerCount == 4)
        {
            _myPlayers[2] = PlayerController.PlayerControllers[2];
            _myPlayers[3] = PlayerController.PlayerControllers[3];
        }

        // Color Screen Border BackGround
        Color BGCol = _Help.V3ToColor(_myPlayers[OwnPlayer].Color);
        BG0.GetComponent<Image>().color = BGCol;
        BG1.GetComponent<Image>().color = BGCol;
        BG2.GetComponent<Image>().color = BGCol;
        BG3.GetComponent<Image>().color = BGCol;

        // Initial Panel Set-up
        for (int i = 0; i < _PlayerCount; i++)
        {
            _myPanels[i].SetScore(0);
            _myPanels[i].SetName(_myPlayers[i].PlayerName);
            _myPanels[i].SetColor(_Help.V3ToColor(_myPlayers[i].Color));
            _myPanels[i].SetHealth(1f);
            _myPanels[i].SetFlashTime(FlashTime);
            _myPanels[i].Connect(true);
        }
    }

    // All method's below call all Panels' update methods
    // N.B. Trying to do a selective Updates based upon PlayerID proved "erratic" with only local client being updated    

    public void UpdateScore()
    {
        for (int i = 0; i < _PlayerCount; i++)
        {
            _myPanels[i].SetScore(_myPlayers[i].Score);
        }
    }

    public void UpdateHealth()
    {
        for (int i = 0; i < _PlayerCount; i++)
        {
            _myPanels[i].SetHealth(_myPlayers[i].Curr_Health / _myPlayers[i].Max_Health);
        }
    }

    public void UpdateConnectionStatus (int PlayerID, bool status)
    {
        _myPanels[PlayerID].Connect(status);
    }

    #endregion

    #region SplashScreen - Public Methods (called by PlayerController - relayed to Splashscreen)

    public void Splash_GameOverWin()
    {
        Splash.GameOverWin();
    }

    public void Splash_GameOverLost()
    {
        Splash.GameOverLost();
    }

    public void Splash_Died(string name)
    {
        Splash.Died(name);
    }

    public void Splash_ReSpawn()
    {
        Splash.ReSpawn();
    }

    public void Splash_OtherDisconnect()
    {
        Splash.OtherDisconnect();
    }
    #endregion

    #region UNITY API
    private void Start()
    {
        GameController.Instance.Event_OnGameOver += OnGameOver;
        PlayerController.Event_OnAllPlayersInitialised += OnAllPlayersInitialised;
        PlayerController.Event_OnLocalPlayerRespawn += OnLocalPlayerRespawn;
    }

    private void OnDestroy()
    {
        GameController.Instance.Event_OnGameOver -= OnGameOver;
        PlayerController.Event_OnAllPlayersInitialised -= OnAllPlayersInitialised;
        PlayerController.Event_OnLocalPlayerRespawn -= OnLocalPlayerRespawn;
        StopAllCoroutines(); // best not forget this
    }
    #endregion

    #region PRIVATE METHODS
    private void OnAllPlayersInitialised(double startTime)
    {
        StartCoroutine(UpdateTimer(startTime, DisableTimeToStart));
    }

    private void OnLocalPlayerRespawn()
    {
        Splash_ReSpawn();
    }

    private void OnGameOver(InGameVariables winningPlayer)
    {
        if (winningPlayer == PlayerController.LocalPlayer.OwnStats)
        {
            Splash_GameOverWin();
        }
        else
        {
            Splash_GameOverLost();
        }
    }

    private void DisableTimeToStart()
    {
        TimeToStartText.text = "";
        TimeToStartText.gameObject.SetActive(false);
    }

    private IEnumerator UpdateTimer(double startTime, Action callback)
    {
        TimeToStartText.gameObject.SetActive(true);
        do
        {
            TimeToStartText.text = ((int)(startTime - PhotonNetwork.Time)).ToString();
            yield return null;
        } while (startTime > PhotonNetwork.Time);

        callback();
    }
    #endregion
}
