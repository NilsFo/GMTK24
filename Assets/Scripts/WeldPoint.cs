using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeldPoint : MonoBehaviour
{
    public enum WeldState
    {
        UNWELDED,
        CAN_WELD,
        WELDED
    }

    public WeldState weldState = WeldState.UNWELDED;
    public WeldPoint connectedWeld = null;

    public new MeshRenderer renderer;

    private ObjectiveLogic _objectiveLogic;


    // Start is called before the first frame update
    void Start()
    {
        _objectiveLogic = FindObjectOfType<ObjectiveLogic>();

        weldState = WeldState.UNWELDED;
        renderer.material.color = Color.red;
    }

    private void Update()
    {
        switch (weldState)
        {
            case WeldState.CAN_WELD:
                if ((int)Time.time % 2 == 0)
                {
                    renderer.material.color = Color.yellow;
                }
                else
                {
                    renderer.material.color = Color.red;
                }

                break;
            case WeldState.WELDED:
                renderer.material.color = Color.green;
                break;
            case WeldState.UNWELDED:
                renderer.material.color = Color.red;
                break;
        }
    }

    public void Weld()
    {
        if (connectedWeld != null && weldState == WeldState.CAN_WELD && connectedWeld.weldState == WeldState.CAN_WELD)
        {
            weldState = WeldState.WELDED;
            connectedWeld.weldState = WeldState.WELDED;

            _objectiveLogic.objectiveProgress += 1;
        }
    }

    public void UnWeld()
    {
        if (connectedWeld != null && weldState == WeldState.WELDED && connectedWeld.weldState == WeldState.WELDED)
        {
            weldState = WeldState.CAN_WELD;
            connectedWeld.weldState = WeldState.CAN_WELD;

            _objectiveLogic.objectiveProgress -= 1;
        }
    }
}