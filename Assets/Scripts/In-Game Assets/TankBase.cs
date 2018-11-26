using System.Collections;
using Photon.Pun;
using UnityEngine;

public abstract class TankBase : MonoBehaviourPun, IDamageable, ITakesPowerUps
{
    #region Public Vars
    [Header("Components (Pre-fabs)")]
    public GameObject CameraPrefab;
    public Transform CameraAnchor;
    public GameObject Turret;
    public Transform FirePos;
    public Transform Shell;
    public GameObject ExpPreFab;
    public Collider Col; // reference for own shells to ignore
    public SoundManager SM;

    [Header("Core Stats")] // Core Stats are Customised by Children
    public float BaseSpeedMax = 1f;
    public float BaseAccel = 1f;
    public float BaseTurnRate = 1f;
    public float BaseTurrTurnRate = 1f;
    public float BaseHealth = 1;
    public float BaseArmour = 1;
    public float BaseDamage = 1;
    public float BaseFireRate = 1f;
    public int BaseShell = 1; // default Shell type, (maybe) changed by Power up's
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

    #endregion

    #region Private Vars
    private int _MyPlayerID;
    private Rigidbody _RB;
    private Vector3 _MyV3Color;
    private TankHelpers _Help = new TankHelpers();
    private bool _turretlock = false;
    [SerializeField]
    private float Cooldown = 0;
    #endregion

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

    public bool AutoFire = false;

    #region UNITY API
    private void Awake()
    {
        // set the "settables" & report them
        CurrentSpeed = 0f;
        C_Health = BaseHealth * ModHealth;
    }

    private void Start ()
    {
        // For OTHER player representation in Scene .. swith on RigidBody Kinematics (stops a lot of juddering)
        if (photonView.IsMine == false && PhotonNetwork.IsConnected == true)
        {
            _RB = GetComponent<Rigidbody>();
            _RB.isKinematic = true;
            return;
        }

        Instantiate(CameraPrefab, CameraAnchor);
        _RB = GetComponent<Rigidbody>();
        if (_RB == null) Debug.Log(" No RB found");
        _RB.mass = C_Mass;
    }

    private void Update()
    {
        // Only For Active Player
        if (photonView.IsMine == false && PhotonNetwork.IsConnected == true)
        {
            return;
        }
        ControlMovement();
        ControlFiring();
        CheckTurretLock();
    }

    private void OnDestroy()
    {
        // death insurance
        StopAllCoroutines();
    }

    #endregion

    #region Public Methods (called by PlayerController)

    public void SetPlayerID(int ID)
    {
        _MyPlayerID = ID;
    }

    public void ChangeColor(Vector3 ColorV)
    {
        _MyV3Color = ColorV;
       Color MyColor = _Help.V3ToColor(_MyV3Color);
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


    public void Fire(int playerID)
    {
        SM.PlaySFX(SFX.Fire);
        Transform shell = (Transform)Instantiate(Shell, FirePos.transform.position, FirePos.transform.rotation);
        ShellScript ss = shell.GetComponent<ShellScript>();
        ss.OwnerID = playerID;
        ss.dmg = C_Damage;
        ss.type = ShellType.Standard;
        Color MyColor = _Help.V3ToColor(_MyV3Color);
        ss.color = MyColor;
        // Get Shell to ignore Firing Units Collider
        // https://docs.unity3d.com/ScriptReference/Physics.IgnoreCollision.html (03 Nov 2018)
        Physics.IgnoreCollision(ss.GetComponent<Collider>(), Col);
    }

    public void TankDie()
    {
        SM.PlaySFX(SFX.Boom);
        GameObject boom = Instantiate(ExpPreFab, transform.position, Quaternion.identity) as GameObject;
        Destroy(boom, 2);
        PhotonView[] _MyPhotons = GetComponentsInChildren<PhotonView>();
        Transform[] _MyShrapnel = GetComponentsInChildren<Transform>();
        foreach (Transform p in _MyShrapnel)
        {
            p.transform.parent = null;
            p.gameObject.AddComponent<Shrapnel>();
            p.gameObject.GetComponent<Shrapnel>().Configure(transform.position);
        }
        // Kill child PhotonViews
        foreach (PhotonView p in _MyPhotons)
        {
            PhotonNetwork.Destroy(p);
        }
        // Kill TankBase PhotonView
        PhotonNetwork.Destroy(this.GetComponent<PhotonView>());

    }
    #endregion

    #region Interface IDamageable Implementation
    public void TakeDamage(int ShellOwnerID, float damage)
    {
        // Only the Local player will process the damage and tell everybody else about it
        if(!photonView.IsMine)
        {
            return;
        }
        SM.PlaySFX(SFX.Hit);
        float pen = damage - C_Armour;
        if (pen > 0)
        {
            PlayerController.LocalPlayer.TakeDamage(ShellOwnerID, damage);
        }
    }
    #endregion

    #region  Interface ITakesPowerUps Implementation (and revertion CoRoutines)
    public void FireRatePlus(float factor, float time)
    {
        ModFireRate /= factor;
        StartCoroutine(RevertFireRate(factor, time));
    }

    IEnumerator RevertFireRate (float factor, float time)
    {
        yield return new WaitForSeconds(time);
        ModFireRate *= factor;
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
        PlayerController.LocalPlayer.RecievePowerUpHealth(gain, _MyPlayerID);
    }
    #endregion


    #region Private Methods (aka Player Controls)

    void ControlFiring()
    {
        // Reduce cooldown
        Cooldown = Mathf.Max(0, Cooldown - Time.deltaTime);
        if (Cooldown <= 0 && AutoFire || (Input.GetMouseButtonDown(0) && Cooldown <= 0) && !AutoFire) // Auto fire for testing
        {
            PlayerController.LocalPlayer.Fire(); // call via the PlayerController so it can call Fire via RPC.
            Cooldown = C_FireRate;
        }
    }

    void CheckTurretLock()
    {
        if (Input.GetKeyDown("space")) _turretlock = !_turretlock;
        if (Input.GetKeyDown("p")) AutoFire = !AutoFire;
    }

    void ControlMovement()
    {
        // Tank Hull Forward / Backward input
        if (Input.GetKey("w")) _RB.AddForce(transform.forward * C_Accel);
        if (Input.GetKey("s")) _RB.AddForce(-transform.forward * C_Accel);
        // no input deceleration ... stops @ 0
        if (!Input.GetKey("w") && !Input.GetKey("s") && _RB.velocity.magnitude != 0)
        {
            // going forwards
            if (Vector3.Dot(_RB.velocity, transform.forward) > 0)
            { _RB.AddForce(-transform.forward * C_Accel / 4); }
            // backwards
            else
            { _RB.AddForce(transform.forward * C_Accel / 4); }

            // bring to graceful halt if close to 0
            if (_RB.velocity.magnitude < 0.1 && _RB.velocity.magnitude > -0.1)
            { _RB.velocity = Vector3.zero; }
        }

        // clamp velocity
        if (_RB.velocity.magnitude > C_SpeedMax)
        { _RB.velocity = Vector3.ClampMagnitude(_RB.velocity, C_SpeedMax); }

        // Eliminate any annoying sideways movement
        _RB.velocity = Vector3.Project(_RB.velocity, transform.forward);


        // Tank Hull Rotation
        if (Input.GetKey("a")) _RB.AddTorque(-transform.up * C_TurnRate);
        else if (Input.GetKey("d")) _RB.AddTorque(transform.up * C_TurnRate);

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
            float A = 0.42f;
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

    }
    #endregion
}
