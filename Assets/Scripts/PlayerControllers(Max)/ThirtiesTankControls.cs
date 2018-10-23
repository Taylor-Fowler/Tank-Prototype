using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using Photon.Pun;
using UnityEngine;

public class ThirtiesTankControls : MonoBehaviourPun {

    public static PlayerController LocalPlayer; // what is this magic ??

    public GameObject Turret;

    public Vector3 Spawn = new Vector3(20f, 0.21f, 20f); // Dev purposes spawn only

    public bool TESTING = true; // toggle "true" means can test without Photon
    [Header("Core Stats")] // EITHER set in each pre-fab of Tank Type ... or Base stats for all, then modded on Start() (based on Type)
    public float BaseSpeedMax = 2f;
    public float BaseReverseSpeedMax = -5f;
    public float BaseAccel = 0.1f;
    public float BaseTurnRate = 30f;
    public float BaseTurrTurnRate = 20f;
    public float BaseHealth = 20;
    public float BaseArmour = 3;
    public float BaseDamage = 5;
    public float BaseFireRate = 1;

    [Header("Stat Modifiers")] // either to mod "Core Stats" at start ... and/or to apply Power Up goodies
    public float ModSpeed = 1f;
    public float ModAccel = 1f;
    public float ModTurn = 1f;
    public float ModTurrTurn = 1f;
    public float ModHealth = 1f;
    public float ModArmour = 1f;
    public float ModDamage = 1f;
    public float ModFireRate = 1f;

    float C_SpeedMax        { get { return BaseSpeedMax * ModSpeed; } } // N.B. no setter
    float C_ReverseSpeedMax { get { return BaseReverseSpeedMax * ModSpeed; } }
    float C_Accel           { get { return BaseAccel * ModAccel; } }
    float C_TurnRate        { get { return BaseTurnRate * ModTurn; } }
    float C_TurrTurnRate    { get { return BaseTurrTurnRate * ModTurrTurn; } }
    float C_Health          { get { return BaseHealth * ModHealth; } } // Placeholder only until mechanics established
    float C_Armour          { get { return BaseArmour * ModArmour; } } // Placeholder only until mechanics established
    float C_Damage          { get { return BaseDamage * ModDamage; } } // Placeholder only until mechanics established
    float C_FireRate        { get { return BaseFireRate * ModFireRate; } }
    public float CurrentSpeed {get; private set;}


    [SerializeField]
    private float Cooldown = 0;

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
        if (OwnTeamColor == null) OwnTeamColor = Color.blue; // becomes black?
        if (OpponentTeamColor == null) OpponentTeamColor = Color.red;

        CurrentSpeed = 0f;

        if (!photonView.IsMine && !TESTING) return; // Not local Player ... don't bother

        ChangeColor(OwnTeamColor);
        transform.position = Spawn;


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
            // Photon Targets not available .. need to figure that shit out
            // photonView.RPC("ChangeColor", PhotonTargets.OthersBuffered, color);
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine && !TESTING) return; // Not local Player ... don't bother

        // Tank Hull Forward / Backward input
        if (Input.GetKey("w")) CurrentSpeed += C_Accel;
        if (Input.GetKey("s")) CurrentSpeed -= C_Accel;
        if (!Input.GetKey("w") && !Input.GetKey("s")) // no input deceleration ... stops @ 0
        {
            if (CurrentSpeed > 0) { CurrentSpeed = Mathf.Max(0, CurrentSpeed - (C_Accel / 4)); }
            else { CurrentSpeed = Mathf.Min(0, CurrentSpeed + (C_Accel / 4)); }
        }
        CurrentSpeed = Mathf.Clamp(CurrentSpeed, C_ReverseSpeedMax, C_SpeedMax); // Clamp Speed
        transform.Translate(CurrentSpeed * Time.deltaTime * Vector3.forward);

        // Tank Hull Rotation
        if (Input.GetKey("a")) transform.Rotate(-C_TurnRate *Time.deltaTime * Vector3.up);
        else if (Input.GetKey("d")) transform.Rotate(C_TurnRate * Time.deltaTime * Vector3.up);

        // Turret (and therefore camera Rotation)
        float TurrToGo = Input.mousePosition.x / Screen.width;
        // tried various ... this seems to work best with a "dead zone" in the middle
        if (TurrToGo < 0.48) { Turret.transform.Rotate(new Vector3(0, -C_TurrTurnRate * Time.deltaTime, 0)); }
        else if (TurrToGo > 0.52) { Turret.transform.Rotate(new Vector3(0, C_TurrTurnRate * Time.deltaTime, 0)); }

    }
}

