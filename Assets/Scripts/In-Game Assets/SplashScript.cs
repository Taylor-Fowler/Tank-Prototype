using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SplashScript : MonoBehaviour {

    #region INSPECTOR SET COMPONENTS & VALUES
    public CanvasGroup Main_Canvas;
    public RectTransform SplashPanel;
    public CanvasGroup Splash_Canvas;
    public Text UpperText;
    public Text LowerText;
    public RectTransform TankSelectPanel;
    public CanvasGroup TankSelect_Canvas;
    public float MainFade_Duration = 2f;
    public float SplashScale_Duration = 2f;
    #endregion

    [SerializeField]
    private float _MainFade_timer = 0;
    private bool _MainFade_Direction = true;

    [SerializeField]
    private float _SplashScale_timer = 0;
    private Vector3 InitialSplashScale;

    // For Tweening
    private TankHelpers Help = new TankHelpers();


    void Awake ()
    {
        InitialSplashScale = SplashPanel.localScale;

        // Make it Everything we see
        Main_Canvas.interactable = false;
        Main_Canvas.alpha = 1;
        // Activate "Game Starting"
        GameStart();
    }

    private void GameStart ()
    {
        LowerText.text = "";
        Splash_Canvas.alpha = 1;
        Splash_Canvas.interactable = false;
        UpperText.text = "Game Starting .....";
        TankSelect_Canvas.alpha = 0;
        TankSelect_Canvas.interactable = false;
        Splash_ScaleUp_MainOut();
    }
	
    public void GameOverWin ()
    {
        UpperText.text = "GRATZ";
        LowerText.text = "You Won !!";
        Splash_Canvas.alpha = 1;
        Splash_Canvas.interactable = false;
        TankSelect_Canvas.alpha = 0;
        TankSelect_Canvas.interactable = false;
        MainFade_Start(true);
        Splash_ScaleUp();
    }

	// Update is called once per frame
	void Update () {

        // TEST KEYS FOR DEBUGGING
        if (Input.GetKeyUp("1")) MainFade_Start(true);
        if (Input.GetKeyUp("2")) MainFade_Start(false);
        if (Input.GetKeyUp("3")) GameOverWin();
    }

    void OnDestroy ()
    {
        // insurance
        StopAllCoroutines();
    }

    private void Splash_ScaleUp_MainOut()
    {
        if (_SplashScale_timer != 0)
        {
            Debug.Log("[SplashScript] attempt to initiate SCALE-UP cancelled as one already running");
            return;
        }
        _SplashScale_timer = SplashScale_Duration;
        StartCoroutine("SplashScale_MainOut");
    }

    IEnumerator SplashScale_MainOut()
    {
        while (_SplashScale_timer > 0)
        {
            _SplashScale_timer = Mathf.Clamp(_SplashScale_timer - Time.deltaTime, 0, SplashScale_Duration);
            float lerp = 1f - (_SplashScale_timer / SplashScale_Duration);
            SplashPanel.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, lerp);
            yield return null;
        }
        MainFade_Start(false);
        yield return null;
    }

    private void Splash_ScaleUp()
    {
        if (_SplashScale_timer != 0)
        {
            Debug.Log("[SplashScript] attempt to initiate SCALE-UP cancelled as one already running");
            return;
        }
        _SplashScale_timer = SplashScale_Duration;
        StartCoroutine("SplashScale");
    }

    IEnumerator SplashScale()
    {
        while (_SplashScale_timer > 0)
        {
            _SplashScale_timer = Mathf.Clamp(_SplashScale_timer - Time.deltaTime, 0, SplashScale_Duration);
            float lerp = 1f - (_SplashScale_timer / SplashScale_Duration);
            SplashPanel.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, lerp);
            yield return null;
        }
        yield return null;
    }

    // fades main canvas over everything .. direction = true => fade-in, false: fade out 
    private void MainFade_Start(bool direction)
    {
        if (_MainFade_timer != 0)
        {
            Debug.Log("[SplashScript] attempt to initiate MAIN_FADE cancelled as one already running");
            return;
        }
        _MainFade_Direction = direction;
        _MainFade_timer = MainFade_Duration;
        if (direction)
            { StartCoroutine("MainFadeIn"); }
        else
            { StartCoroutine("MainFadeOut"); }
    }

    // thanks for the help @ https://www.youtube.com/watch?v=MkoIZTFUego&app=desktop (18 Nov 2018)
    IEnumerator MainFadeIn()
    {
        while (_MainFade_timer > 0)
        {
            _MainFade_timer = Mathf.Clamp(_MainFade_timer - Time.deltaTime, 0, MainFade_Duration);
            Main_Canvas.alpha = 1f - (_MainFade_timer / MainFade_Duration);
            yield return null;
        }
        Main_Canvas.interactable = true;
        yield return null;
    }

    IEnumerator MainFadeOut()
    {
        while (_MainFade_timer > 0)
        {
            _MainFade_timer = Mathf.Clamp(_MainFade_timer - Time.deltaTime, 0, MainFade_Duration);
            Main_Canvas.alpha = (_MainFade_timer / MainFade_Duration);
            yield return null;
        }
        Main_Canvas.interactable = false;
        yield return null;
    }

}
