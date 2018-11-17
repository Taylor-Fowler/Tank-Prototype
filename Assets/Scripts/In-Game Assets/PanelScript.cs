using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelScript : MonoBehaviour {

    public RectTransform DamagePanel;
    public RectTransform LeftPanel;
    public RectTransform RightPanel;
    public RectTransform Flash;
    public float FlashTime = 0.1f;
    public Text Name;
    public Text Score;
    private Color BGColor;
    private float _lastHealth = 0f;
    

    
    public void SetName(string name)
    {
        Name.text = name;
    }

    public void SetScore(int UpdatedScore)
    {
        Score.text = UpdatedScore.ToString();
    }

    public void SetColor (Color col)
    {
        BGColor = col;
        LeftPanel.GetComponent<Image>().color = BGColor;
        RightPanel.GetComponent<Image>().color = BGColor;
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
        Flash.localScale = new Vector3((_lastHealth - Health) * (1f - DamagePanel.localScale.x), Flash.localScale.y, Flash.localScale.z);
        StartCoroutine("StopFlash");
        DamagePanel.localScale = new Vector3((1f - Health), DamagePanel.localScale.y, DamagePanel.localScale.z);

        _lastHealth = Health;
    }

    private IEnumerator StopFlash()
    {
        yield return new WaitForSeconds(FlashTime);
        Flash.localScale = new Vector3(0f, Flash.localScale.y, Flash.localScale.z);
    }


    // Called by GUI Manager if no player for this panel
    public void DeActivate()
    {
        StopAllCoroutines();
        this.gameObject.SetActive(false);
    }
    void OnDestroy()
    {
        // insurance policy
        StopAllCoroutines();
    }




}
