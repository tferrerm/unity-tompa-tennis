using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct BallInfo
{
    private Vector3 position;
    private Vector3 prevPosition;
    public Vector3 velocity;
    public Vector3 eulerAngles;
    public bool grounded;
    
    public BallInfo(Vector3 pos)
    {
        position = pos;
        prevPosition = pos;
        velocity = Vector3.zero;
        eulerAngles = Vector3.zero;
        grounded = false;
    }
    
    public BallInfo(Vector3 pos, Vector3 vel, Vector3 eulerAng, bool grnd)
    {
        position = pos;
        prevPosition = pos;
        velocity = vel;
        eulerAngles = eulerAng;
        grounded = grnd;
    }

    public BallInfo(BallInfo ballInfo)
    {
        position = ballInfo.position;
        prevPosition = ballInfo.prevPosition;
        velocity = ballInfo.velocity;
        eulerAngles = ballInfo.eulerAngles;
        grounded = ballInfo.grounded;
    }

    public Vector3 Position
    {
        get { return position; }
        set
        {
            prevPosition = position;
            position = value;
        }
    }
	
    public Vector3 PrevPosition { get { return prevPosition; }}
	
    public void Log()
    {
        Debug.Log(string.Format("pos: {0} velocity: {1} angle: {2} grounded: {3}", Position, velocity, eulerAngles, grounded));
    }

    public void Clear()
    {
        velocity = Vector3.zero;
        eulerAngles = Vector3.zero;
        grounded = false;
    }

    public void ResetPrevPosition()
    {
        prevPosition = position;
    }
}
