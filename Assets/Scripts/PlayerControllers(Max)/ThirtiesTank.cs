using UnityEngine;

namespace BaseTank
{
    public class ThirtiesTank : BaseTank
    {

        public GameObject Turret;
        public Transform _firePos;
        public Transform Shell;

        public Vector3 Spawn = new Vector3(20f, 0.21f, 20f); // Dev purposes spawn only

        [Header("Stat Modifiers")] // What makes this tank "special"
        public float ModSpeed = 1f;
        public float ModAccel = 1f;
        public float ModTurn = 1f;
        public float ModTurrTurn = 1f;
        public float ModHealth = 1f;
        public float ModArmour = 1f;
        public float ModDamage = 1f;
        public float ModFireRate = 1f;
        public float ModMass = 1f;
        private int C_ShellType = 1;

        float C_SpeedMax { get { return BaseSpeedMax * ModSpeed; } } // N.B. no setter
        float C_ReverseSpeedMax { get { return BaseReverseSpeedMax * ModSpeed; } }
        float C_Accel { get { return BaseAccel * ModAccel; } }
        float C_TurnRate { get { return BaseTurnRate * ModTurn; } }
        float C_TurrTurnRate { get { return BaseTurrTurnRate * ModTurrTurn; } }
        float C_Health { get { return BaseHealth * ModHealth; } } // Placeholder only until mechanics established
        float C_Armour { get { return BaseArmour * ModArmour; } } // Placeholder only until mechanics established
        float C_Damage { get { return BaseDamage * ModDamage; } } // Placeholder only until mechanics established
        float C_FireRate { get { return BaseFireRate * ModFireRate; } }

    }
}
