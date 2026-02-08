using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 单位选择管理器，处理玩家的单位选择和命令下达
/// </summary>
public class UnitSelectionManager : MonoBehaviour
{
    public static UnitSelectionManager Instance { get; private set; }
    
    public UnityAction<List<GameObject>> onSelectionChanged = delegate { };
    
    [Header("选择设置")]
    [SerializeField] private LayerMask selectableLayer;  // 可选择单位的图层
    [SerializeField] private KeyCode selectKey = KeyCode.Mouse0;
    [SerializeField] private KeyCode addSelectionKey = KeyCode.LeftShift;
    
    private List<GameObject> selectedUnits = new List<GameObject>();
    private Camera mainCamera;
    private string playerLeague;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        mainCamera = Camera.main;
        if (selectableLayer.value == 0)
        {
            selectableLayer = LayerMask.GetMask("Ship", "Building");
        }
    }
    
    private void Start()
    {
        playerLeague = GameManager.Instance().PlayerLeague;
    }
    
    private void Update()
    {
        HandleSelectionInput();
        HandleCommandInput();
    }
    
    /// <summary>
    /// 处理选择输入
    /// </summary>
    private void HandleSelectionInput()
    {
        if (Input.GetKeyDown(selectKey))
        {
            Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, Mathf.Infinity, selectableLayer);
            
            if (hit.collider != null)
            {
                GameObject hitObject = hit.collider.gameObject;
                
                // 只选择玩家阵营的单位
                if (hitObject.CompareTag(playerLeague))
                {
                    UnitCommandCP commandCP = hitObject.GetComponent<UnitCommandCP>();
                    if (commandCP != null)
                    {
                        if (Input.GetKey(addSelectionKey))
                        {
                            // 添加到选择
                            if (!selectedUnits.Contains(hitObject))
                            {
                                selectedUnits.Add(hitObject);
                            }
                        }
                        else
                        {
                            // 新选择
                            ClearSelection();
                            selectedUnits.Add(hitObject);
                        }
                        
                        onSelectionChanged.Invoke(selectedUnits);
                    }
                }
            }
            else if (!Input.GetKey(addSelectionKey))
            {
                // 点击空白处，清除选择
                ClearSelection();
            }
        }
    }
    
    /// <summary>
    /// 处理命令输入
    /// </summary>
    private void HandleCommandInput()
    {
        if (selectedUnits.Count == 0) return;
        
        // 右键移动/攻击
        if (Input.GetMouseButtonDown(1))
        {
            Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, Mathf.Infinity, selectableLayer);
            
            if (hit.collider != null)
            {
                GameObject target = hit.collider.gameObject;
                
                // 如果是敌人，攻击命令
                if (!target.CompareTag(playerLeague))
                {
                    IssueAttackCommand(target.transform);
                }
                // 如果是友军，跟随命令
                else
                {
                    IssueFollowCommand(target.transform);
                }
            }
            else
            {
                // 移动到指定位置
                IssueMoveCommand(mousePos);
            }
        }
        
        // 防御命令 (D键)
        if (Input.GetKeyDown(KeyCode.D))
        {
            IssueDefendCommand();
        }
        
        // 保持位置命令 (H键)
        if (Input.GetKeyDown(KeyCode.H))
        {
            IssueHoldCommand();
        }
    }
    
    /// <summary>
    /// 下达移动命令
    /// </summary>
    public void IssueMoveCommand(Vector3 targetPosition)
    {
        foreach (var unit in selectedUnits)
        {
            UnitCommandCP commandCP = unit.GetComponent<UnitCommandCP>();
            if (commandCP != null)
            {
                UnitCommand command = new UnitCommand(UnitCommandType.Move, targetPosition);
                commandCP.AddCommand(command);
            }
        }
    }
    
    /// <summary>
    /// 下达攻击命令
    /// </summary>
    public void IssueAttackCommand(Transform target)
    {
        foreach (var unit in selectedUnits)
        {
            UnitCommandCP commandCP = unit.GetComponent<UnitCommandCP>();
            if (commandCP != null)
            {
                UnitCommand command = new UnitCommand(UnitCommandType.Attack, target, priority: 2f);
                commandCP.ExecuteCommandImmediately(command);
            }
        }
    }
    
    /// <summary>
    /// 下达防御命令
    /// </summary>
    public void IssueDefendCommand()
    {
        foreach (var unit in selectedUnits)
        {
            UnitCommandCP commandCP = unit.GetComponent<UnitCommandCP>();
            if (commandCP != null)
            {
                Vector3 defendPosition = unit.transform.position;
                UnitCommand command = new UnitCommand(UnitCommandType.Defend, defendPosition);
                commandCP.ExecuteCommandImmediately(command);
            }
        }
    }
    
    /// <summary>
    /// 下达保持位置命令
    /// </summary>
    public void IssueHoldCommand()
    {
        foreach (var unit in selectedUnits)
        {
            UnitCommandCP commandCP = unit.GetComponent<UnitCommandCP>();
            if (commandCP != null)
            {
                UnitCommand command = new UnitCommand(UnitCommandType.Hold, Vector3.zero);
                commandCP.ExecuteCommandImmediately(command);
            }
        }
    }
    
    /// <summary>
    /// 下达跟随命令
    /// </summary>
    public void IssueFollowCommand(Transform target)
    {
        foreach (var unit in selectedUnits)
        {
            UnitCommandCP commandCP = unit.GetComponent<UnitCommandCP>();
            if (commandCP != null)
            {
                UnitCommand command = new UnitCommand(UnitCommandType.Follow, target);
                commandCP.AddCommand(command);
            }
        }
    }
    
    /// <summary>
    /// 清除选择
    /// </summary>
    public void ClearSelection()
    {
        selectedUnits.Clear();
        onSelectionChanged.Invoke(selectedUnits);
    }
    
    /// <summary>
    /// 获取当前选中的单位列表
    /// </summary>
    public List<GameObject> GetSelectedUnits()
    {
        return new List<GameObject>(selectedUnits);
    }
    
    /// <summary>
    /// 添加单位到选择
    /// </summary>
    public void AddToSelection(GameObject unit)
    {
        if (!selectedUnits.Contains(unit))
        {
            UnitCommandCP commandCP = unit.GetComponent<UnitCommandCP>();
            if (commandCP != null && unit.CompareTag(playerLeague))
            {
                selectedUnits.Add(unit);
                onSelectionChanged.Invoke(selectedUnits);
            }
        }
    }
    
    /// <summary>
    /// 从选择中移除单位
    /// </summary>
    public void RemoveFromSelection(GameObject unit)
    {
        if (selectedUnits.Remove(unit))
        {
            onSelectionChanged.Invoke(selectedUnits);
        }
    }
}

