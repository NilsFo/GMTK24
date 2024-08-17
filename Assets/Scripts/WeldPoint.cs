using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeldPoint : MonoBehaviour
{
    public enum WeldState
    {
        UNWELDED, CAN_WELD, WELDED
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

    public void Weld()
    {
        if (connectedWeld != null && weldState == WeldState.CAN_WELD && connectedWeld.weldState == WeldState.CAN_WELD)
        {
            weldState = WeldState.WELDED;
            renderer.material.color = Color.green;

            connectedWeld.weldState = WeldState.WELDED;
            connectedWeld.renderer.material.color = Color.green;


            _objectiveLogic.objectiveProgress += 1;
        }
    }

    public void Unweld()
    {
        if  (connectedWeld != null && weldState == WeldState.WELDED && connectedWeld.weldState == WeldState.WELDED)
        {
            weldState = WeldState.CAN_WELD;
            
            renderer.material.color = Color.red;

            connectedWeld.weldState = WeldState.CAN_WELD;
            connectedWeld.renderer.material.color = Color.red;

            _objectiveLogic.objectiveProgress -= 1;
        }
    }
}
