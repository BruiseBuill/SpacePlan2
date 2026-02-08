using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using BF;

public class MainOfMain : MonoBehaviour
{
    public UnityAction<GameObject> onDie = delegate { };
    public UnityAction<ObjectKind> onKill = delegate { };
    //
    [SerializeField] protected ObjectKind objectKind;
    protected int league;
    //
    protected int killSum;

    protected virtual void OnEnable()
    {
        killSum = 0;
    }
    protected void SetLeague(string tag)
    {
        if (tag == "League0")
        {
            league = 0;
        }
        else if (tag == "League1")
        {
            league = 1;
        }
    }
    public void Kill(ObjectKind kind)
    {
        killSum++;
        onKill.Invoke(kind);
    }
    public string ReturnLeague()
    {
        return tag;
    }
    public int ReturnKillSum()
    {
        return killSum;
    }
    public ObjectKind ReturnObjectKind()
    {
        return objectKind;
    }
    protected virtual void Die()
    {
        onDie.Invoke(gameObject);
        PoolManager.Instance().Recycle(gameObject);
    }
    
}
