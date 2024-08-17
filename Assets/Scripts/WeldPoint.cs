using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeldPoint : MonoBehaviour
{
    public enum WeldState {
        UNWELDED, CAN_WELD, WELDED
    }

    public WeldState weldState = WeldState.UNWELDED;
    public WeldPoint connectedWeld = null;

    public MeshRenderer renderer;
    
    // Start is called before the first frame update
    void Start()
    {
        weldState = WeldState.UNWELDED;
        renderer.material.color = Color.red;
    }

    public void Weld() {
        if (connectedWeld != null && weldState == WeldState.CAN_WELD && connectedWeld.weldState == WeldState.CAN_WELD) {
            //TODO Check if there is a different weld point opposite of this one
            weldState = WeldState.WELDED;
            renderer.material.color = Color.green;

            connectedWeld.weldState = WeldState.WELDED;
            connectedWeld.renderer.material.color = Color.green;
        }
    }

    public void Unweld() {
        if (connectedWeld != null && weldState == WeldState.WELDED && connectedWeld.weldState == WeldState.WELDED) {
            weldState = WeldState.CAN_WELD;
            //TODO Unweld the other one too
            renderer.material.color = Color.red;
            
            connectedWeld.weldState = WeldState.CAN_WELD;
            connectedWeld.renderer.material.color = Color.red;
        }
    }
}
