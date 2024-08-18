using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;


public class CameraShaker : MonoBehaviour
{

    public Camera camera;

    [Header("Camera Shake")] public float cameraShakeDuration = 0f;
    private float _cameraShakeDurationTimer = 0f;
    public float amplitudeGainTarget = 0;
    public float frequencyGainTarget = 0;
    public float cameraShakeResetSpeed = 2f;


    // Start is called before the first frame update
    void Start()
    {
        ResetShake();
    }

    public void ShakeCamera(float amplitude, int frequency, float duration)
    {
        camera.transform.DOShakePosition(duration, amplitude, frequency);
    }

    public void ResetShake() {
        DOTween.Kill(camera.transform);
    }
}
