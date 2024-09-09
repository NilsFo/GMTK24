using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;


[Obsolete]
public class CameraShaker : MonoBehaviour
{
    public new Camera camera;


    // Start is called before the first frame update
    void Start()
    {
        ResetShake();
    }

    public void ShakeCamera(float amplitude, int frequency, float duration)
    {
        camera.transform.DOShakePosition(duration, amplitude, frequency);
    }

    public void ResetShake()
    {
        DOTween.Kill(camera.transform);
    }
}