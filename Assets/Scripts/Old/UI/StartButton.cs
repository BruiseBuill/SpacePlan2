using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartButton : MonoBehaviour
{
    Button button;
    private void Awake()
    {
        button = GetComponent<Button>();
    }
    private void OnEnable()
    {
        button.onClick.AddListener(GameManager.Instance().StartGame);
    }
    private void OnDisable()
    {
        button.onClick.RemoveAllListeners();
    }
}
