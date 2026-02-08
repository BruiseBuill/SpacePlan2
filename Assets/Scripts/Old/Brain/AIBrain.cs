using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Difficulty { Easy, Normal, Hard };

public class AIBrain : BaseBrain
{
    [SerializeField] Difficulty difficulty;
    //
    static int[] bdCost = new int[2] { 60, 120 };
    int multiple;
    //
    static int maxFailCount = 4;
    int offsetDistance;
    int failCount;//
    Vector3 buildingPos;//
    //
    float[] waitTimeOfBuilding = new float[2];
    WaitForSeconds[] waitReloadBD = new WaitForSeconds[2];
    //
    float buffBreakTime;
    WaitForSeconds wait_buff;
    //
    int buffIndex;//
    List<int> remainOption = new List<int>();
    //
    Vector3 mainBDPos;
    Vector3[] iniBDPos=new Vector3[2];
    //
    List<int> availibleWeaponIDs = new List<int>();

    private void Awake()
    {
        mainBDPos = transform.position / 6 * 5;
        iniBDPos[0].Set(mainBDPos.x * 0.7f, mainBDPos.y, 0f);
        iniBDPos[1].Set(mainBDPos.x, mainBDPos.y * 0.7f, 0f);
        availibleWeaponIDs.Add(0);
        availibleWeaponIDs.Add(1);
    }
    public void SetDifficulty(Difficulty difficulty)
    {
        this.difficulty = difficulty;
    }
    protected override void Initialize()
    {
        switch (difficulty)
        {
            case Difficulty.Easy:
                waitTimeOfBuilding[0] = 75f;
                waitTimeOfBuilding[1] = 500f;
                offsetDistance = 3;
                buffBreakTime = 45f;
                multiple = 0;
                createShipCP.SetUnfixedData(new bool[3] { true, false, false }, 3f, availibleWeaponIDs, 190, new float[3] { 1.2f, 1f, 0.3f });
                plantBDCP.SetUnfixedData(iniBDPos, mainBDPos);
                break;
            case Difficulty.Normal:
                waitTimeOfBuilding[0] = 40f;
                waitTimeOfBuilding[1] = 100f;
                offsetDistance = 4;
                remainOption.Add(0);
                buffBreakTime = 35f;
                multiple = 2;
                createShipCP.SetUnfixedData(new bool[3] { true, false, false }, 3f, availibleWeaponIDs, 210, new float[3] { 1.1f, 1f, 0.7f });
                plantBDCP.SetUnfixedData(iniBDPos, mainBDPos);
                break;
            case Difficulty.Hard:
                waitTimeOfBuilding[0] = 22f;
                waitTimeOfBuilding[1] = 60f;
                offsetDistance = 5;
                remainOption.Add(0);
                remainOption.Add(0);
                buffBreakTime = 29f;
                multiple = 4;
                createShipCP.SetUnfixedData(new bool[3] { true, false, false }, 3f, availibleWeaponIDs, 230, new float[3] { 1f, 1f, 1f });
                plantBDCP.SetUnfixedData(iniBDPos, mainBDPos);
                break;
        }
        //
        plantBDCP.enabled = true;
        createShipCP.enabled = true;
        //
        remainOption.Add(1);
        remainOption.Add(3);
        remainOption.Add(5);
        //
        waitReloadBD[0] = new WaitForSeconds(waitTimeOfBuilding[0]);
        waitReloadBD[1] = new WaitForSeconds(waitTimeOfBuilding[1]);
        wait_buff = new WaitForSeconds(buffBreakTime);
        //
        StartCoroutine("Buff");
        StartCoroutine("WaitReloadBD", 0);
        StartCoroutine("WaitReloadBD", 1);
    }
    protected override void SetBaseBD()
    {
        plantBDCP.CreateBuilding(mainBDPos).GetComponent<BaseBuilding>().onBaseExplose += OnBaseExplose;
    }
    IEnumerator WaitReloadBD(int index)
    {
        yield return waitReloadBD[index];
        PlantBuilding(index);
        yield return StartCoroutine("WaitReloadBD", index);
    }
    void PlantBuilding(int id)
    {
        failCount = 0;
        buildingPos = createShipCP.ReturnAveShipPos();
        //
        buildingPos.Set(Mathf.RoundToInt(buildingPos.x), Mathf.RoundToInt(buildingPos.y), 0);
        while (!plantBDCP.CanPlantBuilding(buildingPos,id))
        {
            buildingPos.Set(buildingPos.x + Random.Range(-offsetDistance, offsetDistance + 1), buildingPos.y + Random.Range(-offsetDistance, offsetDistance + 1), 0);
            failCount++;
            if (failCount >= maxFailCount) 
            {
                Sell(id);
                return;
            }
        }
        plantBDCP.CreateBuilding(buildingPos, id);
    }
    void Sell(int id)
    {
        createShipCP.AddMoney(bdCost[id] * multiple);
    }
    IEnumerator Buff()
    {
        buffIndex = Random.Range(0, remainOption.Count);

        buffEvent.ChooseBuff(remainOption[buffIndex]);
        AddEvent(remainOption[buffIndex]);
        remainOption.RemoveAt(buffIndex);
        yield return wait_buff;
        if (remainOption.Count > 0) 
        {
            yield return StartCoroutine("Buff");
        }
    }
    void OnBaseExplose()
    {
        GameManager.Instance().AIDefeat(tag);
        createShipCP.StopAllCoroutines();
        StopAllCoroutines();
    }
    void AddEvent(int id)
    {
        switch (id)
        {
            case 1:
                remainOption.Add(0);
                remainOption.Add(0);
                remainOption.Add(2);
                remainOption.Add(6);
                remainOption.Add(7);
                remainOption.Add(8);
                remainOption.Add(9);
                remainOption.Add(10);
                break;
            case 2:
                remainOption.Add(0);
                remainOption.Add(0);
                remainOption.Add(11);
                remainOption.Add(12);
                break;
            case 3:
                remainOption.Add(0);
                remainOption.Add(0);
                remainOption.Add(4);
                break;
            case 4:
                remainOption.Add(0);
                remainOption.Add(0);
                break;
        }
    }
}
