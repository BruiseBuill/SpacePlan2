using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class PlayerBrain : BaseBrain
{
    public UnityAction<bool> onWaitPlant = delegate { };
    //
    static int[] bdCost = new int[2] { 60, 120 };
    //
    [SerializeField] BuildingButton[] buildingButtons;
    bool isPlant;
    bool canPlant;
    int buildingID;
    Vector3 buildingPos;
    [SerializeField] GameObject checkObj;
    [SerializeField] Image checkImage;
    [SerializeField] QuickButton[] checkButtons;
    //
    bool isConfirm;
    //
    Vector3 viewPointToScreenCenter;
    //
    [SerializeField] Text BDPointText;
    [SerializeField] float BDPoint;
    float BDPointMultiple = 1f;
    public float BuildingPointMultiple
    {
        get { return BDPointMultiple; }
        set { BDPointMultiple = value; }
    }
    //
    static int sellSum = 200;
    static int multiple = 20;
    [SerializeField] QuickButton sellButton;
    //
    float[] rewardOfAllObjectKind = new float[] { 1.1f, 2.4f, 6.5f, 50f, 300f };
    //
    [SerializeField] float refreshPlayerBreak;
    WaitForSeconds wait_RefreshPlayer;
    //
    Vector3 mainBDPos;
    Vector3[] iniBDPos = new Vector3[2];
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
    protected override void Initialize()
    {
        createShipCP.SetUnfixedData(new bool[3] { true, false, false }, 3f,availibleWeaponIDs, 230, new float[3] { 1f, 1.1f, 1.2f });
        plantBDCP.SetUnfixedData(iniBDPos, mainBDPos);
        //
        plantBDCP.enabled = true;
        buffEvent.enabled = true;
        createShipCP.enabled = true;
        GetComponent<RandomEvent>().enabled = true;
        //
        viewPointToScreenCenter = new Vector3(0.5f, 0.5f, 0);
        isPlant = canPlant = isConfirm = false;
        wait_RefreshPlayer = new WaitForSeconds(refreshPlayerBreak);
        //
        
        AddBDPoint(0);
        //
        createShipCP.onUnitCreate += SetUnit;
        plantBDCP.onUnitCreate += SetUnit;
        checkButtons[0].onButtonDown += Cancel;
        checkButtons[1].onButtonDown += Confirm;
        sellButton.onClick += SellBDPoint;
        buffEvent.onUnlockBuilding += UnlockBuilding;
        //
        StartCoroutine("SetPlayer");
    }
    protected override void SetBaseBD()
    {
        plantBDCP.CreateBuilding(mainBDPos).GetComponent<BaseBuilding>().onBaseExplose += OnBaseExplose;
    }
    void SetUnit(GameObject a)
    {
        a.GetComponent<MainOfMain>().onKill += AddBDPoint;
        a.GetComponent<MainOfMain>().onDie += UnitDie;
    }
    void UnitDie(GameObject a)
    {
        a.GetComponent<MainOfMain>().onKill -= AddBDPoint;
        a.GetComponent<MainOfMain>().onDie -= UnitDie;
    }
    public void EnterPlantBuilding(int id)
    {
        if (isPlant)
        {
            Cancel();
        }
        else
        {
            buildingID = id;
            isPlant = true;
            checkObj.transform.position = Camera.main.ViewportToScreenPoint(viewPointToScreenCenter);
            checkObj.SetActive(true);
            //
            StartCoroutine("Check");
        }
    }
    IEnumerator Check()
    {
        Vector3 a = checkObj.transform.position;
        Vector3 b = Vector3.zero;
        while (isPlant)
        {
            b = Camera.main.ScreenToWorldPoint(a);
            b.Set(Mathf.RoundToInt(b.x), Mathf.RoundToInt(b.y), 0);
            checkObj.transform.position = Camera.main.WorldToScreenPoint(b);
            if (plantBDCP.CanPlantBuilding(b,buildingID))
            {
                checkImage.color = Color.green;
                canPlant = true;
            }
            else
            {
                checkImage.color = Color.red;
                canPlant = false;
            }
            if (Input.GetMouseButtonDown(0))
            {
                if (isConfirm)
                    isConfirm = false;
                else
                    a = Input.mousePosition;
            }
            yield return null;
        }
    }
    public void Confirm()
    {
        if (canPlant)
        {
            isPlant = false;
            checkObj.SetActive(false);
            buildingPos = Camera.main.ScreenToWorldPoint(checkObj.transform.position);
            buildingPos.z = 0;
            plantBDCP.CreateBuilding(buildingPos, buildingID);
            //
            AddBDPoint(-bdCost[buildingID]);
            onWaitPlant.Invoke(true);
        }
        else
        {
            isConfirm = true;
        }
    }
    public void Cancel()
    {
        isPlant = false;
        checkObj.SetActive(false);
        //
        onWaitPlant.Invoke(false);
    }
    void AddBDPoint(ObjectKind kind)
    {
        switch (kind)
        {
            case ObjectKind.NoneArmorSP:
                BDPoint += rewardOfAllObjectKind[0] * BDPointMultiple;
                break;
            case ObjectKind.LightArmorSP:
                BDPoint += rewardOfAllObjectKind[1] * BDPointMultiple;
                break;
            case ObjectKind.HeavyArmorSP:
                BDPoint += rewardOfAllObjectKind[2] * BDPointMultiple;
                break;
            case ObjectKind.NormalBuilding:
                BDPoint += rewardOfAllObjectKind[3] * BDPointMultiple;
                break;
            case ObjectKind.Base:
                BDPoint += rewardOfAllObjectKind[4] * BDPointMultiple;
                break;
        }
        BDPointText.text = ((int)BDPoint).ToString();
        SetBuildingButton();
    }
    public bool AddBDPoint(int sum)
    {
        if (BDPoint + sum < 0)
        {
            return false;
        }
        else
        {
            BDPoint += (float)sum;
            BDPointText.text = ((int)BDPoint).ToString();
            SetBuildingButton();
            return true;
        }
    }
    void SetBuildingButton()
    {
        for(int i = 0; i < buildingButtons.Length; i++)
        {
            buildingButtons[i].OnBDPointChange(BDPoint >= bdCost[i]);
        }
    }
    void SellBDPoint()
    {
        if (AddBDPoint(-sellSum))
        {
            createShipCP.AddMoney(sellSum * multiple);
            UIManager.Instance().SetInfoText("下一波兵力大幅增加");
        }
    }
    public void UnlockBuilding(int id)
    {
        buildingButtons[id].transform.parent.gameObject.SetActive(true);
    }
    IEnumerator SetPlayer()
    {
        yield return wait_RefreshPlayer;
        GameObject a;
        while (!(a = createShipCP.ReturnMaxHPShip())) 
        {
            yield return null;
        }
        a.GetComponent<AIControl>().enabled = false;
        a.GetComponent<PlayerControl>().enabled = true;
        a.GetComponent<MainOfMain>().onDie += OnPlayerDie;
    }
    void OnPlayerDie(GameObject a)
    {
        a.GetComponent<MainOfMain>().onDie -= OnPlayerDie;
        a.GetComponent<AIControl>().enabled = true;
        a.GetComponent<PlayerControl>().enabled = false;
        StartCoroutine("SetPlayer");
    }
    void OnBaseExplose()
    {
        GameManager.Instance().PlayerDefeat();
        createShipCP.StopAllCoroutines();
    }
}
