using UnityEngine;
using System.Collections;

public enum PUType { FireRate, MoveRate, Health }

public class PowerUpScript : MonoBehaviour {

    private float _RotateSpeed = 90f;
    public PUType type;
    private float _HideTime;
    private Renderer[] _Renderers;
    private Collider[] _Colliders;

    void Start() 
    {
        // initial configure, since there will probably be multiple collisions over the game ..
        // .. why get these references or set variables more than once?
        _Renderers = GetComponentsInChildren<Renderer>();
        _Colliders = GetComponentsInChildren<Collider>();
        switch (type)
        {
            case PUType.FireRate:
                _HideTime = 20f;
                break;
            case PUType.Health:
                _HideTime = 40f;
                break;
            case PUType.MoveRate:
                _HideTime = 30f;
                break;
            default:
                _HideTime = 20f;
                break;
        }
    }

    void OnTriggerEnter (Collider col)
    {
        // Talk to the Interface ....
        ITakesPowerUps hello = col.gameObject.GetComponent<ITakesPowerUps>();
        if (hello != null)
        {
            switch (type)
            {
                case PUType.FireRate:
                    hello.FireRatePlus(2f, 5f);
                    Debug.Log("Fire Rate PU");
                    break;
                case PUType.MoveRate:
                    hello.MovementPlus(2f, 5);
                    Debug.Log("Movement PU");
                    break;
                case PUType.Health:
                    hello.HealthPlus(5f);
                    Debug.Log("Health PU eaten");
                    break;
                default:
                    break;
            }
        // Whatever happened above, it was hit, need to hide it
        StartCoroutine(HideFor());
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
        foreach (Renderer r in _Renderers) r.enabled = false;
        foreach (Collider c in _Colliders) c.enabled = false;
    }
	
    void UnHideMe()
    {
        Debug.Log(type.ToString() + " revealed");
        foreach (Renderer r in _Renderers) r.enabled = true;
        foreach (Collider c in _Colliders) c.enabled = true;
    }

	// Update is called once per frame
	void Update () {
        transform.Rotate(Vector3.up * _RotateSpeed * Time.deltaTime);
    }
}
