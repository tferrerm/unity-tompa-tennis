using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecordedReplayInfo
{
    public Vector3 player1Position;
    public Quaternion player1Rotation;
    public BallHitReplayInfo player1BallHitInfo;
    public Vector3 player2Position;
    public Quaternion player2Rotation;
    public BallHitReplayInfo player2BallHitInfo;
    public Vector3 ballPosition;
    public Quaternion ballRotation;

    public RecordedReplayInfo(
        Vector3 player1Position, Quaternion player1Rotation, BallHitReplayInfo player1BallHitInfo,
        Vector3 player2Position, Quaternion player2Rotation, BallHitReplayInfo player2BallHitInfo,
        Vector3 ballPosition, Quaternion ballRotation)
    {
        this.player1Position = player1Position;
        this.player1Rotation = player1Rotation;
        this.player1BallHitInfo = new BallHitReplayInfo(player1BallHitInfo);
        this.player2Position = player2Position;
        this.player2Rotation = player2Rotation;
        this.player2BallHitInfo = new BallHitReplayInfo(player2BallHitInfo);
        this.ballPosition = ballPosition;
        this.ballRotation = ballRotation;
    }
}
