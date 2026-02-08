using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementCP : MonoBehaviour
{
    [SerializeField] Transform model;
    //
    [SerializeField] float moveSpeed;
    //
    [SerializeField] float rotateSpeed;
    //
    bool isMoving;
    Vector3 moveVector;
    //
    bool isRotating;
    float aimAngle;
    //
    float fixedspeedReduceMultiple;
    [SerializeField] float speedReduceMultiple;

    private void OnEnable()
    {
        isMoving = isRotating = false;
        speedReduceMultiple = 0;
        fixedspeedReduceMultiple = 0;
        moveVector = Vector3.zero;
    }
    public void Move(Vector3 moveVector)
    {
        this.moveVector = moveVector;
        if (!isMoving && moveSpeed != 0) 
        {
            isMoving = true;
            StartCoroutine("Moving");
        }
    }
    public void StopMove()
    {
        isMoving = false;
        moveVector = Vector3.zero;
        StopCoroutine("Moving");
    }
    IEnumerator Moving()
    {
        while (isMoving)
        {
            transform.position += moveSpeed * Time.deltaTime * (1 - speedReduceMultiple) * moveVector;
            yield return null;
        }
    }
    public void Rotate(float Angle)
    {
        aimAngle = Angle;
        if (!isRotating)
        {
            isRotating = true;
            StartCoroutine("Rotating");
        }
    }
    public void Rotate(Vector3 aimVector)
    {
        aimAngle = Mathf.Repeat(Mathf.Atan2(aimVector.y, aimVector.x) * Mathf.Rad2Deg + 270f, 360f);
        if (!isRotating)
        {
            isRotating = true;
            StartCoroutine("Rotating");
        }
    }
    public void StopRotate()
    {
        isRotating = false;
        StopCoroutine("Rotating");
    }
    IEnumerator Rotating()
    {
        while (isRotating)
        {
            if (Mathf.Abs(aimAngle - model.transform.eulerAngles.z) > rotateSpeed * Time.deltaTime)
            {
                model.Rotate(Vector3.forward, Mathf.Repeat(aimAngle - model.transform.eulerAngles.z, 360f) < 180f ? rotateSpeed * Time.deltaTime : -rotateSpeed * Time.deltaTime);
            }
            else
            {
                model.rotation = Quaternion.AngleAxis(aimAngle, Vector3.forward);
            }
            yield return null;
        }
    }
    public void SetRotation(Vector3 aimDirection)
    {
        model.up = aimDirection;
    }
    public Quaternion ReturnRotation()
    {
        return model.rotation;
    }
    public Vector3 ReturnModelUp()
    {
        return model.up;
    }
    public Vector3 ReturnMoveVector()
    {
        return moveVector;
    }
    public float ReturnMoveSpeed()
    {
        return (1 - speedReduceMultiple) * moveSpeed;
    }
    public void ReduceSpeed(float multiple)
    {
        speedReduceMultiple += multiple;
    }
    public void ReduceSpeed(float multiple, float time)
    {
        speedReduceMultiple += multiple;
        StartCoroutine(ReducingSpeed(multiple, time));
    }
    IEnumerator ReducingSpeed(float multiple, float time)
    {
        yield return new WaitForSeconds(time);
        speedReduceMultiple += multiple;
    }
    public void ReduceSpeed(MassLevel massLevel,EngineLevel engineLevel,ArmorLevel armorLevel)
    {
        speedReduceMultiple -= fixedspeedReduceMultiple;
        fixedspeedReduceMultiple = 0;
        switch (massLevel)
        {
            case MassLevel.Large:
                fixedspeedReduceMultiple += 0.3f;
                break;
            case MassLevel.ExLarge:
                fixedspeedReduceMultiple += 0.6f;
                break;
        }
        switch (engineLevel)
        {
            case EngineLevel.Middle:
                fixedspeedReduceMultiple -= 0.2f;
                break;
            case EngineLevel.High:
                fixedspeedReduceMultiple -= 0.4f;
                break;
        }
        switch (armorLevel)
        {
            case ArmorLevel.Light:
                fixedspeedReduceMultiple += 0.1f;
                break;
            case ArmorLevel.Heavy:
                fixedspeedReduceMultiple += 0.2f;
                break;
        }
        speedReduceMultiple += fixedspeedReduceMultiple;
    }
    
}
