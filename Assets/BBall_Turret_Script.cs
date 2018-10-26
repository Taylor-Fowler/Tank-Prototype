using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BBall_Turret_Script : MonoBehaviour {

    public Transform FiringPoint;
    public Transform BBall;
    public float FireRate = 0.5f;
    public float TurnRate = 30;
    private float _Cooldown = 0;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        HandleMovement();
        HandleFiring();
	}

    void HandleMovement()
    {
        if (Input.GetKey("a")) transform.Rotate(transform.up * -TurnRate * Time.deltaTime);
        if (Input.GetKey("d")) transform.Rotate(transform.up * TurnRate * Time.deltaTime);
    }

    void HandleFiring()
    {
        _Cooldown = Mathf.Max(0, _Cooldown - Time.deltaTime);
        if (Input.GetKey("space") && _Cooldown <= 0)
        {
            Instantiate(BBall, FiringPoint.transform.position, FiringPoint.rotation);
            _Cooldown = FireRate;
        }
    }


}
