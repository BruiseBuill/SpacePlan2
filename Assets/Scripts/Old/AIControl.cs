using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum AICondition { Free, Attack };

public class AIControl : MonoBehaviour
{
    MovementCP movementCP;
    HealthCP healthCP;
    AttackCP attackCP;
    MainOfMain mainOfMain;
    //
    [SerializeField] AICondition aICondition;
    //
    [SerializeField] Transform attackTarget;
    [SerializeField] float viewRange;
    //
    [SerializeField] GameObject canvas;
    [SerializeField] Image healthStickImage;
    [SerializeField] bool isShowhealthStick;
    [SerializeField] float showHealthStickTime;
    WaitForSeconds wait_ShowHealthStick;
    //
    [SerializeField] Transform attacker;
    //
    [SerializeField] bool canMove;
    [SerializeField] bool hasInitializedFreeMove;
    [SerializeField] List<Vector3> freeMovePositions=new List<Vector3>();
    LayerMask layers;
    //
    List<AIControl> allyList = new List<AIControl>();
    List<GameObject> enemyList = new List<GameObject>();
    //
    [SerializeField] float sleepThinkBreakTime;
    WaitForSeconds wait_awakeThinkBreak;
    WaitForSeconds wait_sleepThinkBreak;
    WaitForSeconds wait_ThinkBreak;
    [SerializeField] int count;
    [SerializeField] int maxAwakeSum;

    private void Awake()
    {
        movementCP = GetComponent<MovementCP>();
        healthCP = GetComponent<HealthCP>();
        attackCP = GetComponent<AttackCP>();
        mainOfMain = GetComponent<MainOfMain>();
        layers = LayerMask.GetMask("Ship", "Building");
        //
        wait_ShowHealthStick = new WaitForSeconds(showHealthStickTime);
        wait_awakeThinkBreak = null;
        wait_sleepThinkBreak = new WaitForSeconds(sleepThinkBreakTime);
    }
    private void OnEnable()
    {
        healthCP.onBeAttacked += BeAttack;
        healthCP.onBeAttacked += RefreshHealthStick;
        //
        aICondition = AICondition.Free;
        attackTarget = null;
        hasInitializedFreeMove = false;
        attacker = null;
        count = 0;
        wait_ThinkBreak = wait_awakeThinkBreak;
        //
        isShowhealthStick = true;
        healthStickImage.color = CompareTag(GameManager.Instance().PlayerLeague) ? Color.blue : Color.red;
        RefreshHealthStick(null);
        isShowhealthStick = false;
        //
        canvas.transform.Find("PlayerOnly").gameObject.SetActive(false);
        StartCoroutine("WaitAfterOnEnable");
    }
    IEnumerator WaitAfterOnEnable()
    {
        yield return null;
        yield return StartCoroutine("Think");
    }
    IEnumerator Think()
    {
        yield return wait_ThinkBreak;
        if (this.enabled)
        {
            switch (aICondition)
            {
                case AICondition.Attack:
                    if (AttackCheck())
                    {
                        AttackMove();
                    }
                    else
                    {
                        hasInitializedFreeMove = false;
                    }
                    break;
                case AICondition.Free:
                    if (canMove)
                        FreeMove();
                    CheckView();
                    break;
            }
            yield return StartCoroutine("Think");
        }
    }
    bool AttackCheck()
    {
        if (attackTarget && attackTarget.gameObject.activeSelf && (attackTarget.position - transform.position).sqrMagnitude < attackCP.ReturnSqrAttackRange()) 
        {
            if (Vector3.Dot(movementCP.ReturnModelUp(), attackTarget.position - transform.position) > 0)
                attackCP.Attack(movementCP.ReturnModelUp());
            return true;
        }
        else
        {
            attackTarget = null;
            aICondition = AICondition.Free;
            attackCP.StopAttack();
            return false;
        }
    }
    void CheckView()
    {
        count++;
        if (count > maxAwakeSum)
        {
            wait_ThinkBreak = wait_sleepThinkBreak;
        }
        allyList.Clear();
        enemyList.Clear();
        var a = Physics2D.OverlapCircleAll(transform.position, viewRange, layers);
        int i = 0;
        for (; i < a.Length; i++)
        {
            if (a[i].CompareTag(tag)) 
            {
                allyList.Add(a[i].gameObject.GetComponent<AIControl>());
            }
            else
            {
                enemyList.Add(a[i].gameObject);
            }
        }
        int j = 0;
        for (i = 0; i < allyList.Count; i++) 
        {
            if (attacker && allyList[i].SetAttackTarget(attacker)) 
            {
                continue;
            }
            for (j = 0; j < enemyList.Count; j++)
            {
                if (allyList[i].SetAttackTarget(enemyList[j].transform))
                { 
                     break;
                }
            }
        }
        attacker = null;
    }
    void AttackMove()
    {
        Vector3 b = Vector3.Cross(movementCP.ReturnModelUp(), Vector3.forward);
        if (Vector3.Dot(b, movementCP.ReturnMoveVector()) > 0)
        {
            movementCP.Move(b);
        }
        else
        {
            movementCP.Move(-b);
        }
        //
        MovementCP a = attackTarget.GetComponent<MovementCP>();
        movementCP.Rotate(attackTarget.position - transform.position + (attackTarget.position - transform.position).magnitude / attackCP.ReturnBulletSpeed() * a.ReturnMoveSpeed() * a.ReturnMoveVector());
    }
    void FreeMove()
    {
        if (!hasInitializedFreeMove) 
        {
            hasInitializedFreeMove = true;
            freeMovePositions = MapManager.Instance().ReturnMoveTarget(transform.position, mainOfMain.ReturnLeague());
        }
        else
        {
            movementCP.Move((freeMovePositions[0] - transform.position).normalized);
            movementCP.Rotate(freeMovePositions[0] - transform.position);
            if ((freeMovePositions[0] - transform.position).sqrMagnitude < 1)
            {
                if (freeMovePositions.Count == 1)
                {
                    hasInitializedFreeMove = false;
                }
                else
                {
                    freeMovePositions.RemoveAt(0);
                }
            }
        }
    }
    void BeAttack(Transform attacker)
    {
        this.attacker = attacker;
    }
    public bool SetAttackTarget(Transform target)
    {
        switch (aICondition)
        {
            case AICondition.Attack:
                return false;
            case AICondition.Free:
                if(target && target.gameObject.activeSelf && (target.position - transform.position).sqrMagnitude < attackCP.ReturnSqrAttackRange())
                {
                    count = 0;
                    wait_ThinkBreak = wait_awakeThinkBreak;
                    attackTarget = target;
                    aICondition = AICondition.Attack;
                    return true;
                }
                return false;
            default:
                return false;
        }
    }
    private void RefreshHealthStick(Transform a)
    {
        healthStickImage.fillAmount = healthCP.ReturnHealthPercent();
        StartCoroutine("WaitCloseHealthStick");
    }
    IEnumerator WaitCloseHealthStick()
    {
        if (isShowhealthStick)
        {
            yield break;
        }
        isShowhealthStick = true;
        canvas.SetActive(true);
        yield return wait_ShowHealthStick;
        isShowhealthStick = false;
        if (this.enabled)
        {
            canvas.SetActive(false);
        }
    }
    public AICondition ReturnAICondition()
    {
        return aICondition;
    }
    private void OnDisable()
    {
        isShowhealthStick = false;
        canvas.SetActive(false);
        //
        healthCP.onBeAttacked -= BeAttack;
        healthCP.onBeAttacked -= RefreshHealthStick;
    }
}
