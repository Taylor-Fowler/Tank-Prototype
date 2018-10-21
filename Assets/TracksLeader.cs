﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TracksLeader : MonoBehaviour {

    [Header("Debug Serialized Fields")]
    [SerializeField] private Transform _Parent;
    [SerializeField] private Vector3 _LastParentPos;
    [SerializeField] private Vector3 _CurrParentPos;
    [SerializeField] private float _range;
    [SerializeField] private float _HaveTravelled = 0f;
    [SerializeField] private float _LerpFactor = 0f;
    [SerializeField] private int _size = 0;
    [SerializeField] Transform[] _childList;
    [SerializeField] Vector3[] _StartPositions;
    [SerializeField] float[] _distances;
    [SerializeField] int[] _index;

	// Use this for initialization
	void Start ()
    {
        _Parent = transform.parent.transform;

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
            if (i == _size - 1) _index[i] = 0; // Enable wraparound
            _distances[i] = Vector3.Distance(_StartPositions[i], _StartPositions[_index[i]]);
            Debug.Log("Distance from element " + i.ToString() + " to " + _index[i].ToString() + " = " + _distances[i].ToString());
        }
        _range = Vector3.Distance(_StartPositions[0], _StartPositions[1]); // datum range from element 0 to 1
    }
	
	// Update is called once per frame
	void Update () {

        _CurrParentPos = _Parent.transform.position;
        float _ParentTravel = Vector3.Distance(_LastParentPos, _CurrParentPos);

        // check if parent moved "forward"
        int forward = 1;
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
