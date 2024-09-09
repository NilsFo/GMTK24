using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class VolumeSlider : MonoBehaviour
{
    public Slider mySlider;

    // Start is called before the first frame update
    void Start()
    {
        mySlider.value = MusicManager.userDesiredMasterVolume;
    }

    private void OnEnable()
    {
        mySlider.value = MusicManager.userDesiredMasterVolume;
    }

    // Update is called once per frame
    void Update()
    {
        MusicManager.userDesiredMasterVolume = mySlider.value;
    }
}