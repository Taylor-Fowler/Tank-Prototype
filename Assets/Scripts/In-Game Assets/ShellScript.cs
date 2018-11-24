using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ShellType { Standard, Bouncy, Triple } // Placeholder for "when" we go back to implement different shells

public class ShellScript : MonoBehaviour
{
    // Instantiated by void Fire() (in TankBase)
      //  *Set by TB*
    public int OwnerID;          
    public float dmg;           
    public ShellType type;            
    public Color color;  
      // For future "DeathCam" use          
    public Vector3 start;   
    public Quaternion direction; 
      // * set in Configure() *
    public float velocity;    
    public float life;
    public bool bouncy;
    private Rigidbody _RB;

    public GameObject ExpPreFab;

	// Use this for initialization
	void Start () {
        _RB = GetComponent<Rigidbody>();
        Configure();
        Destroy(gameObject, life); // REMEMBER to set life span in Configure()
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
        life = 3f;
        velocity = 10f;
        // Switch for "Others" inc bouncy and further customising
        switch (type)
        {
            case ShellType.Standard: // basic Default Shell
                bouncy = false;
                break;
            case ShellType.Bouncy: // Bouncy Shell // BUGGED DO NOT USE
                bouncy = true;
                break;
            case ShellType.Triple: // Triple Shot // BUGGED DO NOT USE
                bouncy = false;
                for (int i = -1; i <= 1; i +=2)
                { 
                    GameObject clone = Instantiate(gameObject);
                    clone.GetComponent<ShellScript>().type = ShellType.Standard;
                    // Care of https://answers.unity.com/questions/316918/local-forward.html (27 Oct 2018)
                    //clone.transform.Translate(clone.transform.worldToLocalMatrix.MultiplyVector(clone.transform.forward) * 0.1f * i);
                    clone.transform.Rotate(clone.transform.up, 4 * i);
                    //clone.transform.Translate(clone.transform.forward * i * 0.01f); // a bit buggy here ..... maybe a Co-Routine?
                }
                break;
            default:
                break;
        }       
        _RB.velocity = transform.forward * velocity;
    }

    // Collision Script
    void OnTriggerEnter(Collider col)
    {
            IDamageable dam = col.gameObject.GetComponent<IDamageable>();
            if (dam != null)
            {
                dam.TakeDamage(OwnerID, dmg);

                // Explosion animation here too
                GameObject boom = Instantiate(ExpPreFab, transform.position , Quaternion.identity) as GameObject;
                boom.transform.LookAt(start);
                Destroy(boom, 1);

                Destroy(gameObject); 
            }
            else if (!bouncy)
            {
                Destroy(gameObject);
            }
    }

}
