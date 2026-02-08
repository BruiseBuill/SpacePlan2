using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BaseBuilding : MonoBehaviour
{
    public UnityAction onBaseExplose = delegate { };
    //
    private void Start()
    {
        GetComponent<MainOfMain>().onDie += (GameObject a) => { onBaseExplose.Invoke(); };
    }

}
