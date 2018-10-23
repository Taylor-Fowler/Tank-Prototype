using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankCameraScript : MonoBehaviour {

    public Transform LowerPoint;
    public Transform UpperPoint;
    public float LowerFoV = 30;
    public float UpperFoV = 50;
    public float LowerRot = 8.5f;
    public float UpperRot = 14f;
    public float ScrollSense = 0.5f;
    [SerializeField] private float _Lerp = 0;

	// Use this for initialization
	void Start () {
        transform.localPosition = LowerPoint.localPosition;
        tag = "MainCamera";
        UpdateCameraZoom();
    }
	
	// Update is called once per frame
	void Update () {
		if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            _Lerp -= Input.GetAxis("Mouse ScrollWheel") * ScrollSense;
            _Lerp = Mathf.Clamp(_Lerp, 0, 1);
        }
        // For Dev purposes ... to find optimal camera angle/ boom positions 
        // Re-locate into above condition when ready to save a little overhead
        UpdateCameraZoom();
    }

    void UpdateCameraZoom ()
    {
        transform.localPosition = Vector3.Lerp(LowerPoint.localPosition, UpperPoint.localPosition, _Lerp);
        Camera.main.fieldOfView = LowerFoV + ((UpperFoV - LowerFoV) * _Lerp);
        transform.localEulerAngles = new Vector3( LowerRot + ((UpperRot - LowerRot) * _Lerp) ,0f,0f);
    }
}
