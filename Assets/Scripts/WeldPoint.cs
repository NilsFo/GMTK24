using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeldPoint : MonoBehaviour
{
    public enum WeldState {
        UNWELDED, WELDED
    }

    public WeldState weldState = WeldState.UNWELDED;
    public Vector3Int localGridPosition;
    public int weldDirection;

    public MeshRenderer renderer;
    
    // Start is called before the first frame update
    void Start()
    {
        weldState = WeldState.UNWELDED;
        renderer.material.color = Color.red;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Weld() {
        if (weldState == WeldState.UNWELDED) {
            //TODO Check if there is a different weld point opposite of this one
            weldState = WeldState.WELDED;
            renderer.material.color = Color.green;
        }
    }

    public void Unweld() {
        if (weldState == WeldState.WELDED) {
            weldState = WeldState.UNWELDED;
            //TODO Unweld the other one too
            renderer.material.color = Color.red;
        }
    }
}
