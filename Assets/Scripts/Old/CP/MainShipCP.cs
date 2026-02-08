using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EngineLevel { Low,Middle,High};

public class MainShipCP : MainOfMain
{
    MovementCP movementCP;
    AttackCP attackCP;
    HealthCP healthCP;
    WeaponCP weaponCP;
    //
    [SerializeField] EngineLevel engineLevel;
    //
    bool isStuttererReduceSpeed;
    bool isAttack;

    protected void Awake()
    {
        movementCP = GetComponent<MovementCP>();
        healthCP = GetComponent<HealthCP>();
    }
    private void Start()
    {
        healthCP.onHealthOver += Die;
        //
        weaponCP = GetComponent<WeaponCP>();
        weaponCP.onChangeWeapon += OnChangeWeapon;
        //
        attackCP = GetComponent<AttackCP>();
        attackCP.onAttack += OnAttack;
    }
    void OnChangeWeapon(int a,int b)
    {
        movementCP.ReduceSpeed(weaponCP.ReturnMassLevel(), engineLevel, healthCP.ReturnArmorLevel());
        isStuttererReduceSpeed = weaponCP.ReturnIfStutterer();
    }
    public void Initialize(Vector3 pos, Vector3 vector, string tag, float additionalHP)
    {
        transform.position = pos;
        movementCP.SetRotation(vector);
        this.tag = tag;
        SetLeague(tag);
        healthCP.AdditionalHP = additionalHP;
    }
    void OnAttack(bool isAttaking)
    {
        if (isAttaking != isAttack) 
        {
            if (isStuttererReduceSpeed)
            {
                isAttack = isAttaking;
                switch (isAttaking)
                {
                    case true:
                        movementCP.ReduceSpeed(0.3f);
                        break;
                    case false:
                        movementCP.ReduceSpeed(-0.3f);
                        break;
                }
            }  
        }
    }
    public EngineLevel ReturnEngineLevel()
    {
        return engineLevel;
    }

    protected override void Die()
    {
        base.Die(); 
    }
}
