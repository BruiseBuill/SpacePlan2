using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BF;

public enum BulletKind { Rifle,Shrapnel,Missile,Sniper,Stutterer,Grenade};
public class MainBulletCP : MainOfMain
{
    MovementCP movement;
    Vector3 moveVector;
    AutoDie autoDie;
    //
    [SerializeField] protected BulletKind bulletKind;
    bool isAlive;
    //
    protected Transform parent;
    [SerializeField] GameObject aimEX;
    //
    [SerializeField] protected float sameTeamDamageMultiple;
    [SerializeField] protected float damage;
    //
    [SerializeField] bool isAreaEffect;
    [SerializeField] float areaEffectRadius;
    //
    [SerializeField] bool hasMatchlessTime;
    [SerializeField] float matchlessTime;
    Collider2D collide;
    WaitForSeconds wait_Matchless;
    //
    protected LayerMask layer;

    protected virtual void Awake()
    {
        movement = GetComponent<MovementCP>();
        if (hasMatchlessTime)
        {
            collide = GetComponent<Collider2D>();
            wait_Matchless = new WaitForSeconds(matchlessTime);
        }
        layer = LayerMask.GetMask("Ship", "Building");
    }
    private void Start()
    {
        autoDie = GetComponent<AutoDie>();
        autoDie.onLifeTimeOver += Die;
    }
    protected override void OnEnable()
    {
        isAlive = true;
        //
        if (hasMatchlessTime)
        {
            collide.enabled = false;
            StartCoroutine("WaitMatchlessTime");
        }
        movement.Move(moveVector);
        movement.SetRotation(moveVector);
    }
    public void Initialize(Vector3 pos,Vector3 vector,Transform parent,string tag)
    {
        transform.position = pos;
        moveVector = vector;
        this.parent = parent;
        this.tag = tag;
    }
    public Transform ReturnParent()
    {
        return parent;
    }
    void OnTriggerEnter2D(Collider2D collider)
    {
        Die();
        if (!isAreaEffect&& collider.TryGetComponent<HealthCP>(out HealthCP health))
        {
            if (collider.CompareTag(tag))
            {
                health.BeAttacked(damage * sameTeamDamageMultiple, bulletKind);
            }
            else
            {
                if (health.BeAttacked(damage, parent, bulletKind) && parent) 
                {
                    parent.GetComponent<MainOfMain>().Kill(health.GetComponent<MainOfMain>().ReturnObjectKind());
                }
            }
            GameObject a = PoolManager.Instance().Release(aimEX.name);
            a.transform.SetPositionAndRotation(transform.position, movement.ReturnRotation());
            a.SetActive(true);
        }
    }
    IEnumerator WaitMatchlessTime()
    {
        yield return wait_Matchless;
        collide.enabled = true;
    }
    protected override void Die()
    {
        if (isAlive)
        {
            isAlive = false;
            if (isAreaEffect)
            {
                var a = Physics2D.OverlapCircleAll(transform.position, areaEffectRadius, layer);
                for (int i = 0; i < a.Length; i++)
                {
                    if (a[i].TryGetComponent<HealthCP>(out HealthCP health))
                    {
                        if (a[i].CompareTag(tag))
                            health.BeAttacked(damage * sameTeamDamageMultiple, bulletKind);
                        else
                        {
                            if (health.BeAttacked(damage, parent, bulletKind) && parent)
                            {
                                parent.GetComponent<MainOfMain>().Kill(health.GetComponent<MainOfMain>().ReturnObjectKind());
                            }
                        }
                    }
                }
                GameObject b = PoolManager.Instance().Release(aimEX.name);
                b.transform.SetPositionAndRotation(transform.position, movement.ReturnRotation());
                b.SetActive(true);
            }
            base.Die();
        }
    }
}
