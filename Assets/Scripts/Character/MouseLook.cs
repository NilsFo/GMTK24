﻿using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

[AddComponentMenu("Camera/Simple Smooth Mouse Look ")]
public class MouseLook : MonoBehaviour
{
    private Vector2 _mouseAbsolute;
    private Vector2 _smoothMouse;

    [Header("Player Settings")] [Range(0, 1)]
    public float userDesiredSensitivitySetting = 0.5f;

    public float sensitivityScaling = 3f;
    private float _sensitivitySettings;

    [Header("Gamepad Config")] public bool useGamepadOverKBM = true;
    public Vector2 gamepadScaling = new Vector2(13f, 10f);

    [Header("Control Config")] public bool mouseLookEnabled = true;
    public Vector2 clampInDegrees = new Vector2(360f, 180f);
    public bool lockCursor;
    public Vector2 sensitivity = new Vector2(4f, 4f);
    public Vector2 smoothing = new Vector2(3f, 3f);

    [Header("Look Direction")] public Vector2 targetDirection;
    public Vector2 targetCharacterDirection;

    // Assign this if there's a parent object controlling motion, such as a Character Controller.
    // Yaw rotation will affect this object instead of the camera if set.
    public GameObject characterBody;

    void Start()
    {
        // Set target direction to the camera's initial orientation.
        targetDirection = transform.localRotation.eulerAngles;

        // Set target direction for the character body to its inital state.
        if (characterBody)
            targetCharacterDirection = characterBody.transform.localRotation.eulerAngles;
    }

    void Update()
    {
        if (!mouseLookEnabled || Time.timeScale == 0)
        {
            Cursor.lockState = CursorLockMode.None;
            return;
        }

        // Ensure the cursor is always locked when set
        if (lockCursor || useGamepadOverKBM)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // Updating sensitivity
        _sensitivitySettings =
            userDesiredSensitivitySetting * Mathf.Pow(userDesiredSensitivitySetting, sensitivityScaling) + 0.01f;
        _sensitivitySettings = Mathf.Clamp(_sensitivitySettings, 0.01f, sensitivityScaling);

        // Allow the script to clamp based on a desired target value.
        Quaternion targetOrientation = Quaternion.Euler(targetDirection);
        Quaternion targetCharacterOrientation = Quaternion.Euler(targetCharacterDirection);

        // Get raw mouse input for a cleaner reading on more sensitive mice.
        Vector2 mouseDelta = new Vector2(0f, 0f);
        if (useGamepadOverKBM)
        {
            Gamepad gamepad = Gamepad.current;
            if (gamepad != null)
            {
                mouseDelta = gamepad.rightStick.ReadValue() * gamepadScaling;
            }
        }
        else
        {
            Mouse mouse = Mouse.current;
            if (mouse != null)
            {
                mouseDelta = mouse.delta.ReadValue();
            }
        }

        // Scale input against the sensitivity setting and multiply that against the smoothing value.
        mouseDelta = Vector2.Scale(mouseDelta,
            new Vector2(sensitivity.x * smoothing.x * _sensitivitySettings,
                sensitivity.y * smoothing.y * _sensitivitySettings));

        // Interpolate mouse movement over time to apply smoothing delta.
        _smoothMouse.x = Mathf.Lerp(_smoothMouse.x, mouseDelta.x, 1f / smoothing.x);
        _smoothMouse.y = Mathf.Lerp(_smoothMouse.y, mouseDelta.y, 1f / smoothing.y);

        // Find the absolute mouse movement value from point zero.
        _mouseAbsolute += _smoothMouse;

        // Clamp and apply the local x value first, so as not to be affected by world transforms.
        if (clampInDegrees.x < 360)
            _mouseAbsolute.x = Mathf.Clamp(_mouseAbsolute.x, -clampInDegrees.x * 0.5f, clampInDegrees.x * 0.5f);

        // Then clamp and apply the global y value.
        if (clampInDegrees.y < 360)
            _mouseAbsolute.y = Mathf.Clamp(_mouseAbsolute.y, -clampInDegrees.y * 0.5f, clampInDegrees.y * 0.5f);

        transform.localRotation = Quaternion.AngleAxis(-_mouseAbsolute.y, targetOrientation * Vector3.right) *
                                  targetOrientation;

        // If there's a character body that acts as a parent to the camera
        if (characterBody)
        {
            var yRotation = Quaternion.AngleAxis(_mouseAbsolute.x, Vector3.up);
            characterBody.transform.localRotation = yRotation * targetCharacterOrientation;
        }
        else
        {
            var yRotation = Quaternion.AngleAxis(_mouseAbsolute.x, transform.InverseTransformDirection(Vector3.up));
            transform.localRotation *= yRotation;
        }
    }

    private void LateUpdate()
    {
        userDesiredSensitivitySetting = Mathf.Clamp(userDesiredSensitivitySetting, 0f, 1f);
    }

    public void SetUseGamepadOverKbm(bool newValue)
    {
        useGamepadOverKBM = newValue;
    }
}