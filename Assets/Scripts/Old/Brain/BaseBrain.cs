using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseBrain : MonoBehaviour
{
    protected CreateShipCP createShipCP;
    protected PlantBDCP plantBDCP;
    protected BuffEvent buffEvent;
    //
    protected int enemySum;
    protected int allySum;

    private void Start()
    {
        plantBDCP = GetComponent<PlantBDCP>();
        buffEvent = GetComponent<BuffEvent>();
        createShipCP = GetComponent<CreateShipCP>();
        SetBaseBD();
        Initialize();
    }
    protected abstract void SetBaseBD();
    protected abstract void Initialize();
    public void SetEnemySumAndAllySum(int enemySum, int allySum)
    {
        this.enemySum = enemySum;
        this.allySum = allySum;
    }
}
