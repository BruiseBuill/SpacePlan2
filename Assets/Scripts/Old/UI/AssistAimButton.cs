using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AssistAimButton : QuickButton
{
    [SerializeField] bool isAssistAim;
    [SerializeField] Image image;

    private void Awake()
    {
        onClick += Change;
    }
    void Change()
    {
        isAssistAim = !isAssistAim;
        SetInfo(isAssistAim ? 1 : 0);
        image.color = isAssistAim ? Color.green : Color.red;
    }

}
