using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using Photon.Pun;
using UnityEngine;

public class ThirtiesTankControls : MonoBehaviourPun, IDamageable {

    public static PlayerController LocalPlayer; // what is this magic ??

    public GameObject Turret;
    public Transform _firePos;
    public Transform Shell;

    public Vector3 Spawn = new Vector3(20f, 0.21f, 20f); // Dev purposes spawn only

    public bool TESTING = true; // toggle "true" means can test without Photon
    [Header("Core Stats")] // EITHER set in each pre-fab of Tank Type ... or Base stats for all, then modded on Start() (based on Type)
    public float BaseSpeedMax = 2f;
    public float BaseReverseSpeedMax = -5f;
    public float BaseAccel = 20000f;
    public float BaseTurnRate = 30f;
    public float BaseTurrTurnRate = 20f;
    public float BaseHealth = 20;
    public float BaseArmour = 3;
    public float BaseDamage = 5;
    public float BaseFireRate = 0.2f;
    public int BaseShell = 3; // default Shell type, will be changed by Power up's
    public float BaseMass = 1000;

    [Header("Stat Modifiers")] // either to mod "Core Stats" at start ... and/or to apply Power Up goodies
    public float ModSpeed = 1f;
    public float ModAccel = 1f;
    public float ModTurn = 1f;
    public float ModTurrTurn = 1f;
    public float ModHealth = 1f;
    public float ModArmour = 1f;
    public float ModDamage = 1f;
    public float ModFireRate = 1f;
    public float ModMass = 1f;

    float C_SpeedMax        { get { return BaseSpeedMax * ModSpeed; } } // N.B. no setter
    float C_ReverseSpeedMax { get { return BaseReverseSpeedMax * ModSpeed; } }
    float C_Accel           { get { return BaseAccel * ModAccel; } }
    float C_TurnRate        { get { return BaseTurnRate * ModTurn; } }
    float C_TurrTurnRate    { get { return BaseTurrTurnRate * ModTurrTurn; } }
    float C_Health          { get { return BaseHealth * ModHealth; } } // Placeholder only until mechanics established
    float C_Armour          { get { return BaseArmour * ModArmour; } } // Placeholder only until mechanics established
    float C_Damage          { get { return BaseDamage * ModDamage; } } // Placeholder only until mechanics established
    float C_FireRate        { get { return BaseFireRate * ModFireRate; } }
    float C_Mass            { get { return BaseMass * ModMass; } }
    public float CurrentSpeed {get; private set;}
    private Rigidbody RB;


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
        RB = GetComponent<Rigidbody>();
        RB.mass = C_Mass;



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

    // Usinf FIXEDUPDATE for better Physics
    void FixedUpdate()
    {
        if (!photonView.IsMine && !TESTING) return; // Not local Player ... don't bother
        ControlMovement();
        ControlWeaponToggles();
        ControlFiring();
    }

    void ControlFiring()
    {
        // Reduce cooldown
        Cooldown = Mathf.Max(0, Cooldown - Time.deltaTime);
        //if (Cooldown <=0) // Auto fire for testing
        if (Input.GetMouseButtonDown(0) && Cooldown <= 0) // FIRE
        {
            Fire(C_Damage, _firePos.transform.position, _firePos.transform.rotation, BaseShell, OwnTeamColor);
            Cooldown = C_FireRate;
        }
    }

    void Fire (float dmg, Vector3 start, Quaternion direction, int type, Color color)
    {
        Transform shell = (Transform)Instantiate(Shell,start,direction);
        ShellScript ss = shell.GetComponent<ShellScript>();
        ss.dmg = dmg;
        ss.type = type;
        ss.color = color;
    }

    public void TakeDamage(float damage)
    {
        // TBA
    }

    void ControlWeaponToggles()
    {
        if (Input.GetKey("1")) { BaseShell = 1; Debug.Log("Standard Shells"); }
        if (Input.GetKey("2")) { BaseShell = 2; Debug.Log("Bouncy Shells"); }
        if (Input.GetKey("3")) { BaseShell = 3; Debug.Log("Triple-shot Shells"); }
    }

    void ControlMovement()
    {
        // Tank Hull Forward / Backward input
        if (Input.GetKey("w")) RB.AddForce(transform.forward * C_Accel);
        if (Input.GetKey("s")) RB.AddForce(-transform.forward * C_Accel);
        // no input deceleration ... stops @ 0
        if (!Input.GetKey("w") && !Input.GetKey("s") && RB.velocity.magnitude != 0) 
        {
            // going forwards
            if (Vector3.Dot(RB.velocity,transform.forward) > 0)
                { RB.AddForce(-transform.forward * C_Accel / 4); }
            // backwards
            else
                { RB.AddForce(transform.forward * C_Accel / 4); }

            // bring to graceful halt if close to 0
            if (RB.velocity.magnitude < 0.1 && RB.velocity.magnitude > -0.1)
                { RB.velocity = Vector3.zero; }
        }

        // clamp velocity
        if (RB.velocity.magnitude > C_SpeedMax) 
            { RB.velocity = Vector3.ClampMagnitude(RB.velocity, C_SpeedMax); }

        // Eliminate any annoying sideways movement
        RB.velocity = Vector3.Project(RB.velocity, transform.forward);


        // Tank Hull Rotation
        //if (Input.GetKey("a")) transform.Rotate(-C_TurnRate *Time.deltaTime * Vector3.up);
        //else if (Input.GetKey("d")) transform.Rotate(C_TurnRate * Time.deltaTime * Vector3.up);
        if (Input.GetKey("a")) RB.AddTorque(-transform.up * C_TurnRate);
        else if (Input.GetKey("d")) RB.AddTorque(transform.up * C_TurnRate);

        // Turret (and therefore camera Rotation)
        // N.B. Turret does NOT use Physics
        float TurrToGo = Input.mousePosition.x / Screen.width;
        // tried various ... this seems to work best with a "dead zone" in the middle
        if (TurrToGo < 0.48) { Turret.transform.Rotate(new Vector3(0, -C_TurrTurnRate * Time.deltaTime, 0)); }
        else if (TurrToGo > 0.52) { Turret.transform.Rotate(new Vector3(0, C_TurrTurnRate * Time.deltaTime, 0)); }
    }

}

