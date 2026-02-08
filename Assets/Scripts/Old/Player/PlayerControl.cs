using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerControl : MonoBehaviour
{
    static Joystick moveJoystick;
    static Joystick attackJoystick;
    //
    MovementCP movementCP;
    AttackCP attackCP;
    WeaponCP weaponCP;
    HealthCP healthCP;
    MainOfMain mainOfMain;
    //
    [SerializeField] bool isMoving;
    [SerializeField] bool isRotating;
    [SerializeField] bool isAttacking;
    //
    [SerializeField] GameObject canvas;
    [SerializeField] Image healthStickImage;
    [SerializeField] Image bulletStickImage;
    //
    [SerializeField] bool isAssistAim;
    public bool IsAssistAim 
    { 
        set { isAssistAim = value; }
        get { return isAssistAim; }
    }
    Transform attackTarget;
    //
    static Text surplusBulletSumText;
    static Button reloadBulletButton;
    static Button changeWeaponButton;
    static Button changeWeaponButtonForward;
    static Image weaponIconImage;
    //
    static QuickButton assistAimButton;
    static AimOffsetCP aimOffset;
    static CameraOffsetCP cameraOffset;
    //
    static Text killText;
    //
    static Button centralizeButton;
    static float effectDistance = 8f;
    //
    static Button weaponListButton;
    static WeaponList weaponList;
    static LoadWeaponImage loadWeaponImage;
    //
    static LayerMask layer;
    
    private void Awake()
    {
        movementCP = GetComponent<MovementCP>();
        attackCP = GetComponent<AttackCP>();
        weaponCP = GetComponent<WeaponCP>();
        healthCP = GetComponent<HealthCP>();
        mainOfMain = GetComponent<MainOfMain>();
    }
    private void OnEnable()
    {
        if (!moveJoystick)
        {
            moveJoystick = GameObject.FindGameObjectWithTag("moveJoystick").GetComponent<Joystick>();
            attackJoystick = GameObject.FindGameObjectWithTag("attackJoystick").GetComponent<Joystick>();
            surplusBulletSumText = GameObject.FindGameObjectWithTag("surplusBulletSumText").GetComponent<Text>();
            reloadBulletButton = GameObject.FindGameObjectWithTag("reloadBulletButton").GetComponent<Button>();
            changeWeaponButton = GameObject.FindGameObjectWithTag("changeWeaponButton").GetComponent<Button>();
            changeWeaponButtonForward =GameObject.FindGameObjectWithTag("changeWeaponButtonForward").GetComponent<Button>();
            killText = GameObject.FindGameObjectWithTag("killText").GetComponent<Text>();
            weaponIconImage = GameObject.FindGameObjectWithTag("weaponImage").GetComponent<Image>();
            
            assistAimButton = GameObject.FindGameObjectWithTag("assistAimButton").GetComponent<QuickButton>();
            aimOffset = GameObject.FindGameObjectWithTag("managers").GetComponent<AimOffsetCP>();
            cameraOffset = GameObject.FindGameObjectWithTag("managers").GetComponent<CameraOffsetCP>();
            centralizeButton= GameObject.FindGameObjectWithTag("centralizeAim").GetComponent<Button>();
            weaponListButton = GameObject.FindGameObjectWithTag("weaponListButton").GetComponent<Button>();
            weaponList = GameObject.FindGameObjectWithTag("weaponListButton").GetComponent<WeaponList>();
            loadWeaponImage = weaponList.ReturnLoadWeaponImage();
            layer = LayerMask.GetMask("Ship");
        }
        //
        attackCP.onShooting += RefreshBulletStick;
        attackCP.onReloading += RefreshBulletStick;
        weaponCP.onChangeWeapon += OnChangeWeapon;
        healthCP.onBeAttacked += RefreshHealthStick;
        mainOfMain.onKill += RefreshKillText;
        reloadBulletButton.onClick.AddListener(ReloadBullet);
        changeWeaponButton.onClick.AddListener(ChangeWeapon);
        changeWeaponButtonForward.onClick.AddListener(ChangeWeaponForward);
        centralizeButton.onClick.AddListener(ChangeAllyWeapon);
        weaponListButton.onClick.AddListener(ShowWeaponList);
        assistAimButton.onClickWithIndex += ChangeAssistAim;
        assistAimButton.Initialize();
        //
        isMoving = isRotating = isAttacking = false;
        movementCP.StopMove();
        //
        RefreshBulletStick();
        RefreshKillText();
        RefreshHealthStick(null);
        healthStickImage.color = Color.green;
        //
        cameraOffset.ChangeAim(transform);
        aimOffset.ChangeWeapon(attackCP.ReturnAttackRange());
        weaponIconImage.sprite = attackCP.ReturnIcon();
        //
        StartCoroutine("WaitNextOnEnable");
    }
    IEnumerator WaitNextOnEnable()
    {
        yield return null;
        canvas.transform.Find("PlayerOnly").gameObject.SetActive(true);
        canvas.SetActive(true);
    }
    private void Update()
    {
        MoveCheck();
        RotateAndAttackCheck();
        CameraOffset();
        AimOffset();
    }
    void MoveCheck()
    {
        if (moveJoystick.Horizontal != 0 || moveJoystick.Vertical != 0)
        {
            isMoving = true;
            movementCP.Move(moveJoystick.Direction.normalized);
        }
        else if (isMoving)
        {
            isMoving = false;
            movementCP.StopMove();
        }
    }
    void RotateAndAttackCheck()
    {
        if (attackJoystick.Horizontal != 0 || attackJoystick.Vertical != 0)
        {
            isRotating = true;
            if (isAssistAim && isAttacking && attackTarget && attackTarget.gameObject.activeSelf && (attackTarget.position - transform.position).sqrMagnitude < attackCP.ReturnSqrAttackRange())
            {
                MovementCP a = attackTarget.GetComponent<MovementCP>();
                movementCP.Rotate(attackTarget.position - transform.position + (attackTarget.position - transform.position).magnitude / attackCP.ReturnBulletSpeed() * a.ReturnMoveSpeed() * a.ReturnMoveVector());
            }
            else
            {
                attackTarget = null;
                movementCP.Rotate(attackJoystick.Direction);
            }
        }
        else if (isRotating)
        {
            isRotating = false;
            movementCP.StopRotate();
        }
        //
        if (attackJoystick.Direction.sqrMagnitude > 0.9f)
        {
            isAttacking = attackCP.Attack(movementCP.ReturnModelUp());
            if (isAssistAim && !attackTarget)
            {
                attackTarget = attackCP.ReturnRaycastTransform(movementCP.ReturnModelUp());
                if (!attackTarget || attackTarget.CompareTag(tag)) 
                {
                    attackTarget = null;
                }
            }
        }
        else if (isAttacking)
        {
            isAttacking = false;
            attackCP.StopAttack();
        }
    }
    void CameraOffset()
    {
        if (isRotating || isMoving)
        {
            if (isRotating)
                cameraOffset.CameraOffset(attackJoystick.Direction);
            else
                cameraOffset.CameraOffset(moveJoystick.Direction);
        }
        else
        {
            cameraOffset.CameraOffset(Vector3.zero);
        }
    }
    void AimOffset()
    {
        aimOffset.SetAim(transform.position, movementCP.ReturnModelUp());
    }
    void RefreshBulletStick()
    {
        int a = 0;
        int b = 0;
        int c = 0;
        attackCP.ReturnBulletDetail(ref a,ref b,ref c);
        bulletStickImage.fillAmount = (float)a / c;
        surplusBulletSumText.text = a.ToString() + "/" + b.ToString();
    }
    void RefreshBulletStick(float progress)
    {
        int a = 0;
        int b = 0;
        int c = 0;
        attackCP.ReturnBulletDetail(ref a, ref b, ref c);
        bulletStickImage.fillAmount = progress;
        surplusBulletSumText.text = c.ToString() + "/" + b.ToString();
    }
    void ReloadBullet()
    {
        attackCP.Reload();
    }
    void ChangeWeapon()
    {
        weaponCP.BeforeTheChange();
    }
    void ChangeWeaponForward()
    {
        weaponCP.BeforeTheChange(-1);
    }
    void OnChangeWeapon(int a,int b)
    {
        aimOffset.ChangeWeapon(attackCP.ReturnAttackRange());
        weaponIconImage.sprite = attackCP.ReturnIcon();
    }
    void RefreshHealthStick(Transform transform)
    {
        healthStickImage.fillAmount = healthCP.ReturnHealthPercent();
    }
    void RefreshKillText()
    {
        killText.text = "击败数：" + mainOfMain.ReturnKillSum().ToString();
    }
    void RefreshKillText(ObjectKind a )
    {
        killText.text = "击败数：" + mainOfMain.ReturnKillSum().ToString();
    }
    public void ChangeAssistAim(int i)
    {
        if (i == 0)
        {
            isAssistAim = false;
            UIManager.Instance().SetInfoText("自瞄已关闭");
        }
        else
        {
            isAssistAim = true;
            UIManager.Instance().SetInfoText("自瞄已开启");
        }
    }
    void ChangeAllyWeapon()
    {
        var ships = Physics2D.OverlapCircleAll(transform.position, effectDistance, layer);
        UIManager.Instance().SetInfoText("部队武器已统一");
        for (int i = 0; i < ships.Length; i++)
        {
            if (ships[i].CompareTag(tag))
            {
                ships[i].GetComponent<WeaponCP>().SetWeapon(weaponCP.ReturnWeaponID());
            }
        }
    }
    void ShowWeaponList()
    {
        loadWeaponImage.SetImages(weaponCP);
    }
    private void OnDisable()
    {
        canvas.SetActive(false);
        //
        weaponList.OnChangeShowing(false);
        //
        attackCP.onShooting -= RefreshBulletStick;
        attackCP.onReloading -= RefreshBulletStick;
        weaponCP.onChangeWeapon -= OnChangeWeapon;
        healthCP.onBeAttacked -= RefreshHealthStick;
        mainOfMain.onKill -= RefreshKillText;
        reloadBulletButton.onClick.RemoveAllListeners();
        changeWeaponButton.onClick.RemoveAllListeners();
        changeWeaponButtonForward.onClick.RemoveAllListeners();
        centralizeButton.onClick.RemoveAllListeners();
        weaponListButton.onClick.RemoveListener(ShowWeaponList);
        assistAimButton.onClickWithIndex -= ChangeAssistAim;
    }
}
