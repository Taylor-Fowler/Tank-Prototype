using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using Photon.Pun;
using UnityEngine;

public class ThirtiesTankControls : MonoBehaviourPun {

    public static PlayerController LocalPlayer; // what is this magic ??

    [SerializeField]
    private GameObject Turret;

    public bool TESTING = true; // toggle "true" means can test without Photon
    [Header("Core Stats")] // EITHER set in each pre-fab of Tank Type ... or Base stats for all, then modded on Start() (based on Type)
    public float SpeedMax = 2f;
    public float ReverseSpeedMax = -5f;
    public float Accel = 0.1f;
    public float TurnRate = 30f;
    public float TurrTurnRate = 20f;
    public float BaseHealth = 20;
    public float BaseArmour = 3;
    public float BaseDamage = 5;

    [Header("Stat Modifiers")] // either to mod "Core Stats" at start ... and/or to apply Power Up goodies
    public float ModSpeed = 1f;
    public float ModAccel = 1f;
    public float ModTurn = 1f;
    public float ModTurrTurn = 1f;
    public float ModHealth = 1f;
    public float ModArmour = 1f;
    public float ModDamage = 1f;

    [Header("Current Stats")] // what we actually play with
    public float CurrentSpeed = 0f;

    [Header("Player Options")]
    public Color OwnTeamColor;       // defaults to Blue @ Start() if not set.
    public Color OpponentTeamColor;  // defaults to Red @ Start() if not set.

    private void Awake()
    {
        if (photonView.IsMine)
        {
            //LocalPlayer = this;

        }
    }


    // Use this for initialization
    void Start()
    {
        if (OwnTeamColor == null) OwnTeamColor = Color.blue;
        if (OpponentTeamColor == null) OpponentTeamColor = Color.red;


        if (!photonView.IsMine && !TESTING) return; // Not local Player ... don't bother

        ChangeColor(OwnTeamColor);


    }

    [PunRPC] void ChangeColor (Color color)
    {
        Renderer[] RendList = GetComponentsInChildren<Renderer>();
        foreach ( Renderer Rend in RendList)
        {
            if (Rend.tag == "Colourable")
            {
                Rend.material.color = color;
            }
        }

        if (photonView.IsMine && !TESTING)
        {
            // Photon Targets not available .. need to 
            // photonView.RPC("ChangeColor", PhotonTargets.OthersBuffered, color);
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine && !TESTING) return; // Not local Player ... don't bother
        float turning = 0;
        if (Input.GetKeyDown("w"))
        {
            CurrentSpeed += Accel;
            CurrentSpeed = Mathf.Clamp(CurrentSpeed, ReverseSpeedMax, SpeedMax);
        }
        if (Input.GetKeyDown("s"))
        {
            CurrentSpeed -= Accel;
        }
        CurrentSpeed = Mathf.Clamp(CurrentSpeed, ReverseSpeedMax, SpeedMax);
        transform.Translate(CurrentSpeed * Time.deltaTime * Vector3.forward);
        if (Input.GetKey("a"))
        {
            turning -= TurnRate;
        }
        else if (Input.GetKey("d"))
        {
            turning += TurnRate;
        }
        transform.Rotate(turning * Time.deltaTime * Vector3.up);



    }
}

