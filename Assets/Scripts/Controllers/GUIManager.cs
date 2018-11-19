using UnityEngine;
using UnityEngine.UI;

public class GUIManager : MonoBehaviour {
    // SUMMARY
    // GuiManager has a set of 4 GUIPanels (from prefabS, in Scene and Inspector defined)
    // when Configure() called by Local Player ...
    //      _myPanels[] populated with the appropriate number of panels for players (excess ones are DeActivated)
    //      _myPlayers[] populated with references to the Local Players Copy of the InGameVariables
    // thereafter ... Local PlayerController calls UpdateXYZ(PlayerID) methods which in turn ....
    // ...  makes the panel in _myPanels[PlayerID] update to reflect the stats in _myPlayers[PlayerID]
    //
    #region Inspector Settables Public Vars
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

    [Header("Time for damage flash")]
    public float FlashTime = 0.1f;
    #endregion

    #region Private Vars
    private PanelScript[] _myPanels;
    private TankHelpers _Help = new TankHelpers();
    private InGameVariables[] _myPlayers;
    private int _PlayerCount = 0;     // default until configured by local Player Manager
    private int _OwnPlayerNumber = 0; // as above

    #endregion

    #region Public Methods (Tank Choice)

    public void PickTankOne ()
    {
        PlayerController myCon = FindObjectOfType<PlayerController>();
        myCon.ChangeTank(1);
        Debug.Log("[GUI Manager] Change tank Choice called: 1" );
    }

    public void PickTankTwo()
    {
        PlayerController myCon = FindObjectOfType<PlayerController>();
        myCon.ChangeTank(2);
        Debug.Log("[[GUI Manager] Change tank Choice called: 2");
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

        // Initial Panel Set-up
        for (int i = 0; i < _PlayerCount; i++)
        {
            _myPanels[i].SetScore(0);
            _myPanels[i].SetName(_myPlayers[i].PlayerName);
            _myPanels[i].SetColor(_Help.V3ToColor(_myPlayers[i].Color));
            _myPanels[i].SetHealth(1f);
            _myPanels[i].SetFlashTime(FlashTime);
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

}
