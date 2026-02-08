using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using BF;

public enum AttackCondition { Free,Before,After,Reload}

public class AttackCP : MonoBehaviour
{
    public UnityAction onBulletExhaust = delegate { };
    public UnityAction onShooting = delegate { };
    public UnityAction<float> onReloading = delegate { };
    public UnityAction<bool> onAttack = delegate { };
    [SerializeField] AttackCondition attackCondition;
    //
    [SerializeField] Transform singleWeapon;
    [SerializeField] bool canDoubleAttack;
    bool isDoubleAttack;
    [SerializeField] Transform[] doubleWeapon;
    //
    [SerializeField] Weapon weapon;
    [SerializeField] int surplusBulletSumInClip;
    [SerializeField] int surplusBulletSumOutsideClip;
    //
    GameObject attackFlashEX;
    //
    [SerializeField] Transform raycastPoint;
    float attackRange;
    LayerMask layers;
    //
    bool needAttack;
    WaitForSeconds wait_coolBreak;
    WaitForSeconds wait_warmBreak;
    float reloadProgress;
    //
    Vector3 attackDirection;

    private void Awake()
    {   
        layers = LayerMask.GetMask("Ship", "Building");
    }
    private void Start()
    {
        GetComponent<WeaponCP>().onChangeWeapon += SetWeapon;
    }
    private void OnEnable()
    {
        attackCondition = AttackCondition.Reload;
    }
    public void SetWeapon(int weaponID, int bulletSum)
    {
        switch (attackCondition)
        {
            case AttackCondition.Free:
                break;
            case AttackCondition.Before:
                StopCoroutine("BeforeAttack");
                break;
            case AttackCondition.After:
                StopCoroutine("AfterAttack");
                break;
            case AttackCondition.Reload:
                StopCoroutine("Reloading");
                break;
        }
        weapon = WeaponManager.Instance().ReturnWeapon(weaponID);
        isDoubleAttack = canDoubleAttack && weapon.massLevel < MassLevel.Large;
        surplusBulletSumInClip = 0;
        surplusBulletSumOutsideClip = bulletSum;
        attackFlashEX = weapon.attackFlashEX;
        attackRange = weapon.attackRangeOfBullet;
        wait_warmBreak = new WaitForSeconds(weapon.warmBreak);
        wait_coolBreak = new WaitForSeconds(weapon.coolBreak);
        StartCoroutine("Reloading");
    }
    public int ReturnSurplusBulletSumTotal()
    {
        return surplusBulletSumInClip + surplusBulletSumOutsideClip;
    }
    public bool Attack(Vector3 attackDirection)
    {
        this.attackDirection = attackDirection;
        needAttack = true;
        switch (attackCondition)
        {
            case AttackCondition.Free:
                StartCoroutine("BeforeAttack");
                return true;
            case AttackCondition.Before:
                return true;
            case AttackCondition.After:
                return true;
            case AttackCondition.Reload:
                return false;
        }
        return false;
    }
    public void StopAttack()
    {
        needAttack = false;
        switch (attackCondition) 
        {
            case AttackCondition.Free:
                break;
            case AttackCondition.Before:
                onAttack.Invoke(false);
                StopCoroutine("BeforeAttack");
                attackCondition = AttackCondition.Free;
                break;
        }
    }
    IEnumerator BeforeAttack()
    {
        attackCondition = AttackCondition.Before;
        onAttack.Invoke(true);
        yield return wait_warmBreak;
        while (!Raycast(attackDirection))
        {
            yield return null;
        }
        Attacking();
    }
    void Attacking()
    {
        attackDirection += Vector3.Cross(attackDirection, Vector3.forward) * Random.Range(-weapon.recoil, weapon.recoil);
        if (isDoubleAttack)
        {
            CreateBulletAndAttackEX(doubleWeapon[0].position);
            CreateBulletAndAttackEX(doubleWeapon[1].position);
            surplusBulletSumInClip -= 2;
        }
        else
        {
            CreateBulletAndAttackEX(singleWeapon.position);
            surplusBulletSumInClip--;
        }
        onShooting.Invoke();
        StartCoroutine("AfterAttack");
    }
    bool Raycast(Vector3 attackDirection)
    {
        RaycastHit2D hit = Physics2D.Raycast(raycastPoint.position, attackDirection, attackRange, layers);
        if (hit)
        {
            return !hit.collider.CompareTag(tag);
        }
        return true;
    }
    public Transform ReturnRaycastTransform(Vector3 attackDirection)
    {
        RaycastHit2D hit = Physics2D.Raycast(raycastPoint.position, attackDirection, attackRange, layers);
        if (hit)
        {
            return hit.transform;
        }
        return null;
    }
    void CreateBulletAndAttackEX(Vector3 pos)
    {
        GameObject a = PoolManager.Instance().Release(weapon.bullet.name);
        a.GetComponent<MainBulletCP>().Initialize(pos, attackDirection, transform, tag);
        a.SetActive(true);
        GameObject c = PoolManager.Instance().Release(attackFlashEX.name);
        c.transform.position = pos;
        c.transform.up = attackDirection;
        c.SetActive(true);
    }
    IEnumerator AfterAttack()
    {
        attackCondition = AttackCondition.After;
        yield return wait_coolBreak;
        AttackCheck();
    }
    void AttackCheck()
    {
        if (surplusBulletSumInClip == 0)
        {
            onAttack.Invoke(false);
            Reload();
            return;
        }
        if (!needAttack)
        {
            onAttack.Invoke(false);
            attackCondition = AttackCondition.Free;
        }
        else
        {
            attackCondition = AttackCondition.Before;
            StartCoroutine("BeforeAttack");
        }
    }
    public void Reload()
    {
        if (attackCondition != AttackCondition.Reload) 
        {
            StartCoroutine("Reloading");
        } 
    }
    IEnumerator Reloading()
    {
        attackCondition = AttackCondition.Reload;
        surplusBulletSumOutsideClip += surplusBulletSumInClip;
        if (surplusBulletSumOutsideClip > 0)
        {
            surplusBulletSumInClip = Mathf.Min(surplusBulletSumOutsideClip, weapon.bulletSumPerClip);
            surplusBulletSumOutsideClip = surplusBulletSumOutsideClip - surplusBulletSumInClip;
            float startTime = Time.time;
            while (Time.time - startTime < weapon.reloadTime)
            {
                reloadProgress = (Time.time - startTime) / weapon.reloadTime;
                yield return null;
                onReloading.Invoke(reloadProgress);
            }
            onReloading.Invoke((float)surplusBulletSumInClip / weapon.bulletSumPerClip);
            attackCondition = AttackCondition.Free;
        }
        else
        {
            if (weapon.hasInfiniteBullet)
            {
                surplusBulletSumOutsideClip = weapon.initialBulletSum;
                yield return StartCoroutine("Reloading");
            }
            else
            {
                onBulletExhaust.Invoke();
                yield return null;
            }
        } 
    }
    public void ReturnBulletDetail(ref int spBInClip, ref int spBOutClip,ref int BperClip)
    {
        spBInClip = surplusBulletSumInClip;
        spBOutClip = surplusBulletSumOutsideClip;
        BperClip = weapon.bulletSumPerClip;
    }
    public float ReturnSqrAttackRange()
    {
        return attackRange * attackRange;
    }
    public float ReturnAttackRange()
    {
        return attackRange ;
    }
    public float ReturnBulletSpeed()
    {
        return weapon.bulletSpeed;
    }
    public Sprite ReturnIcon()
    {
        return weapon.sprite;
    }
}
