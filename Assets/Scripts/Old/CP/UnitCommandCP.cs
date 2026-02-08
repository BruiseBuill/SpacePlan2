using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 单位命令类型
/// </summary>
public enum UnitCommandType
{
    None,       // 无命令
    Move,       // 移动到指定位置
    Attack,     // 攻击指定目标
    Defend,     // 防御模式（停留在当前位置，攻击范围内敌人）
    Follow,     // 跟随目标
    Hold        // 保持位置（停止移动，但可以攻击）
}

/// <summary>
/// 单位命令数据
/// </summary>
[System.Serializable]
public class UnitCommand
{
    public UnitCommandType commandType;
    public Vector3 targetPosition;
    public Transform targetTransform;
    public float priority; // 命令优先级
    
    public UnitCommand(UnitCommandType type, Vector3 position, float priority = 1f)
    {
        this.commandType = type;
        this.targetPosition = position;
        this.targetTransform = null;
        this.priority = priority;
    }
    
    public UnitCommand(UnitCommandType type, Transform target, float priority = 1f)
    {
        this.commandType = type;
        this.targetPosition = Vector3.zero;
        this.targetTransform = target;
        this.priority = priority;
    }
}

/// <summary>
/// 单位命令组件，处理单位的命令系统
/// </summary>
public class UnitCommandCP : MonoBehaviour
{
    public UnityAction<UnitCommand> onCommandReceived = delegate { };
    public UnityAction<UnitCommandType> onCommandChanged = delegate { };
    
    private UnitCommand currentCommand;
    private Queue<UnitCommand> commandQueue = new Queue<UnitCommand>();
    
    private MovementCP movementCP;
    private AttackCP attackCP;
    private AIControl aiControl;
    private UnitTypeCP unitTypeCP;
    
    [Header("命令执行参数")]
    [SerializeField] private float moveReachDistance = 0.5f;  // 到达目标点的距离阈值
    [SerializeField] private float defendRadius = 5f;         // 防御模式下的防御半径
    
    private bool isExecutingCommand = false;
    
    private void Awake()
    {
        movementCP = GetComponent<MovementCP>();
        attackCP = GetComponent<AttackCP>();
        aiControl = GetComponent<AIControl>();
        unitTypeCP = GetComponent<UnitTypeCP>();
    }
    
    private void OnEnable()
    {
        currentCommand = null;
        commandQueue.Clear();
        isExecutingCommand = false;
    }
    
    /// <summary>
    /// 添加命令到队列
    /// </summary>
    public void AddCommand(UnitCommand command)
    {
        if (command == null) return;
        
        // 如果当前没有命令或新命令优先级更高，立即执行
        if (currentCommand == null || command.priority > currentCommand.priority)
        {
            ExecuteCommand(command);
        }
        else
        {
            commandQueue.Enqueue(command);
        }
        
        onCommandReceived.Invoke(command);
    }
    
    /// <summary>
    /// 立即执行命令（清除队列）
    /// </summary>
    public void ExecuteCommandImmediately(UnitCommand command)
    {
        commandQueue.Clear();
        ExecuteCommand(command);
        onCommandReceived.Invoke(command);
    }
    
    /// <summary>
    /// 执行命令
    /// </summary>
    private void ExecuteCommand(UnitCommand command)
    {
        if (command == null) return;
        
        currentCommand = command;
        isExecutingCommand = true;
        onCommandChanged.Invoke(command.commandType);
        
        StartCoroutine(ExecuteCommandCoroutine(command));
    }
    
    private IEnumerator ExecuteCommandCoroutine(UnitCommand command)
    {
        switch (command.commandType)
        {
            case UnitCommandType.Move:
                yield return StartCoroutine(ExecuteMoveCommand(command));
                break;
                
            case UnitCommandType.Attack:
                yield return StartCoroutine(ExecuteAttackCommand(command));
                break;
                
            case UnitCommandType.Defend:
                yield return StartCoroutine(ExecuteDefendCommand(command));
                break;
                
            case UnitCommandType.Hold:
                yield return StartCoroutine(ExecuteHoldCommand(command));
                break;
                
            case UnitCommandType.Follow:
                yield return StartCoroutine(ExecuteFollowCommand(command));
                break;
        }
        
        // 命令执行完成，执行下一个命令
        if (commandQueue.Count > 0)
        {
            ExecuteCommand(commandQueue.Dequeue());
        }
        else
        {
            currentCommand = null;
            isExecutingCommand = false;
            onCommandChanged.Invoke(UnitCommandType.None);
        }
    }
    
    /// <summary>
    /// 执行移动命令
    /// </summary>
    private IEnumerator ExecuteMoveCommand(UnitCommand command)
    {
        if (!unitTypeCP.CanMove)
        {
            yield break;
        }
        
        Vector3 targetPos = command.targetPosition;
        float sqrDistance = float.MaxValue;
        
        while (sqrDistance > moveReachDistance * moveReachDistance)
        {
            sqrDistance = (targetPos - transform.position).sqrMagnitude;
            Vector3 direction = (targetPos - transform.position).normalized;
            
            movementCP.Move(direction);
            movementCP.Rotate(direction);
            
            yield return null;
        }
        
        movementCP.StopMove();
    }
    
    /// <summary>
    /// 执行攻击命令
    /// </summary>
    private IEnumerator ExecuteAttackCommand(UnitCommand command)
    {
        Transform target = command.targetTransform;
        if (target == null || !target.gameObject.activeSelf)
        {
            yield break;
        }
        
        // 移动到攻击范围
        if (unitTypeCP.CanMove)
        {
            float attackRange = unitTypeCP.AttackRange;
            float sqrAttackRange = attackRange * attackRange;
            float sqrDistance = (target.position - transform.position).sqrMagnitude;
            
            while (sqrDistance > sqrAttackRange)
            {
                Vector3 direction = (target.position - transform.position).normalized;
                movementCP.Move(direction);
                movementCP.Rotate(target.position - transform.position);
                
                sqrDistance = (target.position - transform.position).sqrMagnitude;
                yield return null;
            }
        }
        
        // 持续攻击目标
        while (target != null && target.gameObject.activeSelf)
        {
            Vector3 attackDirection = (target.position - transform.position).normalized;
            movementCP.Rotate(target.position - transform.position);
            attackCP.Attack(attackDirection);
            
            yield return null;
        }
        
        attackCP.StopAttack();
        movementCP.StopMove();
    }
    
    /// <summary>
    /// 执行防御命令
    /// </summary>
    private IEnumerator ExecuteDefendCommand(UnitCommand command)
    {
        Vector3 defendPosition = command.targetPosition != Vector3.zero 
            ? command.targetPosition 
            : transform.position;
        
        LayerMask enemyLayer = LayerMask.GetMask("Ship", "Building");
        float defendRadiusSqr = defendRadius * defendRadius;
        
        while (true)
        {
            // 如果单位可以移动，保持在防御位置附近
            if (unitTypeCP.CanMove)
            {
                float distanceFromDefendPos = (transform.position - defendPosition).sqrMagnitude;
                if (distanceFromDefendPos > defendRadiusSqr)
                {
                    Vector3 direction = (defendPosition - transform.position).normalized;
                    movementCP.Move(direction);
                }
                else
                {
                    movementCP.StopMove();
                }
            }
            
            // 搜索并攻击范围内的敌人
            Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, unitTypeCP.AttackRange, enemyLayer);
            Transform closestEnemy = null;
            float closestDistance = float.MaxValue;
            
            foreach (var enemy in enemies)
            {
                if (!enemy.CompareTag(tag) && enemy.gameObject.activeSelf)
                {
                    float distance = (enemy.transform.position - transform.position).sqrMagnitude;
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestEnemy = enemy.transform;
                    }
                }
            }
            
            if (closestEnemy != null)
            {
                Vector3 attackDirection = (closestEnemy.position - transform.position).normalized;
                movementCP.Rotate(closestEnemy.position - transform.position);
                attackCP.Attack(attackDirection);
            }
            else
            {
                attackCP.StopAttack();
            }
            
            yield return null;
        }
    }
    
    /// <summary>
    /// 执行保持位置命令
    /// </summary>
    private IEnumerator ExecuteHoldCommand(UnitCommand command)
    {
        movementCP.StopMove();
        
        // 保持位置但可以攻击
        LayerMask enemyLayer = LayerMask.GetMask("Ship", "Building");
        
        while (true)
        {
            Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, unitTypeCP.AttackRange, enemyLayer);
            Transform closestEnemy = null;
            float closestDistance = float.MaxValue;
            
            foreach (var enemy in enemies)
            {
                if (!enemy.CompareTag(tag) && enemy.gameObject.activeSelf)
                {
                    float distance = (enemy.transform.position - transform.position).sqrMagnitude;
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestEnemy = enemy.transform;
                    }
                }
            }
            
            if (closestEnemy != null)
            {
                Vector3 attackDirection = (closestEnemy.position - transform.position).normalized;
                movementCP.Rotate(closestEnemy.position - transform.position);
                attackCP.Attack(attackDirection);
            }
            else
            {
                attackCP.StopAttack();
            }
            
            yield return null;
        }
    }
    
    /// <summary>
    /// 执行跟随命令
    /// </summary>
    private IEnumerator ExecuteFollowCommand(UnitCommand command)
    {
        Transform target = command.targetTransform;
        if (target == null || !unitTypeCP.CanMove)
        {
            yield break;
        }
        
        float followDistance = 2f;
        float followDistanceSqr = followDistance * followDistance;
        
        while (target != null && target.gameObject.activeSelf)
        {
            float sqrDistance = (target.position - transform.position).sqrMagnitude;
            
            if (sqrDistance > followDistanceSqr)
            {
                Vector3 direction = (target.position - transform.position).normalized;
                movementCP.Move(direction);
                movementCP.Rotate(target.position - transform.position);
            }
            else
            {
                movementCP.StopMove();
            }
            
            yield return null;
        }
        
        movementCP.StopMove();
    }
    
    /// <summary>
    /// 取消当前命令
    /// </summary>
    public void CancelCommand()
    {
        commandQueue.Clear();
        currentCommand = null;
        isExecutingCommand = false;
        
        if (movementCP != null)
            movementCP.StopMove();
        if (attackCP != null)
            attackCP.StopAttack();
        
        onCommandChanged.Invoke(UnitCommandType.None);
    }
    
    /// <summary>
    /// 获取当前命令
    /// </summary>
    public UnitCommand GetCurrentCommand()
    {
        return currentCommand;
    }
    
    /// <summary>
    /// 检查是否正在执行命令
    /// </summary>
    public bool IsExecutingCommand()
    {
        return isExecutingCommand;
    }
}

