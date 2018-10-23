using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TracksLeader : MonoBehaviour {

    private Transform _Parent;
    private Vector3 _LastParentPos;
    private Vector3 _CurrParentPos;
    private float _range;
    private float _HaveTravelled = 0f;
    private float _LerpFactor = 0f;
    private int _size = 0;
    private Transform[] _childList;
    private Vector3[] _StartPositions;
    private float[] _distances;
    private int[] _index;
    private float _ParentScaleMod = 1;
    public bool TracksForward = true; // Used to customise direction, is case assembly motion is against the flow

	// Use this for initialization
	void Start ()
    {
        _ParentScaleMod = transform.root.localScale.z;
        _Parent = transform.parent.transform; // edit as appropriate to get "reference movement" object

        // Get all children and populate Arrays
        // n.b. calling object added to front of array, so need to get rid of it
        Transform[] _tempList = GetComponentsInChildren<Transform>();

        _size = _tempList.GetLength(0) - 1;
        _childList = new Transform[_size];
        _StartPositions = new Vector3[_size];
        _index = new int[_size];
        _distances = new float[_size];
        for (int i = 0; i < _size; i++)
        {
            _childList[i] = _tempList[i + 1];
            _StartPositions[i] = _childList[i].localPosition;
            _index[i] = i + 1;
        }
        _index[_size - 1] = 0; // enable wrap-around
        for (int i = 0; i < _size; i++)
        {
            _distances[i] = Vector3.Distance(_StartPositions[i], _StartPositions[_index[i]]);
        }
        _range = _distances[0]; // datum range
    }
	
	// Update is called once per frame
	void Update () {

        _CurrParentPos = _Parent.transform.position;
        float _ParentTravel = Vector3.Distance(_LastParentPos, _CurrParentPos) / _ParentScaleMod;

        // check if parent moved "forward"
        int forward = 1;
        if (!TracksForward) forward = -1;
        if (Vector3.Dot((_CurrParentPos - _LastParentPos), _Parent.transform.forward) > 0) forward = -1;

        _HaveTravelled += _ParentTravel * forward;
        _HaveTravelled = _HaveTravelled % _range;
        _LerpFactor = _HaveTravelled / _range;
        if (_LerpFactor < 0) _LerpFactor += 1f;

        for (int i = 0; i < _size; i++)
        {
            _childList[i].localPosition = Vector3.MoveTowards(_StartPositions[i], _StartPositions[_index[i]], (_LerpFactor * _distances[i]) );
        }

        _LastParentPos = _CurrParentPos;


    }
}
