using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Pool
{
    public GameObject prefab;
    public int size;
    Queue<GameObject> queue = new Queue<GameObject>();
    Transform transParent;
    void Create()
    {
        GameObject a = GameObject.Instantiate(prefab);
        a.transform.SetParent(transParent);
        a.SetActive(false);
        a.name = prefab.name;
        queue.Enqueue(a);
    }
    public void Initialize(Transform parent)
    {
        transParent = parent;
        for (int i = 0; i < size; i++)
        {
            Create();
        }
    }
    public GameObject GetFromPool()
    {
        GameObject a;
        if (queue.Count <= 0)
        {
            Create();
        }
        a = queue.Dequeue();
        return a;
    }
    public void BackToPool(GameObject a)
    {
        a.SetActive(false);

        /*if (queue.Contains(a))
        {
            Debug.LogError(a.transform.position);
        }*/
        queue.Enqueue(a);
    }
}