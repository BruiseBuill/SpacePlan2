using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RandomEvent : MonoBehaviour
{
    BuffEvent buffEvent;
    [SerializeField] AnimationClip openClip;
    [SerializeField] AnimationClip closeClip;
    [SerializeField] Animator animator;
    //
    [SerializeField] float breakTime = 28f;
    WaitForSeconds wait_nextRandomEvent;
    int surplusCount;
    bool isOpen;
    //
    Dictionary<string, string> dic = new Dictionary<string, string>();
    [SerializeField] List<int> remainOption = new List<int>();
    //
    [SerializeField] QuickButton[] buttons;
    [SerializeField] Text[] texts;
    int[] optionID;
    int pos;

    private void Awake()
    {
        surplusCount = 0;
        isOpen = false;
        //
        var asd= (Resources.Load("Info") as TextAsset).text.Split('\n');
        for (int i = 0; i < asd.Length; i++)
        {
            var a = asd[i].Split(':');
            dic.Add(a[0], a[1]);
        }
        
        optionID = new int[buttons.Length];
        //
        wait_nextRandomEvent = new WaitForSeconds(breakTime);
    }
    
    private void InitializeOptions()
    {
        remainOption.Add(0);
        remainOption.Add(0);
        remainOption.Add(1);
        remainOption.Add(3);
        remainOption.Add(5);
    }
    private void OnEnable()
    {
        InitializeOptions();
    }
    private void Start()
    {
        buffEvent = GetComponent<BuffEvent>();
        for(int i = 0; i < buttons.Length; i++)
        {
            buttons[i].onClickWithIndex += OnChooseEvent;
        }
        StartCoroutine("WaitRandomEvent");
    }
    IEnumerator WaitRandomEvent()
    {
        ShowRandomEvent();
        yield return wait_nextRandomEvent;
        yield return StartCoroutine("WaitRandomEvent");
    }
    void ShowRandomEvent()
    {
        surplusCount++;
        if (!isOpen)
        {
            isOpen = true;
            animator.SetBool("isOpen", true);
            RefreshRandomEvent();
        }
    }
    void OnChooseEvent(int index)
    {
        if (isOpen && optionID[index] != -1) 
        {
            surplusCount--;
            buffEvent.ChooseBuff(optionID[index]);
            AddEvent(optionID[index]);
            for (int i = 0; i < buttons.Length; i++)
            {
                if (i != index && optionID[i] != -1)
                    remainOption.Add(optionID[i]);
            }
            //
            if (remainOption.Count == 0)
            {
                StopCoroutine("WaitRandomEvent");
            }
            //
            if (surplusCount > 0 && remainOption.Count > 0)
            {
                RefreshRandomEvent();
            }
            else
            {
                isOpen = false;
                animator.SetBool("isOpen", false);
            }
        }
    }
    void RefreshRandomEvent()
    {
        for (int i = 0; i < buttons.Length; i++) 
        {
            if (remainOption.Count == 0)
            {
                optionID[i] = -1;
                texts[i].text = null;
            }
            else
            {
                pos = Random.Range(0, remainOption.Count);
                optionID[i] = remainOption[pos];
                remainOption.RemoveAt(pos);
                texts[i].text = dic[optionID[i].ToString()];
            }
        }
    }
    void AddEvent(int id)
    {
        switch (id)
        {
            case 1:
                remainOption.Add(2);
                remainOption.Add(6);
                remainOption.Add(7);
                remainOption.Add(8);
                remainOption.Add(9);
                remainOption.Add(10);
                break;
            case 2:
                remainOption.Add(0);
                remainOption.Add(11);
                remainOption.Add(12);
                remainOption.Add(13);
                break;
            case 3:
                remainOption.Add(0);
                remainOption.Add(0);
                remainOption.Add(4);
                break;
            case 4:
                remainOption.Add(0);
                remainOption.Add(13);
                break;
            case 6:
            case 7:
            case 8:
            case 9:
                remainOption.Add(0);
                break;
            case 10:
                remainOption.Add(13);
                break;
            case 11:
                remainOption.Add(13);
                break;
            case 12:
                remainOption.Add(13);
                break;
        }
    }
}
