using System.Collections;
using UnityEngine;

public class WeatherController : MonoBehaviour {

    [SerializeField] private Material sky;
    [SerializeField] private Light sun;

    private float _fullIntensity;
    [SerializeField]
    private float _cloudValue = 0f;

    void Awake()
    {
        Messenger.AddListener(GameEvent.WEATHER_UPDATED, OnWeatherUpdated);
    }

    void OnDestroy ()
    {
        Messenger.RemoveListener(GameEvent.WEATHER_UPDATED, OnWeatherUpdated);
    }

	// Use this for initialization
	void Start () {
        _fullIntensity = sun.intensity;
	}
	
    private void OnWeatherUpdated()
    {
        SetOvercast(Managers.Weather.cloudValue);
        Debug.Log("Success: clouds being set to " + Managers.Weather.cloudValue.ToString());
    }

	// Update is called once per frame
	//void Update () {
    //    SetOvercast(_cloudValue);
    //    _cloudValue += 0.005f;
	//}

    private void SetOvercast (float value)
    {
        sky.SetFloat("_Blend", value);
        sun.intensity = _fullIntensity - (_fullIntensity * value);
    }
}
