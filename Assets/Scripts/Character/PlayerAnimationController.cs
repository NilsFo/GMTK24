using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationController : MonoBehaviour {
    public Welder welder;
    public Animator welderAnim;
    
    private static readonly int Equip = Animator.StringToHash("equip");

    // Start is called before the first frame update
    void Start() {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        welderAnim.SetBool(Equip, welder.welderState == Welder.WelderState.ACTIVE);
    }
}
