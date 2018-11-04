﻿
public class ThirtiesTank : TankBase {

    // This is what makes THIS tank special
    // The TankBase stats are customised here
	private void Reset()
    {
        BaseSpeedMax = 3f;
        BaseAccel = 20000f;
        BaseTurnRate = 30f;
        BaseTurrTurnRate = 20f;
        BaseHealth = 20;
        BaseArmour = 3;
        BaseDamage = 5;
        BaseFireRate = 0.2f;
        BaseShell = 1; // default Shell type, will be changed by Power up's
        BaseMass = 1000;
    }

}