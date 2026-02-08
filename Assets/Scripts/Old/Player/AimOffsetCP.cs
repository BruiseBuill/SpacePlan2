using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AimOffsetCP : MonoBehaviour
{
    [SerializeField] RectTransform rect;
    [SerializeField] float multiple;
    float weaponAttackRange;

    public void SetAim(Vector3 pos,Vector3 modelUp)
    {
        rect.position = Camera.main.WorldToScreenPoint(pos + multiple * weaponAttackRange * modelUp);
    }
    public void ChangeWeapon(float attackRange)
    {
        weaponAttackRange = attackRange;
    }
}
