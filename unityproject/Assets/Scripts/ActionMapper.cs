using UnityEngine;

public class ActionMapper
{
    
    public static float GetMoveHorizontal(int playerNum = 0)
    {
        //float keyb = (Input.GetKey(KeyCode.LeftArrow) ? -1f : 0f) + (Input.GetKey(KeyCode.RightArrow) ? 1f : 0f);
        return -1 * (Input.GetAxis("Horizontal")/* + keyb*/);
    }
    
    public static float GetMoveVertical(int playerNum = 0)
    {
        //float keyb = (Input.GetKey(KeyCode.UpArrow) ? 1f : 0f) + (Input.GetKey(KeyCode.DownArrow) ? -1f : 0f);
        return Input.GetAxis("Vertical") /*+ keyb*/;
    }
    
    public static bool IsSprinting(int playerNum = 0)
    {
        return false; // Input.GetKey(KeyCode.LeftShift) || Input.GetAxis("RT") > 0.1f || Input.GetAxis("LT") > 0.1f;
    }

    public static bool RacquetSwing()
    {
        return Input.GetButton("Hit"); // Input.GetKeyDown(KeyCode.Space);
    }

    // TODO: Add sensitivity to HitVertical and HitHorizontal, and maybe to Hit
    public static bool GetForward()
    {
        return Input.GetAxis("HitVertical") > 0; // Input.GetKey(KeyCode.UpArrow);
    }
    
    // TODO: Add sensitivity to HitVertical and HitHorizontal, and maybe to Hit
    public static bool GetBackward()
    {
        return Input.GetAxis("HitVertical") < 0; // Input.GetKey(KeyCode.DownArrow);
    }
    
    // TODO: Add sensitivity to HitVertical and HitHorizontal, and maybe to Hit
    public static bool GetLeft()
    {
        return Input.GetAxis("HitHorizontal") < 0; // Input.GetKey(KeyCode.LeftArrow);
    }
    
    // TODO: Add sensitivity to HitVertical and HitHorizontal, and maybe to Hit
    public static bool GetRight()
    {
        return Input.GetAxis("HitHorizontal") > 0; // Input.GetKey(KeyCode.RightArrow);
    }
}