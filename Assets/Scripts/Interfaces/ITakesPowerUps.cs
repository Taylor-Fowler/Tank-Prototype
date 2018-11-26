///////////////////////////////////////////////////////////////////////////
// CI6510 Optimised Game Programming    (Kingston University)            //
// Course Work 1 : Network Game with Plug-ins                            //
// Submission by Max Bryans (K1628007) and Taylor Fowler (K1612040)      //
// December 2018                                                         //
///////////////////////////////////////////////////////////////////////////
public interface ITakesPowerUps {

    void FireRatePlus(float factor, float time);
    void MovementPlus(float factor, float time);
    void HealthPlus(float gain);

}
