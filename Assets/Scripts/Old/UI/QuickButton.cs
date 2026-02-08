using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class QuickButton : MonoBehaviour,IPointerDownHandler,IPointerClickHandler
{
    [SerializeField] int info;
    public UnityAction onButtonDown = delegate { };
    public UnityAction onClick = delegate { };
    public UnityAction<int> onClickWithIndex = delegate { };

    public virtual void OnPointerDown(PointerEventData eventData)
    {
        onButtonDown.Invoke();
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        onClick.Invoke();
        onClickWithIndex.Invoke(info);
    }
    public void Initialize()
    {
        onClickWithIndex.Invoke(info);
    }
    protected void SetInfo(int i)
    {
        info = i;
    }
}
