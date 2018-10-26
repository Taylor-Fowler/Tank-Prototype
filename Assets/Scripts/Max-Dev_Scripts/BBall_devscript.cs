using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BBall_devscript : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Destroy(gameObject, 3f);
        Rigidbody RB = GetComponent<Rigidbody>();
        RB.velocity = (transform.forward * 15f);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
