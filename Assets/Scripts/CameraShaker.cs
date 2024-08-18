using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;


public class CameraShaker : MonoBehaviour
{

    public Camera camera;
    public CinemachineVirtualCamera virtualCamera;
    private CinemachineBasicMultiChannelPerlin noise;

    [Header("Camera Shake")] public float cameraShakeDuration = 0f;
    private float _cameraShakeDurationTimer = 0f;
    public float amplitudeGainTarget = 0;
    public float frequencyGainTarget = 0;
    public float cameraShakeResetSpeed = 2f;


    // Start is called before the first frame update
    void Start()
    {
        ResetShake();
        noise = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }

    // Update is called once per frame
    void Update()
    {
        // Shake
        if (cameraShakeDuration > 0)
        {
            _cameraShakeDurationTimer += Time.deltaTime;
        }

        if (amplitudeGainTarget <= 0 || frequencyGainTarget <= 0 || _cameraShakeDurationTimer >= cameraShakeDuration)
        {
            ResetShake();
        }

        if (noise != null)
        {
            //print("amp: "+amplitudeGainTarget+" - freq: " + frequencyGainTarget);
            noise.m_AmplitudeGain = Mathf.Lerp(noise.m_AmplitudeGain, amplitudeGainTarget,
                Time.deltaTime * cameraShakeResetSpeed);
            noise.m_FrequencyGain = Mathf.Lerp(noise.m_FrequencyGain, amplitudeGainTarget,
                Time.deltaTime * cameraShakeResetSpeed);
        }
    }

    public void ShakeCamera(float amplitudeGain, float frequencyGain, float duration)
    {
        if (amplitudeGainTarget < amplitudeGain || frequencyGainTarget < frequencyGain)
        {
            cameraShakeDuration = duration;
            amplitudeGainTarget = Mathf.Max(amplitudeGainTarget, frequencyGain);
            frequencyGainTarget = Mathf.Max(frequencyGainTarget, amplitudeGain);
        }
    }

    public void ResetShake()
    {
        amplitudeGainTarget = 0;
        frequencyGainTarget = 0;
        _cameraShakeDurationTimer = 0;
    }
}
