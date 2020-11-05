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
    int frameNumPositionUpdate;
    
    public BallInfo(Vector3 pos)
    {
        position = pos;
        prevPosition = pos;
        frameNumPositionUpdate = -1;
        velocity = Vector3.zero;
        eulerAngles = Vector3.zero;
        grounded = false;
    }
    
    public BallInfo(Vector3 pos, Vector3 vel, Vector3 eulerAng, bool grnd)
    {
        position = pos;
        prevPosition = pos;
        frameNumPositionUpdate = -1;
        velocity = vel;
        eulerAngles = eulerAng;
        grounded = grnd;
    }

    public Vector3 Position
    {
        get { return position; }
        set
        {
            if (frameNumPositionUpdate != Time.frameCount)
            {
                prevPosition = position;
                frameNumPositionUpdate = Time.frameCount;
            }
			
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
        frameNumPositionUpdate = -1;
        velocity = Vector3.zero;
        eulerAngles = Vector3.zero;
        grounded = false;
    }

    public void ResetPrevPosition()
    {
        prevPosition = position;
    }
}
