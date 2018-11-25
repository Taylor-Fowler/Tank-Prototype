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
