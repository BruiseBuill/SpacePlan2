using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TeachingButton : MonoBehaviour
{
    Button button;
    private void Awake()
    {
        button = GetComponent<Button>();
    }
    private void OnEnable()
    {
        button.onClick.AddListener(GameManager.Instance().StartTeaching);
    }
    private void OnDisable()
    {
        button.onClick.RemoveAllListeners();
    }
}
