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

	// Use this for initialization
	void Start () {
        Debug.Log("Shell Fired");
        Configure();
        Destroy(gameObject, life); // REMEMBER to set life span in Configure()
	}
	
	// Update is called once per frame
	void Update () {
        // Move
        transform.Translate(transform.forward * velocity * Time.deltaTime);
	}

    void Configure ()
    {
        // record Instantiation position and rotation
        //start = transform.position;
        //direction = transform.rotation;

        // set Colours
        Renderer[] Rs = gameObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in Rs) r.material.color = color;

        // "Default"
        life = 5f;
        velocity = 1f;

        // Switch for "Others" inc bouncy and further customising
        switch (type)
        {
            case 1:
                break;
            default:
                break;
        }
    }

}
