using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ShellType { Standard, Bouncy, Triple } // Placeholder for "when" we go back to implement different shells

public class ShellScript : MonoBehaviour
{

    // Fired with
    // void Fire (float dmg, Vector3 start, Vector3 direction, int type, Color color)

    public int OwnerID;
    public float dmg;
    public Vector3 start;
    public Quaternion direction;
    public int type;
    public Color color;
    public float velocity;
    public float life;
    public bool bouncy;
    private Rigidbody RB;
    public float ArmingTime = 0.05f; // required to prevent hitting own collider (or could eventually use an owner ID .. but that would prevent "accidental" bounce self kills)
    private bool _armed = false;

	// Use this for initialization
	void Start () {
        RB = GetComponent<Rigidbody>();
        Configure();
        Destroy(gameObject, life); // REMEMBER to set life span in Configure()
	}
	
	// Update is called once per frame
	void Update () {
        // Check Shell arming
        if (!_armed)
        {
            ArmingTime -= Time.deltaTime;
            if (ArmingTime <= 0)
            {
                ArmingTime = 0;
                _armed = true;
                RB.detectCollisions = true;
            }
        }
	}

    void Configure ()
    {
        // IMPORTANT NOTE ....
        // Bouncy shells are bugged due to some Unity Physics strangeness which makes then inconsistent
        // Triple shot also seems flaky .. Moved from "arming" of time to Having shell ignore firing collider
        // Therefore .... going to re-visit them AFTER further Network Code complete (and we decide we need more gravy)

        // record Instantiation position and rotation
        start = transform.position;
        direction = transform.rotation;

        // set Colours
        Renderer[] Rs = gameObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in Rs) r.material.color = color;

        // "Default" // maybe changed for different shells
        life = 2f;
        velocity = 10f;
        RB.detectCollisions = false; // needs to be armed
        // Switch for "Others" inc bouncy and further customising
        switch (type)
        {
            case 1: // basic Default Shell
                bouncy = false;
                break;
            case 2: // Bouncy Shell // BUGGED DO NOT USE
                bouncy = true;
                break;
            case 3: // Triple Shot // BUGGED DO NOT USE
                bouncy = false;
                for (int i = -1; i <= 1; i +=2)
                { 
                    GameObject clone = Instantiate(gameObject);
                    clone.GetComponent<ShellScript>().type = 1;
                    // Care of https://answers.unity.com/questions/316918/local-forward.html (27 Oct 2018)
                    //clone.transform.Translate(clone.transform.worldToLocalMatrix.MultiplyVector(clone.transform.forward) * 0.1f * i);
                    clone.transform.Rotate(clone.transform.up, 4 * i);
                    //clone.transform.Translate(clone.transform.forward * i * 0.01f); // a bit buggy here ..... maybe a timed callback?
                }
                break;
            default:
                break;
        }       
        RB.velocity = transform.forward * velocity;
    }

    // Collision Script
    void OnTriggerEnter(Collision col)
    {
        //if (_armed) // Currently redundant, as being armed enables Collision detection .... but we may want to toggle arming on/off and this serves that purpose.
        //{
            IDamageable dam = col.gameObject.GetComponent<IDamageable>();
            if (dam != null)
            {
                dam.TakeDamage(dmg);
                Debug.Log("Shell Injecting damage");
                Destroy(gameObject); // Add Explosion animation here too
            }
            else if (!bouncy)
            {
                Debug.Log("Shell hits something not IDamageable and dies");
                Destroy(gameObject);
            }
        //}
    }

}
