using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public Transform cameraPos;

    private void LateUpdate()
    {
        transform.position = cameraPos.position;
    }
}
