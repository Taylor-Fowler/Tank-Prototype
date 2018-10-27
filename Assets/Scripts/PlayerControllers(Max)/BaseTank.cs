using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

namespace BaseTank
{
    public abstract class BaseTank : MonoBehaviourPun, IDamageable
    {

        public static PlayerController LocalPlayer; // what is this magic ??

        public bool TESTING = true; // toggle "true" means can test without Photon
        [Header("Core Stats")] // EITHER set in each pre-fab of Tank Type ... or Base stats for all, then modded on Start() (based on Type)
        public float BaseSpeedMax = 2f;
        public float BaseReverseSpeedMax = -5f;
        public float BaseAccel = 0.1f;
        public float BaseTurnRate = 30f;
        public float BaseTurrTurnRate = 20f;
        public float BaseHealth = 20;
        public float BaseArmour = 3;
        public float BaseDamage = 5;
        public float BaseFireRate = 0.2f;
        public int BaseShell = 1; // default Shell type, will be changed by Power up's
        public float BaseMass = 1000;


        public float CurrentSpeed { get; private set; }
        private int C_ShellType = 1;
        private Rigidbody RB;


        [Header("Player Options")]
        public Color OwnTeamColor;       // defaults to Blue @ Start() if not set.
        public Color OpponentTeamColor;  // defaults to Red @ Start() if not set.

        [SerializeField]
        private float Cooldown = 0;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void TakeDamage(float damage)
        {

        }
    }
}
