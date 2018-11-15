using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public abstract class TankBase : MonoBehaviourPun, IDamageable, ITakesPowerUps
{
    public GameObject CameraPrefab;
    public Transform CameraAnchor;
    public GameObject Turret;
    public Transform _firePos;
    public Transform Shell;
    public GameObject ExpPreFab;

    private TankHelpers Help = new TankHelpers();

    public bool TESTING = true; // toggle "true" means can test without Photon
    [Header("Core Stats")] // Core Stats are Customised by Children
    public float BaseSpeedMax = 1f;
    public float BaseAccel = 1f;
    public float BaseTurnRate = 1f;
    public float BaseTurrTurnRate = 1f;
    public float BaseHealth = 1;
    public float BaseArmour = 1;
    public float BaseDamage = 1;
    public float BaseFireRate = 1f;
    public int BaseShell = 1; // default Shell type, will be changed by Power up's
    public float BaseMass = 1;

    [Header("Stat Modifiers")] // Changed by Power Up goodies
    public float ModSpeed = 1f;
    public float ModAccel = 1f;
    public float ModTurn = 1f;
    public float ModTurrTurn = 1f;
    public float ModHealth = 1f;
    public float ModArmour = 1f;
    public float ModDamage = 1f;
    public float ModFireRate = 1f;
    public float ModMass = 1f;

    float C_SpeedMax { get { return BaseSpeedMax * ModSpeed; } } // N.B. no setter
    float C_Accel { get { return BaseAccel * ModAccel; } }
    float C_TurnRate { get { return BaseTurnRate * ModTurn; } }
    float C_TurrTurnRate { get { return BaseTurrTurnRate * ModTurrTurn; } }
    float C_Health { get; set; } // configured at Start();
    float C_Armour { get { return BaseArmour * ModArmour; } } // Placeholder only until mechanics established
    float C_Damage { get { return BaseDamage * ModDamage; } } // Placeholder only until mechanics established
    float C_FireRate { get { return BaseFireRate * ModFireRate; } }
    float C_Mass { get { return BaseMass * ModMass; } }
    public float CurrentSpeed { get; private set; }
    private Rigidbody RB;
    public Collider Col; // reference for own shells to ignore .. public so Photon can use it?

    private bool _turretlock = false;
    public Vector3 MyV3Color;
    //public Color MyColor;

    [SerializeField]
    private float Cooldown = 0;
    public bool AutoFire = false;

    #region UNITY API
    private void Start ()
    {
        // Only For Local Player
        if (photonView.IsMine == false && PhotonNetwork.IsConnected == true)
        {
            RB = GetComponent<Rigidbody>();
            RB.isKinematic = true;
            return;
        }

        Instantiate(CameraPrefab, CameraAnchor);
        CurrentSpeed = 0f;
        RB = GetComponent<Rigidbody>();
        if (RB == null) Debug.Log(" No RB found");
        RB.mass = C_Mass;
        Col = GetComponent<Collider>();

        // set the "settables" & report them
        C_Health = BaseHealth* ModHealth;
        PlayerController.LocalPlayer.RecieveBaseHealth(C_Health);
    }

    private void Update()
    {
        // Only For Active Player
        if (photonView.IsMine == false && PhotonNetwork.IsConnected == true)
        {
            return;
        }
        ControlMovement();
        ControlWeaponToggles();
        ControlFiring();
        CheckTurretLock();
    }
    #endregion

    public void ChangeColor()
    {
       Color MyColor = Help.V3ToColor(MyV3Color);
       Renderer[] RendList = GetComponentsInChildren<Renderer>();
       foreach (Renderer Rend in RendList)
       {
           if (Rend.tag == "Colourable")
           {
               Rend.material.color = MyColor;
           }
       }
    }



    public float GetHealth()
    {
        return C_Health;
    }

    void CheckTurretLock()
    {
        if (Input.GetKeyDown("space")) _turretlock = !_turretlock;
        if (Input.GetKeyDown("p")) AutoFire = !AutoFire;
    }

    void ControlFiring()
    {
        // Reduce cooldown
        Cooldown = Mathf.Max(0, Cooldown - Time.deltaTime);
        if (Cooldown <=0 && AutoFire || (Input.GetMouseButtonDown(0) && Cooldown <= 0) && !AutoFire) // Auto fire for testing
        {
            PlayerController.LocalPlayer.Fire(); // call via the PlayerController so it can call Fire via RPC.
            Cooldown = C_FireRate;
        }
    }

    public void Fire(int playerID)
    {
        Transform shell = (Transform)Instantiate(Shell, _firePos.transform.position, _firePos.transform.rotation);
        ShellScript ss = shell.GetComponent<ShellScript>();
        ss.OwnerID = playerID;
        ss.dmg = C_Damage;
        ss.type = ShellType.Standard;
        Color MyColor = Help.V3ToColor(MyV3Color);
        ss.color = MyColor;
        // Get Shell to ignore Firing Units Collider
        // https://docs.unity3d.com/ScriptReference/Physics.IgnoreCollision.html (03 Nov 2018)
        Physics.IgnoreCollision(ss.GetComponent<Collider>(), Col);
    }

    public void TankDie()
    {
        GameObject boom = Instantiate(ExpPreFab, transform.position, Quaternion.identity) as GameObject;
        Destroy(boom, 1);
    }

    #region Interface IDamageable Implementation
    public void TakeDamage(int OwnerID, float damage)
    {
        // Only the Local player will process the damage and tell everybody else about it
        if(!photonView.IsMine)
        {
            return;
        }
        float pen = damage - C_Armour;
        if (pen > 0)
        {
            PlayerController.LocalPlayer.TakeDamage(OwnerID, damage);
        }
    }
    #endregion


    #region  Interface ITakesPowerUps Implementation
    public void FireRatePlus(float factor, float time)
    {
        ModFireRate *= factor;
        StartCoroutine(RevertFireRate(factor, time));
    }

    IEnumerator RevertFireRate (float factor, float time)
    {
        yield return new WaitForSeconds(time);
        ModFireRate /= factor;
    }

    public void MovementPlus(float factor, float time)
    {
        ModAccel *= factor;
        ModTurn *= factor;
        ModSpeed *= factor;
        StartCoroutine(RevertMovement(factor, time));
    }

    IEnumerator RevertMovement(float factor, float time)
    {
        yield return new WaitForSeconds(time);
        ModAccel /= factor;
        ModTurn /= factor;
        ModSpeed /= factor;
    }

    public void HealthPlus(float gain)
    {
        C_Health = Mathf.Clamp(C_Health + gain, 0, BaseHealth * ModHealth);
    }
    #endregion



    void ControlWeaponToggles() // For Development only
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
            if (Vector3.Dot(RB.velocity, transform.forward) > 0)
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
        if (Input.GetKey("a")) RB.AddTorque(-transform.up * C_TurnRate);
        else if (Input.GetKey("d")) RB.AddTorque(transform.up * C_TurnRate);

        // Turret (and therefore camera Rotation)
        // N.B. Turret does NOT use Physics
        if (!_turretlock)
        {
            float TurrToGo = Input.mousePosition.x / Screen.width;
            // tried various ... this seems to work best with a taper to a "dead zone" in the middle
            //  -------- A           D --------------
            //            \         /
            //              B --- C
            //
            float A = 0.46f;
            float B = 0.49f;
            float C = 1f - B;
            float D = 1f - A;
            float AB = B - A;

            if (TurrToGo <= A) { Turret.transform.Rotate(new Vector3(0, -C_TurrTurnRate * Time.deltaTime, 0)); }
            else if (TurrToGo < B)
            {
                float mod = (B - TurrToGo) / AB;
                Turret.transform.Rotate(new Vector3(0, -C_TurrTurnRate * Time.deltaTime * mod, 0));
            }
            else if (TurrToGo > C && TurrToGo < D)
            {
                float mod = (TurrToGo - C) / AB;
                Turret.transform.Rotate(new Vector3(0, C_TurrTurnRate * Time.deltaTime * mod, 0));
            }
            else if (TurrToGo >= D) { Turret.transform.Rotate(new Vector3(0, C_TurrTurnRate * Time.deltaTime, 0)); }

       }

            // Now to drag the Parent with me
            // thanks to https://forum.unity.com/threads/child-parent-dragging-not-working.30927/ (06.11.2018)
            //Vector3 relPos = LocalPlayer.transform.position - transform.position;
            //LocalPlayer.transform.position = transform.position + relPos;
            //LocalPlayer.myHullPos = transform.position;
            //LocalPlayer.myHullRot = transform.rotation;
            //LocalPlayer.myTurrPos = Turret.transform.position;
            //LocalPlayer.myTurrRot = Turret.transform.rotation;
            //LocalPlayer.transform.position = transform.position;

    }
}
