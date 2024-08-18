using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationController : MonoBehaviour {
    public Welder welder;
    public Animator welderAnim;
    public Animator remoteAnim;
    private Crane _crane;
    
    private static readonly int Equip = Animator.StringToHash("equip");
    private static readonly int Button = Animator.StringToHash("button");

    // Start is called before the first frame update
    void Start() {
        _crane = FindObjectOfType<Crane>();
        _crane.OnGrabEvent.AddListener(PressRemoteButton);
        _crane.OnDropEvent.AddListener(PressRemoteButton);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        welderAnim.SetBool(Equip, welder.welderState == Welder.WelderState.ACTIVE);
    }

    private void PressRemoteButton() {
        remoteAnim.SetTrigger(Button);
    }
}
