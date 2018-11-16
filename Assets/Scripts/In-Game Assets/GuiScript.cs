using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GuiScript : MonoBehaviour {

    public RectTransform P1;
    public Text P1Score;
    public Text P1Name;

    public RectTransform P2;

    private InGameVariables P1Stats;
    private InGameVariables P2Stats;


    public void Configure()
    {
        P1Stats = PlayerController.PlayerControllers[0];
        P1Score.text = P1Stats.Score.ToString();
        P1Name.text = P1Stats.PlayerName.ToString();



        P2Stats = PlayerController.PlayerControllers[1];

  
    }

	// Use this for initialization
	void Start () {

    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
