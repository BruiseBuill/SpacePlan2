using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraOffsetCP : MonoBehaviour
{
    [SerializeField] CinemachineVirtualCamera VirtualCamera;
    //
    [SerializeField] float offsetSpeed;
    [SerializeField] CinemachineCameraOffset cameraOffsetClass;
    [SerializeField] float cameraOffset;

    public void CameraOffset(Vector3 vector)
    {
        cameraOffsetClass.m_Offset = Vector3.Lerp(cameraOffsetClass.m_Offset, vector.normalized * cameraOffset, offsetSpeed);
    }
    public void ChangeAim(Transform a)
    {
        VirtualCamera.Follow = a;
    }
}
