using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeldTrigger : MonoBehaviour {

    private WeldPoint myWeldPoint;
    
    // Start is called before the first frame update
    void Start() {
        myWeldPoint = GetComponentInParent<WeldPoint>();
    }

    private void OnTriggerEnter(Collider other) {
        var _otherWeldPoint = other.gameObject.GetComponentInParent<WeldPoint>();
        if (_otherWeldPoint == null)
            return;
        myWeldPoint.connectedWeld = _otherWeldPoint;

        myWeldPoint.weldState = WeldPoint.WeldState.CAN_WELD;
        myWeldPoint.connectedWeld.weldState = WeldPoint.WeldState.CAN_WELD;
        
        Debug.Log("Weld point found");
    }
    
    private void OnTriggerExit(Collider other) {
        myWeldPoint.connectedWeld.weldState = WeldPoint.WeldState.UNWELDED;
        myWeldPoint.connectedWeld = null;
        myWeldPoint.weldState = WeldPoint.WeldState.UNWELDED;
    }
}
