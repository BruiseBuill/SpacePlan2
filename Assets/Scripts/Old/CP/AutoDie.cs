using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using BF;

public class AutoDie : MonoBehaviour
{
    [SerializeField] bool isReturnToPool;
    [SerializeField] bool isSetActiveFalse;
    //
    [SerializeField] float lifeTime;
    WaitForSeconds wait_LifeTime;
    //
    [SerializeField] bool isRandomLifeTime;
    [SerializeField] float maxLifeTime;
    [SerializeField] float minLifeTime;
    public UnityAction onLifeTimeOver = delegate { };
    private void Awake()
    {
        wait_LifeTime = new WaitForSeconds(lifeTime);
    }
    private void OnEnable()
    {
        if (isRandomLifeTime)
        {
            wait_LifeTime = new WaitForSeconds(Random.Range(minLifeTime, maxLifeTime));
        }
        StartCoroutine("WaitDie");
    }
    IEnumerator WaitDie()
    {
        yield return wait_LifeTime;
        onLifeTimeOver.Invoke();
        if (isReturnToPool)
            PoolManager.Instance().Recycle(gameObject);
        if (isSetActiveFalse)
            gameObject.SetActive(false);
    }
}
