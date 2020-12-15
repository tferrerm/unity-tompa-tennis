using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReplayManager : MonoBehaviour
{
    public const float MaxRecordingTime = 15f;
    public bool isRecording;
    public bool isPlayingReplay;
    
    public List<RecordedReplayInfo> recordedReplayInfo;
    private int _recordingLimit;

    private Player _player;
    private AIPlayer _aiPlayer;
    public Ball ball;
    
    private BallHitReplayInfo player1BallHitInfo;
    private BallHitReplayInfo player2BallHitInfo;
    
    // Start is called before the first frame update
    void Start()
    {
        _recordingLimit = (int) Mathf.Round(MaxRecordingTime * (1f / Time.fixedDeltaTime));
        recordedReplayInfo = new List<RecordedReplayInfo>(_recordingLimit);

        var gameManager = GetComponent<GameManager>();
        _player = gameManager.player;
        _aiPlayer = gameManager.aiPlayer;
        
        player1BallHitInfo = new BallHitReplayInfo(_player.playerId);
        player2BallHitInfo = new BallHitReplayInfo(_aiPlayer.playerId);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    private void FixedUpdate()
    {
        if (isRecording)
        {
            RecordPositions();
            ResetBallHitInfo(); // TODO check if necessary
        }
        else if (isPlayingReplay && recordedReplayInfo.Count > 0)
        {
            PlayRecording();
        }
    }

    private void RecordPositions()
    {
        var playerTransform = _player.transform;
        var aiPlayerTransform = _aiPlayer.transform;
        var ballTransform = ball.transform;
        
        recordedReplayInfo.Add(new RecordedReplayInfo(
                playerTransform.position, playerTransform.rotation, player1BallHitInfo,
                aiPlayerTransform.position, aiPlayerTransform.rotation, player2BallHitInfo,
                ballTransform.position, ballTransform.rotation
            ));
        
        if (recordedReplayInfo.Count > _recordingLimit)
        {
            recordedReplayInfo.RemoveAt(0);
        }
    }

    public void InitializeReplay()
    {
        isRecording = false;
        isPlayingReplay = true;
        _player.InitializeRecordingPlay();
        _aiPlayer.InitializeRecordingPlay();
    }

    // TODO: fix animation when hitting ball. Store some hitting Ball variable in RecordedReplayInfo
    private void PlayRecording()
    {
        var recordedInfo = recordedReplayInfo[0];
        
        _player.ReplayMove(recordedInfo.player1Position, recordedInfo.player1Rotation, recordedInfo.player1BallHitInfo);
        _aiPlayer.ReplayMove(recordedInfo.player2Position, recordedInfo.player2Rotation, recordedInfo.player2BallHitInfo);
        ball.ReplayMove(recordedInfo.ballPosition, recordedInfo.ballRotation);
        
        recordedReplayInfo.RemoveAt(0);
    }
    
    public void StopReplay()
    {
        isPlayingReplay = false;
        _player.StopRecordingPlay();
        _aiPlayer.StopRecordingPlay();
    }
    
    public void SetPlayerDriveHit(int playerId, bool isFastHit)
    {
        if (playerId == _player.playerId)
        {
            if (isFastHit)
                player1BallHitInfo.FastDrive = true;
            else
                player1BallHitInfo.Drive = true;
        }
        else
        {
            if (isFastHit)
                player2BallHitInfo.FastDrive = true;
            else
                player2BallHitInfo.Drive = true;
        }
    }
    
    public void SetPlayerBackhandHit(int playerId, bool isFastHit)
    {
        if (playerId == _player.playerId)
        {
            if (isFastHit)
                player1BallHitInfo.FastBackhand = true;
            else
                player1BallHitInfo.Backhand = true;
        }
        else
        {
            if (isFastHit)
                player2BallHitInfo.FastBackhand = true;
            else
                player2BallHitInfo.Backhand = true;
        }
    }
    
    public void SetPlayerServeHit(int playerId)
    {
        if (playerId == _player.playerId)
        {
            player1BallHitInfo.Serve = true;
        }
        else
        {
            player2BallHitInfo.Serve = true;
        }
    }
    
    private void ResetBallHitInfo()
    {
        player1BallHitInfo.Reset();
        player2BallHitInfo.Reset();
    }
}
