using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//using UnityEngine.Events;
using UnityEngine.EventSystems;

public class BuildingButton : MonoBehaviour,IPointerClickHandler
{
    [SerializeField] int id;
    //
    PlayerBrain playerBrain;
    //
    [SerializeField] Image fillImage;
    [SerializeField] Image simpleImage;
    [SerializeField] float reloadTime;
    //
    float startTime;

    private void Start()
    {
        var a= GameObject.FindObjectsOfType<PlayerBrain>();
        for(int i = 0; i < a.Length; i++)
        {
            if (a[i].enabled)
                playerBrain = a[i];
        }
    }
    private void OnEnable()
    {
        fillImage.fillAmount = 1;
        StartCoroutine("Reload");
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!fillImage.enabled && !simpleImage.enabled)
        {
            playerBrain.onWaitPlant += IfPlant;
            playerBrain.EnterPlantBuilding(id);
        }
    }
    IEnumerator Reload()
    {
        fillImage.fillAmount = 1;
        fillImage.enabled = true;
        startTime = Time.time;
        while(Time.time - startTime < reloadTime)
        {
            fillImage.fillAmount = 1 - (Time.time - startTime) / reloadTime;
            yield return null;
        }
        fillImage.fillAmount = 0;
        fillImage.enabled = false;
    }
    void IfPlant(bool a)
    {
        if (a)
        {
            playerBrain.onWaitPlant -= IfPlant;
            StartCoroutine("Reload");
        }
        else
        {
            playerBrain.onWaitPlant -= IfPlant;
        }
    }
    public void OnBDPointChange(bool isEnough)
    {
        if (simpleImage.enabled == isEnough)
            simpleImage.enabled = !isEnough;
    }
}
