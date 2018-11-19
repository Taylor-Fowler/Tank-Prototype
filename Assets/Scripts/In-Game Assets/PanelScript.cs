using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelScript : MonoBehaviour {

    #region Inspector Settables Public Vars

    [Header("Components (in scene)")]
    public RectTransform HealthPanel;
    public RectTransform LeftPanel;
    public RectTransform RightPanel;
    public RectTransform Flash;
    public Text Name;
    public Text Score;
    #endregion

    #region Private Vars

    private Color _BGColor;
    private float _FlashTime = 0.1f;
    private float _lastHealth = 0f;
    #endregion

    #region Public Methods

    public void SetName(string name)
    {
        Name.text = name;
    }

    public void SetScore(int UpdatedScore)
    {
        Score.text = UpdatedScore.ToString();
    }

    public void SetFlashTime(float Time)
    {
        _FlashTime = Time;
    }

    public void SetColor (Color col)
    {
        _BGColor = col;
        LeftPanel.GetComponent<Image>().color = _BGColor;
        RightPanel.GetComponent<Image>().color = _BGColor;
    }

    public void SetHealth(float Health)
    {
        if (Health == _lastHealth) return; // no change, do nothing

        if (Health < 0f) Health = 0f;
        if (Health > 1f)
        {
            Health = 1f;
            Debug.Log("[PanelScript] Cannot set Health to more than 100%");
        }
        StartCoroutine("StopFlash");
        HealthPanel.localScale = new Vector3(Health, HealthPanel.localScale.y, HealthPanel.localScale.z);

        _lastHealth = Health;
    }

    // Called by GUI Manager if no player for this panel
    public void DeActivate()
    {
        StopAllCoroutines();
        this.gameObject.SetActive(false);
    }
    #endregion


    #region Unity Monobehaviour API

    void OnDestroy()
    {
        // insurance policy
        StopAllCoroutines();
    }

    #endregion

    #region CoRoutines

    private IEnumerator StopFlash()
    {
        yield return new WaitForSeconds(_FlashTime);
        Flash.localScale = HealthPanel.localScale;
    }

    #endregion


}
