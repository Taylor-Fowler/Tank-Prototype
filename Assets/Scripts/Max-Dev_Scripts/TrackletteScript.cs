using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackletteScript : MonoBehaviour {
    [Header("Debug Serialized Fields")]
    [SerializeField] private Transform Parent;
    [SerializeField] private float _range;
    [SerializeField] private float HaveTravelled = 0f;
    [SerializeField] private float _LerpFactor = 0f;
    [SerializeField] private Vector3 _travelVector;

    [Header("Settables")]
    public Transform to;
    public Transform LeaderTrack;
    public bool LeadTrack = false;

    private Vector3 LastParentPos;
    private Vector3 CurrParentPos;
    private float ParentTravel;
    private Vector3 _toPos;
    private Quaternion _toAngle;
    //private float _toAngle;
    private Vector3 _fromPos;
    private Quaternion _fromAngle;
    //private float _fromAngle;

    private TrackletteScript MyLeader;

	// Use this for initialization
	void Start () {
        // find the ultimate Parent
        Parent = transform.root;
        LastParentPos = Parent.transform.position;

        _fromPos = transform.localPosition; // Original Position (Local)
        _toPos = to.localPosition; // Local Position of "target"
        _travelVector = _toPos - _fromPos;
        _range = Vector3.Distance(_fromPos , _toPos);

        // will get around to angles eventually
        _fromAngle = transform.localRotation;
        _toAngle = to.localRotation;
        //_fromAngle = transform.rotation.eulerAngles.x;
        //_toAngle = to.rotation.eulerAngles.x;

        if (!LeadTrack) MyLeader = LeaderTrack.GetComponent<TrackletteScript>();
	}
	
	// Update is called once per frame
	void Update () {

        // reset back to initial local position & rotation
        transform.localPosition = _fromPos;
        transform.localRotation = _fromAngle;

        if (LeadTrack) // Lead Track establishes Lerp for the whole train
        { 
            CurrParentPos = Parent.transform.position;
            ParentTravel = Vector3.Distance(LastParentPos, CurrParentPos);

            // check if parent moved "forward"
            int forward = 1;
            if (Vector3.Dot((CurrParentPos - LastParentPos), Parent.transform.forward) > 0) forward = -1;

            HaveTravelled += ParentTravel * forward;
            HaveTravelled = HaveTravelled % _range;
            _LerpFactor = HaveTravelled / _range;
            if (_LerpFactor < 0) _LerpFactor += 1f;

            LastParentPos = CurrParentPos;
        }
        else // if not Lead Track ... get the Lerp value calculated by the Lead Track
        {
            _LerpFactor = MyLeader.GetLerp();
        }

        transform.Translate((_travelVector * _LerpFactor), Space.World);
        transform.localRotation = Quaternion.Slerp(_fromAngle, _toAngle, _LerpFactor);
    }

    public float GetLerp()
    {
        return _LerpFactor;
    }
}
