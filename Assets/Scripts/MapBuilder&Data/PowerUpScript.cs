using UnityEngine;
using System.Collections;

public enum PUType { FireRate, MoveRate, Health }

public class PowerUpScript : MonoBehaviour {

    private float _RotateSpeed = 90f;
    public PUType type;
    private float _HideTime;

    void Start()
    {
        switch (type)
        {
            case PUType.FireRate:
                _HideTime = 2f;
                break;
            case PUType.Health:
                _HideTime = 2f;
                break;
            case PUType.MoveRate:
                _HideTime = 2f;
                break;
            default:
                _HideTime = 2f;
                break;
        }
    }

    void OnTriggerEnter (Collider col)
    {
        ITakesPowerUps hello = col.gameObject.GetComponent<ITakesPowerUps>();
        if (hello != null)
        {
            switch (type)
            {
                case PUType.FireRate:
                    hello.FireRatePlus(2f, 5f);
                    Debug.Log("Fire Rate PU");
                    StartCoroutine(HideFor());
                    break;
                case PUType.MoveRate:
                    hello.MovementPlus(2f, 5);
                    Debug.Log("Movement PU");
                    StartCoroutine(HideFor());
                    break;
                case PUType.Health:
                    hello.HealthPlus(5f);
                    Debug.Log("Health PU eaten");
                    StartCoroutine(HideFor());
                    break;
                default:
                    break;
            }
        }
    }

    IEnumerator HideFor()  
    {
        HideMe();
        yield return new WaitForSeconds(_HideTime);
        UnHideMe();
    }

    void HideMe()
    {
        Debug.Log(type.ToString() + " hidden");
        gameObject.SetActive(false);
    }
	
    void UnHideMe()
    {
        gameObject.SetActive(true);
        Debug.Log(type.ToString() + " revealed");
    }

	// Update is called once per frame
	void Update () {
        transform.Rotate(Vector3.up * _RotateSpeed * Time.deltaTime);
    }
}
