using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainBuilding : MainOfMain
{
    HealthCP healthCP;
    MovementCP movementCP;
    //

    protected void Awake()
    {
        movementCP = GetComponent<MovementCP>();
        healthCP = GetComponent<HealthCP>();
    }
    private void Start()
    {
        
        healthCP.onHealthOver += Die;
    }
    public void Initialize(Vector3 pos, Vector3 vector, string tag, float additionalHP)
    {
        transform.position = pos;
        movementCP.SetRotation(vector);
        this.tag = tag;
        SetLeague(tag);
        healthCP.AdditionalHP = additionalHP;
    }
    protected override void Die()
    {
        base.Die();
    }
}
