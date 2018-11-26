///////////////////////////////////////////////////////////////////////////
// CI6510 Optimised Game Programming    (Kingston University)            //
// Course Work 1 : Network Game with Plug-ins                            //
// Submission by Max Bryans (K1628007) and Taylor Fowler (K1612040)      //
// December 2018                                                         //
///////////////////////////////////////////////////////////////////////////
using UnityEngine;

public class Shrapnel : MonoBehaviour {

    private Vector3 _Vel;
    private float _IniVel = 4.0f;
    private Vector3 _UpValue = new Vector3(0, 2f, 0);
    private Vector3 _G = new Vector3(0, -1f, 0);
    private float _Life = 3.0f;


	// Use this for initialization
	void Start () {
        Destroy(gameObject, _Life);
    }
	
	// Update is called once per frame
	void Update () {
        // Apply Gravity
        _Vel += _G;
        transform.Translate(_Vel * Time.deltaTime,Space.World);
	}

    public void Configure (Vector3 EpiCentre)
    {
        _Vel = transform.position - EpiCentre;
        _Vel.Normalize();
        _Vel += _UpValue;
        _Vel *= _IniVel;
    }

}
