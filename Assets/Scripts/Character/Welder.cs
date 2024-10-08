using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Welder : MonoBehaviour
{
    public enum WelderState
    {
        ACTIVE,
        INACTIVE
    }

    public bool useGamepadOverKBM = false;
    public WelderState welderState = WelderState.ACTIVE;

    public float weldDistance = 1.5f;

    public AudioSource weldSFX;
    private GameState _gameState;

    // Start is called before the first frame update
    void Start()
    {
        _gameState = FindObjectOfType<GameState>();
    }

    // Update is called once per frame
    void Update()
    {
        Mouse m = Mouse.current;
        Gamepad g = Gamepad.current;
        bool weldInput = false;

        if (!useGamepadOverKBM && m != null)
        {
            weldInput = m.leftButton.wasPressedThisFrame;
        }

        if (useGamepadOverKBM && g != null)
        {
            weldInput = g.buttonWest.wasPressedThisFrame;
        }

        if (welderState == WelderState.ACTIVE && _gameState.currentPlayerState == GameState.PlayerState.Playing)
        {
            if (weldInput)
            {
                Weld();
            }
        }
    }

    private void FixedUpdate()
    {
        welderState = WelderState.INACTIVE;

        int layerMask = LayerMask.GetMask("Default", "Entities");
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, weldDistance,
                layerMask))
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance,
                Color.yellow);
            var weldPoint = hit.collider.gameObject.GetComponent<WeldPoint>();
            if (weldPoint != null)
            {
                if (weldPoint.weldState is WeldPoint.WeldState.CAN_WELD or WeldPoint.WeldState.WELDED)
                    welderState = WelderState.ACTIVE;
            }
        }
    }

    public void Weld()
    {
        // Raycast
        int layerMask = LayerMask.GetMask("Default", "Entities");

        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, weldDistance,
                layerMask))
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance,
                Color.yellow);
            var weldPoint = hit.collider.gameObject.GetComponent<WeldPoint>();
            if (weldPoint != null)
            {
                Debug.Log("Weld Point found");
                if (weldPoint.weldState == WeldPoint.WeldState.CAN_WELD)
                {
                    weldPoint.Weld();
                    weldSFX.Play();
                }
                else if (weldPoint.weldState == WeldPoint.WeldState.WELDED)
                {
                    weldPoint.UnWeld();
                    weldSFX.Play();
                }
            }
            else
            {
                Debug.Log("Not a Weld Point " + hit.transform.gameObject.name, hit.transform);
            }
        }
        else
        {
            Debug.Log("Nothing to Weld");
        }
    }

    public void SetUseGamepadOverKbm(bool newValue)
    {
        useGamepadOverKBM = newValue;
    }
}