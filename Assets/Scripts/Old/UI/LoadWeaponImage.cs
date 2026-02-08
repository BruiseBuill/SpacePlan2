using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class LoadWeaponImage : MonoBehaviour
{
    public UnityAction onButtonDown = delegate { };
    GameObject[] weaponImageObject;
    //
    WeaponCP weaponCP;
    List<int> ids;

    private void Awake()
    {
        weaponImageObject = new GameObject[transform.childCount];
        for(int i = 0; i < transform.childCount; i++)
        {
            weaponImageObject[i] = transform.GetChild(i).gameObject;
            weaponImageObject[i].GetComponent<QuickButton>().onClickWithIndex += OnButtonDown;
        }
    }
    public void SetImages(WeaponCP weaponCP)
    {
        int i = 0;
        this.weaponCP = weaponCP;
        var a = weaponCP.ReturnWeaponImage();
        for (i = 0; i < weaponImageObject.Length; i++)
        {
            weaponImageObject[i].SetActive(false);
        }
        for( i= 0; i < a.Count; i++)
        {
            weaponImageObject[i].GetComponent<Image>().sprite = a[i];
            weaponImageObject[i].SetActive(true);
        }
    }
    void OnButtonDown(int i)
    {
        ids = weaponCP.ReturnWeaponIDs();
        weaponCP.SetWeapon(ids[i]);
        onButtonDown.Invoke();
    }
}
