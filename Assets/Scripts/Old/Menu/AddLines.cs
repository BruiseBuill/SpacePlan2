using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddLines : MonoBehaviour
{
    [SerializeField] GameObject line;
    [SerializeField] Transform lineParent;
    [SerializeField] int maxPlayerSum;
    //


    public void AddLine()
    {
        if(lineParent.childCount < maxPlayerSum)
        {
            GameObject a = Instantiate(line, lineParent);
        }
    }
    public void ReduceLine()
    {
        if (lineParent.childCount > 1)
        {
            Destroy(lineParent.GetChild(lineParent.childCount - 1).gameObject);
        }
    }

}
