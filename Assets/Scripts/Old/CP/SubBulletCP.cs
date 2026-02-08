using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BF;

public class SubBulletCP : MonoBehaviour
{
    MovementCP movement;
    //
    [SerializeField] GameObject subBullet;
    [SerializeField] int subBulletSum;
    [SerializeField] bool isRandomShooting;
    [SerializeField] float subBulletRecoil;
    //
    Transform parent;
    //
    Vector3 normal;
    Vector3 recoilDirection;
    Vector3 currentDirection;


    private void Awake()
    {
        movement = GetComponent<MovementCP>();
        GetComponent<MainOfMain>().onDie += CreateSubBullet;
    }
    private void OnEnable()
    {
        parent = GetComponent<MainBulletCP>().ReturnParent();
    }
    void CreateSubBullet(GameObject b)
    {
        if (isRandomShooting)
        {
            for (int i = 0; i < subBulletSum; i++)
            {
                GameObject a = PoolManager.Instance().Release(subBullet.name);
                a.GetComponent<MainBulletCP>().Initialize(transform.position, new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f).normalized, parent, tag);
                a.SetActive(true);
            }
        }
        else
        {
            normal = movement.ReturnModelUp();
            recoilDirection = Vector3.Cross(normal, Vector3.forward);
            currentDirection = normal + recoilDirection * (-subBulletRecoil);
            for (int i = 0; i < subBulletSum; i++)
            {
                GameObject a = PoolManager.Instance().Release(subBullet.name);
                a.GetComponent<MainBulletCP>().Initialize(transform.position, currentDirection, parent, tag);
                a.SetActive(true);
                currentDirection += 2f / (subBulletSum - 1) * subBulletRecoil * recoilDirection;
            }
        }
        
    }
}
