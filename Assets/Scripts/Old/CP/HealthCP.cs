using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Reflection;

public enum ArmorLevel{None,Light,Heavy,Building}

public class HealthCP : MonoBehaviour
{
    public UnityAction<Transform> onBeAttacked = delegate { };
    public UnityAction onHealthOver = delegate { };
    //
    [SerializeField] float HP;
    [SerializeField] float maxHP;
    float additionalHP;
    public float AdditionalHP
    {
        set { additionalHP = value; }
    }
    [SerializeField] ArmorLevel armorLevel;
    float armorDamageMultiple;
    //
    [SerializeField] bool canAutoRepair;
    [SerializeField] float autoRepairBreak;
    WaitForSeconds wait_autoRepair;
    [SerializeField] float autoRepairSum;
    //
    [Header("建筑物的额外免伤")]
    [SerializeField] bool isBuilding;
    [SerializeField] float freeConditionDamageMultiple;
    //
    [SerializeField] Image healthWeb;
    //
    

    private void Awake()
    {
        switch (armorLevel)
        {
            case ArmorLevel.None:
                armorDamageMultiple = 1f;
                break;
            case ArmorLevel.Light:
                armorDamageMultiple = 0.4f;
                break;
            case ArmorLevel.Heavy:
                armorDamageMultiple = 0.1f;
                break;
            case ArmorLevel.Building:
                armorDamageMultiple = 0.2f;
                break;
        }
        wait_autoRepair = new WaitForSeconds(autoRepairBreak);
    }
    private void OnEnable()
    {
        HP = maxHP + additionalHP;

        if (canAutoRepair)
        {
            StartCoroutine("AutoRepair");
        }
    }
    public void BeAttacked(float damage, BulletKind bulletKind)
    {
        if (HP > 0)
        {
            BulletAndArmor(bulletKind, ref damage);
            //
            HP -= damage * armorDamageMultiple;
            if (HP <= 0)
            {
                onHealthOver.Invoke();
            }
        }
    }
    public bool BeAttacked(float damage, Transform attacker, BulletKind bulletKind)
    {
        if (HP > 0)
        {
            BulletAndArmor(bulletKind, ref damage);
            //
            HP -= damage * armorDamageMultiple;
            onBeAttacked.Invoke(attacker);
            if (HP <= 0)
            {
                onHealthOver.Invoke();
                return true;
            }
            return false;
        }
        return false;
    }
    void BulletAndArmor(BulletKind bulletKind,ref float damage)
    {
        switch (bulletKind)
        {
            case BulletKind.Sniper:
                switch (armorLevel)
                {
                    case ArmorLevel.Light:
                        damage *= 1.45f;
                        break;
                    case ArmorLevel.Heavy:
                        damage *= 3.7f;
                        break;
                }
                break;
            case BulletKind.Shrapnel:
                if (isBuilding)
                {
                    damage *= 1.8f;
                }
                break;
            case BulletKind.Missile:
                switch (armorLevel)
                {
                    case ArmorLevel.Heavy:
                        damage *= 2.8f;
                        break;
                    case ArmorLevel.Light:
                        damage *= 1.3f;
                        break;
                }
                break;
            case BulletKind.Stutterer:
                switch (armorLevel)
                {
                    case ArmorLevel.None:
                        damage *= 2f;
                        break;
                    case ArmorLevel.Light:
                        damage *= 1.1f;
                        break;
                }
                break;
        }
        //
        if (isBuilding && GetComponent<AIControl>().ReturnAICondition() == AICondition.Free)
        {
            damage *= freeConditionDamageMultiple;
        }
    }
    IEnumerator AutoRepair()
    {
        if (HP > 0)
        {
            HP = Mathf.Min(HP + autoRepairSum, maxHP + additionalHP);
            
        }
        yield return wait_autoRepair;
        yield return StartCoroutine("AutoRepair");
    }
    public float ReturnHealthPoint()
    {
        return HP / armorDamageMultiple;
    }
    public float ReturnHealthPercent()
    {
        return HP / maxHP;
    }
    public ArmorLevel ReturnArmorLevel()
    {
        return armorLevel;
    }
}
