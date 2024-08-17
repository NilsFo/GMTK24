using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Welder : MonoBehaviour
{
    public enum WelderState {
        ACTIVE,
        INACTIVE
    }

    public WelderState welderState = WelderState.ACTIVE;

    public float weldDistance = 1.5f;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (welderState == WelderState.ACTIVE) {
            if (Mouse.current.leftButton.wasPressedThisFrame) {
                Weld();
            }
        }
    }

    public void Weld() {
        // Raycast
        
        int layerMask = LayerMask.GetMask("Default", "Entities");

        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, weldDistance, layerMask)) {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
            var weldPoint = hit.collider.gameObject.GetComponent<WeldPoint>();
            if (weldPoint != null) {
                Debug.Log("Weld Point found");
                if(weldPoint.weldState == WeldPoint.WeldState.CAN_WELD)
                    weldPoint.Weld();
                else if(weldPoint.weldState == WeldPoint.WeldState.WELDED)
                    weldPoint.Unweld();
            } else {
                Debug.Log("Not a Weld Point " + hit.transform.gameObject.name, hit.transform);
            }
        }
        else
        {
            Debug.Log("Nothing to Weld");
        }
    }
}
