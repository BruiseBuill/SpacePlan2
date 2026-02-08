using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using BF;

public enum ObjectKind{NoneArmorSP,LightArmorSP,HeavyArmorSP,NormalBuilding,Base,Other};

public class CreateShipCP : MonoBehaviour
{
    public UnityAction<ObjectKind> onUnitDie = delegate { };
    public UnityAction<GameObject> onUnitCreate = delegate { };
    //
    GameObject[] totalShips = new GameObject[3];
    [SerializeField] bool[] availibileShip;
    //
    float addtionalHP;
    public float AdditionHP
    {
        get { return addtionalHP; }
        set { addtionalHP = value; }
    }
    //
    float minCreateBreak = 3f;
    WaitForSeconds wait_Create;
    List<GameObject> shipList = new List<GameObject>();
    //
    [SerializeField] List<int> availibleWeaponIDs;
    List<int> ids = new List<int>();//��ʱ����
    //
    int surplusMoney;
    float moneyMultiple;
    public float MoneyMultiple 
    {
        get { return moneyMultiple; }
        set { moneyMultiple = value; }
    }
    [SerializeField] int moneyPerSecond;
    //
    static int[] shipCost = new int[3] { 200, 450, 1200 };
    [SerializeField] float[] possibilityOfShip;
    float totalPossibility;
    int presentCreateShipID = -1;
    [SerializeField] float[] possibilityCount = new float[3];//��ʱ����
    int maxPossibilityIndex;//��ʱ����
    float maxPossibility;//��ʱ����

    private void Awake()
    {
        SetFixedData();
    }
    void SetFixedData()
    {
        totalShips[0] = Resources.Load("Prefab/Ship/Ship0") as GameObject;
        totalShips[1] = Resources.Load("Prefab/Ship/Ship1") as GameObject;
        totalShips[2] = Resources.Load("Prefab/Ship/Ship2") as GameObject;
        //
        moneyMultiple = 1f;
    }
    public void SetUnfixedData(bool[] availibileShip, float minCreateBreak,List<int> availibleWeaponIDs, int moneyPerSecond, float[] possibilityOfShip)
    {
        this.availibileShip = availibileShip;
        this.minCreateBreak = minCreateBreak;
        this.availibleWeaponIDs = availibleWeaponIDs;
        this.moneyPerSecond = moneyPerSecond;
        this.possibilityOfShip = possibilityOfShip;
        //
        for(int i = 0; i < possibilityOfShip.Length; i++)
        {
            totalPossibility += possibilityOfShip[i] * (availibileShip[i] ? 1 : 0);
        }
        presentCreateShipID = SortPossibility();
    }
    private void Start()
    {
        wait_Create = new WaitForSeconds(minCreateBreak);
        CreateShip();
    }
    void CreateShip()
    {
        surplusMoney += (int)(moneyPerSecond * minCreateBreak * moneyMultiple);
        while (surplusMoney > shipCost[presentCreateShipID]) 
        {
            GameObject a = PoolManager.Instance().Release(totalShips[presentCreateShipID].name);
            SetShip(a);
            surplusMoney -= shipCost[presentCreateShipID];
            //
            presentCreateShipID = SortPossibility();
            //
        }
        StartCoroutine("WaitingCreate");
    }
    int SortPossibility()
    {
        int i;
        for (i = 0; i < possibilityCount.Length; i++)
        {
            possibilityCount[i] += possibilityOfShip[i] * (availibileShip[i] ? 1 : 0);
        }
        maxPossibilityIndex = 0;
        maxPossibility = 0;
        for(i = 0; i < possibilityOfShip.Length; i++)
        {
            if (maxPossibility < possibilityCount[i])
            {
                maxPossibility = possibilityCount[i];
                maxPossibilityIndex = i;
            }
        }
        possibilityCount[maxPossibilityIndex] -= totalPossibility;
        return maxPossibilityIndex;
    }
    void SetShip(GameObject a)
    {
        a.GetComponent<MainOfMain>().onDie += RedoRegisterShip;
        a.GetComponent<MainShipCP>().Initialize(transform.position, Vector3.up, tag, addtionalHP);
        SetWeapon(a);
        shipList.Add(a);
        onUnitCreate.Invoke(a);
        a.SetActive(true);
    }
    void SetWeapon(GameObject a)
    {
        int i = 0;
        int j = 0;
        ids.Clear();
        switch (a.GetComponent<MainShipCP>().ReturnEngineLevel()) 
        {
            case EngineLevel.Low:
                j = 3;
                break;
            case EngineLevel.Middle:
                j = 8;
                break;
            case EngineLevel.High:
                j = 10;
                break;
        }
        for (i = 0; i < availibleWeaponIDs.Count; i++)
        {
            if (availibleWeaponIDs[i] < j)
                ids.Add(availibleWeaponIDs[i]);
        }
        a.GetComponent<WeaponCP>().SetWeapon(ids.ToArray());
    }
    IEnumerator WaitingCreate()
    {
        yield return wait_Create;
        CreateShip();
    }
    public void RedoRegisterShip(GameObject a)
    {
        MainOfMain mainOf = a.GetComponent<MainOfMain>();
        mainOf.onDie -= RedoRegisterShip;
        onUnitDie.Invoke(mainOf.ReturnObjectKind());
        shipList.Remove(a);
    }
    public GameObject ReturnMaxHPShip()
    {
        if (shipList.Count == 0)
        {
            return null;
        }
        float max = 0;
        int index = 0;
        float c;
        for (int i = 0; i < shipList.Count; i++)
        {
            c = shipList[i].GetComponent<HealthCP>().ReturnHealthPoint();
            if (max <= c)
            {
                index = i;
                max = c;
            }
        }
        return shipList[index];
    }
    public void AddMoney(int sum)
    {
        surplusMoney += sum;
    }
    public void UnlockShip(int id)
    {
        availibileShip[id] = true;
        totalPossibility += possibilityOfShip[id];
    }
    public void UnlockWeapon(int id)
    {
        availibleWeaponIDs.Add(id);
        availibleWeaponIDs.Sort();
    }
    public Vector3 ReturnAveShipPos()
    {
        Vector3 j = Vector3.zero;
        for(int i = 0; i < shipList.Count; i++)
        {
            j += shipList[i].transform.position;
        }
        return j / shipList.Count;
    }
}
