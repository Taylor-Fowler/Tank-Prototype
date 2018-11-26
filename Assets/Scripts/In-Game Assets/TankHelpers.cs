///////////////////////////////////////////////////////////////////////////
// CI6510 Optimised Game Programming    (Kingston University)            //
// Course Work 1 : Network Game with Plug-ins                            //
// Submission by Max Bryans (K1628007) and Taylor Fowler (K1612040)      //
// December 2018                                                         //
///////////////////////////////////////////////////////////////////////////
using UnityEngine;
/// <summary>
/// Utility Class, just to store multi-class usable functions
/// </summary>
    class TankHelpers
    { 
    /// <summary>
    /// COLOR MANIPULATORS 
    /// </summary>

        public Vector3 ColorToV3(Color color)
        {
            return new Vector3(color.r, color.g, color.b);
        }

        public Color V3ToColor (Vector3 Vect3)
        {
            return new Color(Vect3.x, Vect3.y, Vect3.z, 1f);
        }

        public Color RandomColor()
        {
            return new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1f);
        }

        public Color InvertColor(Color ColorToInvert)
        {
            return new Color(1f - ColorToInvert.r, 1f - ColorToInvert.g, 1f - ColorToInvert.b);
        }

    /// <summary>
    /// TWEENING FORMULA 
    /// Adapted from https://stackoverflow.com/questions/13462001/ease-in-and-ease-out-animation-formula (19 Nov 2018)
    /// </summary>

    public float InOutQuadBlend(float t)
    {
        if (t <= 0.5f)
            return 2.0f * Mathf.Pow(t, 2);
        t -= 0.5f;
        return (float) 2.0f * t * (1.0f - t) + 0.5f;
    }

    public float BezierBlend(float t)
    {
        return Mathf.Pow(t, 2) * (3.0f - 2.0f * t);
    }

    public float ParametricBlend(float t)
    {
        float sqt = Mathf.Pow(t,2);
        return sqt / (2.0f * (sqt - t) + 1.0f);
    }

    public float TrygoEase(float t)
    {
        return (float) Mathf.Pow(Mathf.Sin(5 * t / Mathf.PI), 2);
    }

    public float ParabolEase(float t)
    {
        float y = 2 * t * t;
        if (t > 0.5f)
        {
            t -= 1;
            y = -2 * t * t + 1;
        }
        return y;
    }

    // More from https://joshondesign.com/2013/03/01/improvedEasingEquations (19/11/2108)
    // Okies .... a bit buggy !!
    public float easeOutElastic(float t)
    {
        float p = 0.3f;
        return Mathf.Pow(2, -10 * t) * Mathf.Sin((t - p / 4) * (2 * Mathf.PI) / p) + 1f;
    }

    public float easeInElastic (float t)
    {
        return easeOutElastic(1f - t);
    }



}

