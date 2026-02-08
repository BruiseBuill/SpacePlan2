using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 阵营管理器，管理两个对立阵营
/// </summary>
public class FactionManager : MonoBehaviour
{
    public static FactionManager Instance { get; private set; }
    
    public UnityAction<string> onFactionDefeated = delegate { }; // 阵营被击败事件
    
    [Header("阵营设置")]
    [SerializeField] private string faction1Tag = "League0";  // 阵营1标签
    [SerializeField] private string faction2Tag = "League1";  // 阵营2标签
    
    private Dictionary<string, List<GameObject>> factionUnits = new Dictionary<string, List<GameObject>>();
    private Dictionary<string, GameObject> factionBases = new Dictionary<string, GameObject>();
    
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
        
        InitializeFactions();
    }
    
    private void InitializeFactions()
    {
        factionUnits[faction1Tag] = new List<GameObject>();
        factionUnits[faction2Tag] = new List<GameObject>();
    }
    
    /// <summary>
    /// 注册单位到阵营
    /// </summary>
    public void RegisterUnit(GameObject unit, string factionTag)
    {
        if (!factionUnits.ContainsKey(factionTag))
        {
            factionUnits[factionTag] = new List<GameObject>();
        }
        
        if (!factionUnits[factionTag].Contains(unit))
        {
            factionUnits[factionTag].Add(unit);
            
            // 如果是基地，单独记录
            MainOfMain mainOfMain = unit.GetComponent<MainOfMain>();
            if (mainOfMain != null && mainOfMain.ReturnObjectKind() == ObjectKind.Base)
            {
                factionBases[factionTag] = unit;
                
                // 监听基地被摧毁事件
                BaseBuilding baseBuilding = unit.GetComponent<BaseBuilding>();
                if (baseBuilding != null)
                {
                    baseBuilding.onBaseExplose += () => OnBaseDestroyed(factionTag);
                }
            }
        }
    }
    
    /// <summary>
    /// 从阵营中移除单位
    /// </summary>
    public void UnregisterUnit(GameObject unit, string factionTag)
    {
        if (factionUnits.ContainsKey(factionTag))
        {
            factionUnits[factionTag].Remove(unit);
        }
    }
    
    /// <summary>
    /// 基地被摧毁时的处理
    /// </summary>
    private void OnBaseDestroyed(string factionTag)
    {
        Debug.Log($"阵营 {factionTag} 的基地被摧毁！");
        onFactionDefeated.Invoke(factionTag);
        
        // 可以在这里添加游戏结束逻辑
        CheckGameEnd();
    }
    
    /// <summary>
    /// 检查游戏是否结束
    /// </summary>
    private void CheckGameEnd()
    {
        int activeFactions = 0;
        string lastActiveFaction = "";
        
        foreach (var kvp in factionBases)
        {
            if (kvp.Value != null && kvp.Value.activeSelf)
            {
                activeFactions++;
                lastActiveFaction = kvp.Key;
            }
        }
        
        if (activeFactions <= 1)
        {
            // 游戏结束
            Debug.Log($"游戏结束！获胜阵营：{lastActiveFaction}");
            // 可以调用GameManager的胜利/失败方法
        }
    }
    
    /// <summary>
    /// 获取指定阵营的所有单位
    /// </summary>
    public List<GameObject> GetFactionUnits(string factionTag)
    {
        if (factionUnits.ContainsKey(factionTag))
        {
            // 清理已销毁的单位
            factionUnits[factionTag].RemoveAll(unit => unit == null || !unit.activeSelf);
            return new List<GameObject>(factionUnits[factionTag]);
        }
        return new List<GameObject>();
    }
    
    /// <summary>
    /// 获取指定阵营的敌人单位
    /// </summary>
    public List<GameObject> GetEnemyUnits(string factionTag)
    {
        List<GameObject> enemies = new List<GameObject>();
        
        foreach (var kvp in factionUnits)
        {
            if (kvp.Key != factionTag)
            {
                // 清理已销毁的单位
                kvp.Value.RemoveAll(unit => unit == null || !unit.activeSelf);
                enemies.AddRange(kvp.Value);
            }
        }
        
        return enemies;
    }
    
    /// <summary>
    /// 检查两个单位是否为敌对关系
    /// </summary>
    public bool AreEnemies(GameObject unit1, GameObject unit2)
    {
        string tag1 = unit1.tag;
        string tag2 = unit2.tag;
        
        return (tag1 == faction1Tag && tag2 == faction2Tag) ||
               (tag1 == faction2Tag && tag2 == faction1Tag);
    }
    
    /// <summary>
    /// 获取指定阵营的基地
    /// </summary>
    public GameObject GetFactionBase(string factionTag)
    {
        if (factionBases.ContainsKey(factionTag))
        {
            return factionBases[factionTag];
        }
        return null;
    }
    
    /// <summary>
    /// 检查阵营是否仍然存在（基地是否存活）
    /// </summary>
    public bool IsFactionActive(string factionTag)
    {
        if (factionBases.ContainsKey(factionTag))
        {
            GameObject baseObj = factionBases[factionTag];
            return baseObj != null && baseObj.activeSelf;
        }
        return false;
    }
}

