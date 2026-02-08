using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MassLevel { Small,Large,ExLarge};

[CreateAssetMenu(menuName ="SpacePlan/Weapon", fileName = "WeaponFile")]
public class Weapon :ScriptableObject
{
    public MassLevel massLevel;
    public float recoil;
    //
    public GameObject bullet;
    public bool isStutterer;
    public float bulletSpeed;
    public float attackRangeOfBullet;
    public int bulletSumPerClip;
    public bool hasInfiniteBullet;
    public int initialBulletSum;
    //
    public GameObject attackFlashEX;
    //
    public float warmBreak;
    public float coolBreak;
    //
    public float reloadTime;
    //
    public Sprite sprite;
}
