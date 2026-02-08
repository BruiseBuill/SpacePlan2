using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Filter : MonoBehaviour
{
    RectTransform rectTransform;
    //
    WaitForEndOfFrame WaitForEndOfFrame;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        WaitForEndOfFrame = new WaitForEndOfFrame();
    }
    private void OnEnable()
    {
        StartCoroutine("wait");
    }
    IEnumerator wait()
    {
        yield return WaitForEndOfFrame;
        int i = 0;
        float totalHight = 0;
        for (; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).gameObject.activeSelf)
                totalHight += transform.GetChild(i).GetComponent<RectTransform>().rect.height;
        }
        rectTransform.sizeDelta = new Vector2(rectTransform.rect.width, totalHight);
    }
}
