using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GUIManager : MonoBehaviour {

    public PanelScript P0;
    public PanelScript P1;
    public PanelScript P2;
    public PanelScript P3;

    public RectTransform BG0;
    public RectTransform BG1;
    public RectTransform BG2;

    private PanelScript[] _myPanels;
    private InGameVariables[] _myPlayers;

    private TankHelpers Help = new TankHelpers();

    public int PlayerCount = 0;     // default until configured
    public int OwnPlayerNumber = 0; // default until configured

    // Called by Local Player's PlayerController ... once tank has spawned.
    public void Configure(int NumberPlayers, int OwnPlayer)
    {
        PlayerCount = NumberPlayers;
        OwnPlayerNumber = OwnPlayer;

        // populate Panels's Array (and deactivate redundant Panels)
        _myPanels = new PanelScript[PlayerCount];
        _myPanels[0] = P0;
        _myPanels[1] = P1;
        if (PlayerCount == 4)
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
        _myPlayers = new InGameVariables[PlayerCount];
        _myPlayers[0] = PlayerController.PlayerControllers[0];
        _myPlayers[1] = PlayerController.PlayerControllers[1];
        if (PlayerCount == 4)
        {
            _myPlayers[2] = PlayerController.PlayerControllers[2];
            _myPlayers[3] = PlayerController.PlayerControllers[3];
        }

        // Color Screen BackGround
        Color BGCol = Help.V3ToColor(_myPlayers[OwnPlayer].Color);
        BG0.GetComponent<Image>().color = BGCol;
        BG1.GetComponent<Image>().color = BGCol;
        BG2.GetComponent<Image>().color = BGCol;

        // configure Panels
        for (int i = 0; i < PlayerCount; i++)
        {
            _myPanels[i].SetScore(0);
            _myPanels[i].SetName(_myPlayers[i].PlayerName);
            _myPanels[i].SetColor(Help.V3ToColor(_myPlayers[i].Color));
            _myPanels[i].SetHealth(1f);
        }
    }

    public void UpdateScore(int player)
    {
        _myPanels[player].SetScore(_myPlayers[player].Score);
    }

    public void UpdateHealth(int player)
    {
        _myPanels[player].SetHealth(_myPlayers[player].Curr_Health / _myPlayers[player].Max_Health);
    }


}
