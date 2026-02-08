using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectricBullet : MainBulletCP
{
    //
    [SerializeField] float radius;
    //
    Transform attackAim;
    [SerializeField] float attackBreakTime;
    WaitForSeconds wait_Attack;
    //LayerMask layer;
    //
    List<Vector3> list = new List<Vector3>();
    //
    LineRenderer line;
    [SerializeField] float minLineLength;
    [SerializeField] float maxLineLength;
    [SerializeField] float deviation;
    //
    [SerializeField] float drawTime;

    protected override void Awake()
    {
        base.Awake();
        wait_Attack = new WaitForSeconds(attackBreakTime);
        line = GetComponent<LineRenderer>();
    }
    new private void OnEnable()
    {
        StartCoroutine("Wait");
    }
    IEnumerator Wait()
    {
        while (true)
        {
            yield return wait_Attack;
            Attack();
        }
    }
    void Attack()
    {
        var a = Physics2D.OverlapCircle(transform.position, radius, layer);
        if (a && a.TryGetComponent<HealthCP>(out HealthCP health))
        {
            if (a.CompareTag(tag))
                health.BeAttacked(damage * sameTeamDamageMultiple, bulletKind);
            else
            {
                if (health.BeAttacked(damage, parent, bulletKind) && parent)
                {
                    parent.GetComponent<MainOfMain>().Kill(health.GetComponent<MainOfMain>().ReturnObjectKind());
                }
            }
            attackAim = a.transform;
            StartCoroutine("ContinueDrawing");
        }
    }
    IEnumerator ContinueDrawing()
    {
        float startTime = Time.time;
        while (Time.time - startTime < drawTime)
        {
            list.Clear();
            DrawLine(transform.position, attackAim.transform.position);
            yield return null;
        }
        line.positionCount = 0;
        line.enabled = false;
    }
    void DrawLine(Vector3 start, Vector3 end)
    {
        list.Add(start);
        float msLength = Random.Range(minLineLength, maxLineLength);
        float sqrLength = msLength * msLength;
        Vector3 now = start;
        while ((now - end).sqrMagnitude >= sqrLength)
        {
            now += (end - now).normalized * msLength;
            now += RandomOffect();
            list.Add(now);
        }
        list.Add(end);
        Vector3[] postions = list.ToArray();
        line.positionCount = postions.Length;
        line.SetPositions(postions);
        line.enabled = true;
    }
    Vector3 RandomOffect()
    {
        return new Vector3(Random.Range(-deviation, deviation),
         Random.Range(-deviation, deviation), 0);
    }
    private void OnDisable()
    {
        line.positionCount = 0;
        line.enabled = false;
    }
}
