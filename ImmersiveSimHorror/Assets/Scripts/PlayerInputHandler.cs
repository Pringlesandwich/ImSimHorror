using UnityEngine;

public class PlayerInputHandler : MonoBehaviour
{

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    //look
    public float GetAxisRaw(string inputName)
    {
        return Input.GetAxisRaw(inputName);
    }


    //jump
    public bool GetJumpInput()
    {
        return Input.GetKeyDown(KeyCode.Space);
    }


    //run
    public bool GetRunHeld()
    {
        return Input.GetKey(KeyCode.LeftShift);
    }


    //slowwalk
    public bool GetSlowHeld()
    {
        return Input.GetKey(KeyCode.LeftControl);
    }


    //crouch input
    public bool GetCrouchInput()
    {
        return Input.GetKeyDown(KeyCode.C);
    }


    //climbing
    public bool GetClimbInput()
    {
        return Input.GetKeyDown(KeyCode.Space);
    }


    //lean left/right
    public bool GetLeanHeld(bool isLeft)
    {
        var inputNeeded = isLeft ? KeyCode.Q : KeyCode.E;
        return Input.GetKey(inputNeeded);
    }


    public bool GetInteractInput()
    {
        return Input.GetButtonDown("Fire1");
    }


    public bool GetInteractHeld()
    {
        return Input.GetButton("Fire1");
    }
}
