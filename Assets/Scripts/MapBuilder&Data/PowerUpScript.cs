using UnityEngine;

public enum PUType { FireRate, MoveRate, Health }

public class PowerUpScript : MonoBehaviour {

    private float _RotateSpeed = 90f;
    public PUType type;

    void OnTriggerEnter (Collider col)
    {
        ITakesPowerUps hello = col.gameObject.GetComponent<ITakesPowerUps>();
        if (hello != null)
        {
            switch (type)
            {
                case PUType.FireRate:

                    break;
                case PUType.MoveRate:

                    break;
                case PUType.Health:

                    break;
                default:
                    break;
            }
        }
    }
	
	// Update is called once per frame
	void Update () {
        transform.Rotate(Vector3.up * _RotateSpeed * Time.deltaTime);
    }
}
