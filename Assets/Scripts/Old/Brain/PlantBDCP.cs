using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using BF;

public class PlantBDCP : MonoBehaviour
{
    public UnityAction<ObjectKind> onUnitDie = delegate { };
    public UnityAction<GameObject> onUnitCreate = delegate { };
    //
    float addtionalHP;
    public float AdditionHP
    {
        get { return addtionalHP; }
        set { addtionalHP = value; }
    }
    //
    GameObject[] totalBuilding = new GameObject[3];
    [SerializeField] bool[] availibility;
    static float[] buildingRadius = new float[] { 1.3f, 1.3f, 1.3f };
    //
    [SerializeField] Vector3[] iniBuildingPos;
    //
    [SerializeField] Vector3 defaultBDPos;
    //
    List<GameObject> buildingList = new List<GameObject>();
    //
    LayerMask layer;

    private void Awake()
    {
        SetFixedData();
    }
    void SetFixedData()
    {
        totalBuilding[0]= Resources.Load("Prefab/Building/BD_0") as GameObject;
        totalBuilding[1] = Resources.Load("Prefab/Building/BD_1") as GameObject;
        totalBuilding[2] = Resources.Load("Prefab/Building/BD_Main") as GameObject;
        //
        layer = LayerMask.GetMask("Ship", "Building");
    }
    public void SetUnfixedData(Vector3[] iniBuildingPos,Vector3 defaultBDPos)
    {
        this.iniBuildingPos = iniBuildingPos;
        this.defaultBDPos = defaultBDPos;
    }
    private void Start()
    {
        Initialize();
    }
    void Initialize()
    {
        for (int i = 0; i < iniBuildingPos.Length; i++)
        {
            CreateBuilding(iniBuildingPos[i], 0);
        }
    }
    public void CreateBuilding(Vector3 pos, int id)
    {
        GameObject a = PoolManager.Instance().Release(totalBuilding[id].name);
        a.GetComponent<MainOfMain>().onDie += RedoRegisterBuilding;
        a.GetComponent<MainBuilding>().Initialize(pos, Vector3.up, tag, addtionalHP);
        buildingList.Add(a);
        onUnitCreate.Invoke(a);
        a.SetActive(true);
    }
    public GameObject CreateBuilding(Vector3 pos)//CreateBaseBuilding
    {
        GameObject a = PoolManager.Instance().Release(totalBuilding[totalBuilding.Length - 1].name);
        a.GetComponent<MainOfMain>().onDie += RedoRegisterBuilding;
        a.GetComponent<MainBuilding>().Initialize(pos, Vector3.up, tag, addtionalHP);
        buildingList.Add(a);
        onUnitCreate.Invoke(a);
        a.SetActive(true);
        return a;
    }
    public void RedoRegisterBuilding(GameObject a)
    {
        MainOfMain mainOf = a.GetComponent<MainOfMain>();
        mainOf.onDie -= RedoRegisterBuilding;
        onUnitDie.Invoke(mainOf.ReturnObjectKind());
        buildingList.Remove(a);
    }
    public bool CanPlantBuilding(Vector3 pos, int id)
    {
        return availibility[id] && !Physics2D.OverlapCircle(pos, buildingRadius[id], layer);
    }
    public void UnlockBuilding(int id)
    {
        availibility[id] = true;
    }
    public Vector3 ReturnRandomBuildingPos()
    {
        if (buildingList.Count == 0)
        {
            return defaultBDPos;
        }
        else
            return buildingList[Random.Range(0, buildingList.Count)].transform.position;
    }
}
