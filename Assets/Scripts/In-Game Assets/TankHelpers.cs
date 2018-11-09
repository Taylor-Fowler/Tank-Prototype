using UnityEngine;
/// <summary>
/// Utility Class (or Library), just to store multi-class usable functions
/// </summary>
    class TankHelpers
    { 
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

    }

