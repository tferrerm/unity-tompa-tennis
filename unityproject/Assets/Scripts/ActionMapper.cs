using UnityEngine;

public class ActionMapper
{
    
    public static float GetMoveHorizontal(int playerNum = 0)
    {
        float keyb = (Input.GetKey(KeyCode.LeftArrow) ? -1f : 0f) + (Input.GetKey(KeyCode.RightArrow) ? 1f : 0f);
        return Input.GetAxis("Horizontal") + keyb;
    }
    
    public static float GetMoveVertical(int playerNum = 0)
    {
        float keyb = (Input.GetKey(KeyCode.UpArrow) ? 1f : 0f) + (Input.GetKey(KeyCode.DownArrow) ? -1f : 0f);
        return Input.GetAxis("Vertical") + keyb;
    }
    
    public static bool IsSprinting(int playerNum = 0)
    {
        return false; //Input.GetKey(KeyCode.LeftShift) || Input.GetAxis("RT") > 0.1f || Input.GetAxis("LT") > 0.1f;
    }
}