
public class SpaceTank : TankBase {

    // This is what makes THIS tank special
    // The TankBase stats are customised here
    private void Reset()
    {
        BaseSpeedMax = 5f;
        BaseAccel = 30000f;
        BaseTurnRate = 50f;
        BaseTurrTurnRate = 40f;
        BaseHealth = 15;
        BaseArmour = 2;
        BaseDamage = 4;
        BaseFireRate = 0.1f;
        BaseShell = 1; // default Shell type, will be changed by Power up's
        BaseMass = 1000;
    }
}
