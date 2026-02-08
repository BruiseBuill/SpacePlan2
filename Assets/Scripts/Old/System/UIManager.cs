using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BF;

public class UIManager : Single<UIManager>
{
    Text winText;
    Text infoText;
    //

    protected void Awake()
    {
        winText = GameObject.FindGameObjectWithTag("winText").GetComponent<Text>();
        infoText = GameObject.FindGameObjectWithTag("infoText").GetComponent<Text>();
    }
    public void SetWinText(string content)
    {
        winText.enabled = true;
        winText.text = content;
    }
    public void SetInfoText(string content)
    {
        infoText.gameObject.SetActive(true);
        infoText.text = content;
    }
}
