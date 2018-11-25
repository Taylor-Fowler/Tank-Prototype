using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LoadingPanel : MonoBehaviour
{
    public Button Close;
    
    [SerializeField] private Text _loadingText;
    private string _loadingTextMessage;
    private Action _cancelAction;
    private int _dotIndex = 0;
    private float _timeSinceLastDot = 0f;
    private Coroutine _turnOffRoutine;


    public void StartLoading(string message, string cancelMessage, float cancelTimer, Action cancelAction)
    {
        gameObject.SetActive(true);
        _cancelAction = cancelAction;
        _loadingText.text = message;
        _loadingTextMessage = message;
        Close.onClick.AddListener(delegate
        {
            TurnOffPanel(cancelMessage, cancelTimer);
        });
    }

    public void TurnOffPanel(string message, float timer)
    {
        // The loading has already been triggered for turn off
        if(_turnOffRoutine != null)
        {
            return;
        }

        _loadingTextMessage = message;
        _dotIndex = 0;
        _timeSinceLastDot = 0f;
        _loadingText.text = message;

        _turnOffRoutine = StartCoroutine(DelayTurnOff(timer));
    }

    public void LoadingRoutineExternallyEnded()
    {
        _cancelAction = null;
    }

    private IEnumerator DelayTurnOff(float timer)
    {
        do
        {
            yield return null;
            timer -= Time.deltaTime;
        } while(timer > 0f);

        StopLoading();
    }

    private void StopLoading()
    {
        if(_cancelAction != null)
        {
            _cancelAction();
        }
        _turnOffRoutine = null;

        Close.onClick.RemoveAllListeners();
        _loadingText.text = "";
        _timeSinceLastDot = 0f;
        _dotIndex = 0;
        gameObject.SetActive(false);
    }

    private void Update()
    {
        _timeSinceLastDot += Time.deltaTime;

        if(_timeSinceLastDot >= 0.5f)
        {
            _timeSinceLastDot -= 0.5f;
            _dotIndex++;

            if(_dotIndex > 3)
            {
                _dotIndex = 0;
            }
            string message = _loadingTextMessage;
            for(int i = 0; i < _dotIndex; i++)
            {
                message += ".";
            }

            _loadingText.text = message;
        }
    }
}
