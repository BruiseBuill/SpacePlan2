using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BF;


public class WeaponManager : Single<WeaponManager>
{
    [SerializeField] Weapon[] weapons;

    public Weapon ReturnWeapon(int i)
    {
        return weapons[i];
    }
}
