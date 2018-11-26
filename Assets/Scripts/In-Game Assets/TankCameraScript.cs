///////////////////////////////////////////////////////////////////////////
// CI6510 Optimised Game Programming    (Kingston University)            //
// Course Work 1 : Network Game with Plug-ins                            //
// Submission by Max Bryans (K1628007) and Taylor Fowler (K1612040)      //
// December 2018                                                         //
///////////////////////////////////////////////////////////////////////////
using UnityEngine;

public class TankCameraScript : MonoBehaviour {

    #region PUBLIC / INSPECTOR MEMBERS
    public Transform LowerPoint;
    public Transform UpperPoint;
    public float LowerFoV = 30;
    public float UpperFoV = 50;
    public float LowerRot = 8.5f;
    public float UpperRot = 14f;
    public float ScrollSense = 0.5f;
    #endregion

    #region PRIVATE MEMBERS
    [SerializeField] private float _Lerp = 0;
    #endregion

    #region PUBLIC METHODS
    public void Activate()
    {
        transform.localPosition = UpperPoint.localPosition;
        tag = "MainCamera";
        UpdateCameraZoom();
    }
    #endregion

    #region PRIVATE METHODS
    void UpdateCameraZoom ()
    {
        transform.localPosition = Vector3.Lerp(LowerPoint.localPosition, UpperPoint.localPosition, _Lerp);
        Camera.main.fieldOfView = LowerFoV + ((UpperFoV - LowerFoV) * _Lerp);
        transform.localEulerAngles = new Vector3( LowerRot + ((UpperRot - LowerRot) * _Lerp) ,0f,0f);
    }
    #endregion

    #region UNITY API
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
    #endregion


}
