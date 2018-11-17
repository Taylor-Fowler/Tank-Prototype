using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelScript : MonoBehaviour {

    public RectTransform DamagePanel;
    public RectTransform LeftPanel;
    public RectTransform RightPanel;
    public Text Name;
    public Text Score;
    public Color BGColor;
    
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
        if (Health < 0f) Health = 0f;
        if (Health > 1f)
        {
            Health = 1f;
            Debug.Log("[PanelScript] Cannot set Health to more than 100%");
        }
        DamagePanel.localScale = new Vector3((1f - Health), DamagePanel.localScale.y, DamagePanel.localScale.z);
    }

    // Called by GUI Manager if no player for this panel
    public void DeActivate()
    {
        this.gameObject.SetActive(false);
    }




}
