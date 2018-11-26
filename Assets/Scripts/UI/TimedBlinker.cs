///////////////////////////////////////////////////////////////////////////
// CI6510 Optimised Game Programming    (Kingston University)            //
// Course Work 1 : Network Game with Plug-ins                            //
// Submission by Max Bryans (K1628007) and Taylor Fowler (K1612040)      //
// December 2018                                                         //
///////////////////////////////////////////////////////////////////////////
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimedBlinker : MonoBehaviour
{
    public float BlinkTime = 1f;
    [SerializeField] private GameObject _blinkObject;

    private float _internalTimer = 0f;

    private void Update()
    {
        _internalTimer += Time.deltaTime;

        if(_internalTimer >= BlinkTime)
        {
            _internalTimer -= BlinkTime;
            _blinkObject.SetActive(!_blinkObject.activeSelf);
        }
    }
}
