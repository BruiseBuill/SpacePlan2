using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WeaponCP : MonoBehaviour
{
    public UnityAction<int,int> onChangeWeapon = delegate { };
    [SerializeField] MassLevel maxMassOfWeapon;
    //
    [SerializeField] int presentWeaponIndex;
    [SerializeField] List<int> weaponIDs = new List<int>();
    [Header("²»Òª¸Ä")]
    [SerializeField] List<int> surplusBulletSum = new List<int>();
    AttackCP attackCP;
    //
    List<Sprite> sprites = new List<Sprite>();

    private void Start()
    {
        attackCP = GetComponent<AttackCP>();
        attackCP.onBulletExhaust += BeforeTheChange;
    }
    public void SetWeapon(params int[] weaponID)
    {
        weaponIDs.Clear();
        for (int i = 0; i < weaponID.Length; i++)
        {
            weaponIDs.Add(weaponID[i]);
        }
    }
    private void OnEnable()
    {
        presentWeaponIndex = Random.Range(0, weaponIDs.Count);
        StartCoroutine("WaitOneFrame");
    }
    IEnumerator WaitOneFrame()
    {
        yield return null;
        surplusBulletSum.Clear();
        foreach (int i in weaponIDs)
        {
            surplusBulletSum.Add(WeaponManager.Instance().ReturnWeapon(i).initialBulletSum);
        }
        ChangeWeapon();
    }
    public void BeforeTheChange()
    {
        surplusBulletSum[presentWeaponIndex] = attackCP.ReturnSurplusBulletSumTotal();
        ChangeWeapon();
    }
    public void BeforeTheChange(int i )
    {
        surplusBulletSum[presentWeaponIndex] = attackCP.ReturnSurplusBulletSumTotal();
        ChangeWeapon(i);
    }
    void ChangeWeapon(int i = 1)
    {
        presentWeaponIndex = (presentWeaponIndex + i+ weaponIDs.Count) % weaponIDs.Count;
        while (surplusBulletSum[presentWeaponIndex] == 0)
        {
            presentWeaponIndex = (presentWeaponIndex + i + weaponIDs.Count) % weaponIDs.Count;
        }
        onChangeWeapon.Invoke(weaponIDs[presentWeaponIndex], surplusBulletSum[presentWeaponIndex]);
    }
    public void SetWeapon(int i)
    {
        if (weaponIDs.Contains(i))
        {
            surplusBulletSum[presentWeaponIndex] = attackCP.ReturnSurplusBulletSumTotal();
            presentWeaponIndex = weaponIDs.IndexOf(i);
            onChangeWeapon.Invoke(weaponIDs[presentWeaponIndex], surplusBulletSum[presentWeaponIndex]);
        }
    }
    public MassLevel ReturnMassLevel()
    {
        return WeaponManager.Instance().ReturnWeapon(weaponIDs[presentWeaponIndex]).massLevel;
    }
    public bool ReturnIfStutterer()
    {
        return WeaponManager.Instance().ReturnWeapon(weaponIDs[presentWeaponIndex]).isStutterer;
    }
    public int ReturnWeaponID()
    {
        return weaponIDs[presentWeaponIndex];
    }
    public List<Sprite> ReturnWeaponImage()
    {
        sprites.Clear();
        for(int i = 0; i < weaponIDs.Count; i++)
        {
            sprites.Add(WeaponManager.Instance().ReturnWeapon(weaponIDs[i]).sprite);
        }
        return sprites;
    }
    public List<int> ReturnWeaponIDs()
    {
        return weaponIDs;
    }
}
