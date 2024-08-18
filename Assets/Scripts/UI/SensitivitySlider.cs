using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SensitivitySlider : MonoBehaviour
{

    public Slider mySlider;
    private MouseLook mouseLook;
    private bool hasLook;

    // Start is called before the first frame update
    void Start()
    {
        mySlider.value = LevelShare.userDesiredSensitivity;

        mouseLook = FindObjectOfType<MouseLook>();
        hasLook = mouseLook != null;
    }

    // Update is called once per frame
    void Update()
    {
        LevelShare.userDesiredSensitivity = mySlider.value;

        if (hasLook)
        {
            mouseLook.sensitivitySettings = LevelShare.userDesiredSensitivity;
        }
    }
}
