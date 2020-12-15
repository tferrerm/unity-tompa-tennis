using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallHitReplayInfo
{
    private int playerId;
    private bool drive;
    private bool backhand;
    private bool fastDrive;
    private bool fastBackhand;
    private bool serve;
    
    public BallHitReplayInfo(int playerId)
    {
        this.playerId = playerId;
    }

    public BallHitReplayInfo(int playerId, bool drive, bool backhand, bool fastDrive, bool fastBackhand, bool serve)
    {
        this.playerId = playerId;
        this.drive = drive;
        this.backhand = backhand;
        this.fastDrive = fastDrive;
        this.fastBackhand = fastBackhand;
        this.serve = serve;
    }

    public BallHitReplayInfo(BallHitReplayInfo ballHitInfo)
    {
        playerId = ballHitInfo.playerId;
        drive = ballHitInfo.drive;
        backhand = ballHitInfo.backhand;
        fastDrive = ballHitInfo.fastDrive;
        fastBackhand = ballHitInfo.fastBackhand;
        serve = ballHitInfo.serve;
    }

    public int PlayerId
    {
        get => playerId;
        set => playerId = value;
    }

    public bool Drive
    {
        get => drive;
        set => drive = value;
    }

    public bool Backhand
    {
        get => backhand;
        set => backhand = value;
    }

    public bool Serve
    {
        get => serve;
        set => serve = value;
    }

    public bool FastDrive
    {
        get => fastDrive;
        set => fastDrive = value;
    }

    public bool FastBackhand
    {
        get => fastBackhand;
        set => fastBackhand = value;
    }

    public void Reset()
    {
        serve = false;
        drive = false;
        backhand = false;
        fastDrive = false;
        fastBackhand = false;
    }
}
