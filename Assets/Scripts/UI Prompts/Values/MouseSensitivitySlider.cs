using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MouseSensitivitySlider : MonoBehaviour
{
    public Slider mySlider;
    private MouseLook mouseLook;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Reading sensitivity slider: " + MusicManager.userDesiredMasterVolume);
        mySlider.value = LevelShare.userDesiredSensitivitySetting;
        mouseLook = FindObjectOfType<MouseLook>(true);
    }

    private void OnEnable()
    {
        mySlider.value = LevelShare.userDesiredSensitivitySetting;
    }

    // Update is called once per frame
    void Update()
    {
        LevelShare.userDesiredSensitivitySetting = mySlider.value;
        mouseLook.userDesiredSensitivitySetting = LevelShare.userDesiredSensitivitySetting;
    }
}