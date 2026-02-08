using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponList : MonoBehaviour
{
    [SerializeField] bool isShowing;
    //
    [SerializeField] Sprite waitOpenSprite;
    [SerializeField] Sprite waitCloseSprite;
    [SerializeField] GameObject scroll;
    Button button;
    Image image;
    //
    LoadWeaponImage loadWeaponImage;

    private void Awake()
    {
        button = GetComponent<Button>();
        image = GetComponent<Image>();
        button.onClick.AddListener(OnChangeShowing);
    }
    private void Start()
    {
        loadWeaponImage = transform.GetComponentInChildren<LoadWeaponImage>();
        loadWeaponImage.onButtonDown += OnChangeShowing;
        //
        scroll.SetActive(false);
    }
    void OnChangeShowing()
    {
        isShowing = !isShowing;
        switch (isShowing)
        {
            case true:
                image.sprite = waitCloseSprite;
                scroll.SetActive(isShowing);
                break;
            case false:
                image.sprite = waitOpenSprite;
                scroll.SetActive(isShowing);
                break;
        }
    }
    public void OnChangeShowing(bool a)
    {
        if (scroll)
        {
            isShowing = a;
            switch (isShowing)
            {
                case true:
                    image.sprite = waitCloseSprite;
                    scroll.SetActive(isShowing);
                    break;
                case false:
                    image.sprite = waitOpenSprite;
                    scroll.SetActive(isShowing);
                    break;
            }
        }
        
    }
    public LoadWeaponImage ReturnLoadWeaponImage()
    {
        return loadWeaponImage;
    }
}
