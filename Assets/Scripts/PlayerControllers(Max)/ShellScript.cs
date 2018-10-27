using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShellScript : MonoBehaviour
{

    // Fired with
    // void Fire (float dmg, Vector3 start, Vector3 direction, int type, Color color)

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

	// Use this for initialization
	void Start () {
        RB = GetComponent<Rigidbody>();
        Configure();
        Destroy(gameObject, life); // REMEMBER to set life span in Configure()
	}
	
	// Update is called once per frame
	void Update () {
        // Movement handled by RigidBody
        ArmingTime = Mathf.Max(0, ArmingTime - Time.deltaTime);
	}

    void Configure ()
    {
        // record Instantiation position and rotation
        start = transform.position;
        direction = transform.rotation;

        // set Colours
        Renderer[] Rs = gameObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in Rs) r.material.color = color;

        // "Default" // maybe changed for different shells
        life = 2f;
        velocity = 10f; 
        // Switch for "Others" inc bouncy and further customising
        switch (type)
        {
            case 1: // basic Default Shell
                bouncy = false;
                break;
            case 2: // Bouncy Shell
                bouncy = true;
                break;
            case 3: // Triple Shot
                bouncy = false;
                for (int i = -1; i <= 1; i +=2)
                { 
                    GameObject clone = Instantiate(gameObject);
                    clone.GetComponent<ShellScript>().type = 1;
                    // Care of https://answers.unity.com/questions/316918/local-forward.html (27 Oct 2018)
                    //clone.transform.Translate(clone.transform.worldToLocalMatrix.MultiplyVector(clone.transform.forward) * 0.1f * i);
                    clone.transform.Rotate(clone.transform.up, 4 * i);
                    clone.transform.Translate(clone.transform.forward * i * 0.01f); // a bit buggy here ..... maybe a timed callback?
                }
                break;
            default:
                break;
        }
        
        RB.velocity = transform.forward * velocity;

    }

    // Collision Script
    void OnCollisionEnter(Collision col)
    {
        //if (ArmingTime > 0) Debug.Log("Too soon : Arming = " + ArmingTime.ToString());
        if (ArmingTime <= 0)
        {
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
        }
    }

}
