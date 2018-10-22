using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using Photon.Pun;
using UnityEngine;

public class ThirtiesTankControls : MonoBehaviourPun {

    public bool TESTING = true;
    public float SpeedMax = 2f;
    public float ReverseSpeedMax = -5f;
    public float Accel = 0.1f;
    public float TurnRate = 30f;
    public float CurrentSpeed = 0f;

    // Use this for initialization
    void Start()
    {
        if (!photonView.IsMine && !TESTING) return; // Not local Player ... don't bother

    }


    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine && !TESTING) return; // Not local Player ... don't bother
        float turning = 0;
        if (Input.GetKeyDown("w"))
        {
            CurrentSpeed += Accel;
            CurrentSpeed = Mathf.Clamp(CurrentSpeed, ReverseSpeedMax, SpeedMax);
        }
        if (Input.GetKeyDown("s"))
        {
            CurrentSpeed -= Accel;
        }
        CurrentSpeed = Mathf.Clamp(CurrentSpeed, ReverseSpeedMax, SpeedMax);
        transform.Translate(CurrentSpeed * Time.deltaTime * Vector3.forward);
        if (Input.GetKey("a"))
        {
            turning += TurnRate;
        }
        else if (Input.GetKey("d"))
        {
            turning -= TurnRate;
        }
        transform.Rotate(turning * Time.deltaTime * Vector3.up);
    }
}

