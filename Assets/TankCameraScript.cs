using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankCameraScript : MonoBehaviour {

    public Transform LowerPoint;
    public Transform UpperPoint;
    public float _Lerp = 0;
    public float LowerFoV = 30;
    public float UpperFoV = 50;
    public float ScrollSense = 0.25f;

	// Use this for initialization
	void Start () {
        transform.localPosition = LowerPoint.localPosition;
        tag = "MainCamera";
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            _Lerp -= Input.GetAxis("Mouse ScrollWheel") * ScrollSense;
            _Lerp = Mathf.Clamp(_Lerp, 0, 1);
            transform.localPosition = Vector3.Lerp(LowerPoint.localPosition, UpperPoint.localPosition, _Lerp);
            
        }
	}
}
