using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecordedReplayInfo
{
    public Vector3 player1Position;
    public Quaternion player1Rotation;
    public Vector3 player2Position;
    public Quaternion player2Rotation;
    public Vector3 ballPosition;
    public Quaternion ballRotation;

    public RecordedReplayInfo(
        Vector3 player1Position, Quaternion player1Rotation,
        Vector3 player2Position, Quaternion player2Rotation,
        Vector3 ballPosition, Quaternion ballRotation)
    {
        this.player1Position = player1Position;
        this.player1Rotation = player1Rotation;
        this.player2Position = player2Position;
        this.player2Rotation = player2Rotation;
        this.ballPosition = ballPosition;
        this.ballRotation = ballRotation;
    }
}
