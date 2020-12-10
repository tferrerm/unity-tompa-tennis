using UnityEngine;
using UnityEngine.InputSystem;

public class ActionMapper
{
    private readonly InputAction _wasd;
    private readonly InputAction _hitBall;

    public ActionMapper(InputAction wasd, InputAction hitBall)
    {
        _wasd = wasd;
        _hitBall = hitBall;
    }
    public float GetMoveHorizontal(int playerNum = 0)
    {
        //float keyb = (Input.GetKey(KeyCode.LeftArrow) ? -1f : 0f) + (Input.GetKey(KeyCode.RightArrow) ? 1f : 0f);
        return ReadHorizontal();
    }
    
    public float GetMoveVertical(int playerNum = 0)
    {
        //float keyb = (Input.GetKey(KeyCode.UpArrow) ? 1f : 0f) + (Input.GetKey(KeyCode.DownArrow) ? -1f : 0f);
        return ReadVertical();
    }
    
    public bool IsSprinting(int playerNum = 0)
    {
        return false; // Input.GetKey(KeyCode.LeftShift) || Input.GetAxis("RT") > 0.1f || Input.GetAxis("LT") > 0.1f;
    }

    public bool RacquetSwing()
    {
        return _hitBall.triggered; //Input.GetButton("Hit"); // Input.GetKeyDown(KeyCode.Space);
    }

    // TODO: Add sensitivity to HitVertical and HitHorizontal, and maybe to Hit
    public bool GetForward()
    {
        return ReadVertical() > 0; // Input.GetAxis("HitVertical") > 0; // Input.GetKey(KeyCode.UpArrow);
    }
    
    // TODO: Add sensitivity to HitVertical and HitHorizontal, and maybe to Hit
    public bool GetBackward()
    {
        return ReadVertical() < 0; // Input.GetAxis("HitVertical") < 0; // Input.GetKey(KeyCode.DownArrow);
    }
    
    // TODO: Add sensitivity to HitVertical and HitHorizontal, and maybe to Hit
    public bool GetLeft()
    {
        return ReadHorizontal() > 0; // Input.GetAxis("HitHorizontal") < 0; // Input.GetKey(KeyCode.LeftArrow);
    }
    
    // TODO: Add sensitivity to HitVertical and HitHorizontal, and maybe to Hit
    public bool GetRight()
    {
        return ReadHorizontal() < 0; // Input.GetAxis("HitHorizontal") > 0; // Input.GetKey(KeyCode.RightArrow);
    }
    
    private float ReadHorizontal()
    {
        return -1 * _wasd.ReadValue<Vector2>()[0];
    }

    private float ReadVertical()
    {
        return _wasd.ReadValue<Vector2>()[1];
    }
}