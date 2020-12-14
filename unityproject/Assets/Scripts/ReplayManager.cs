using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReplayManager : MonoBehaviour
{
    public const float MaxRecordingTime = 10f;
    public bool isRecording;
    public bool isPlayingReplay;
    
    public List<RecordedReplayInfo> recordedReplayInfo;
    private int _recordingLimit;

    private Player _player;
    private AIPlayer _aiPlayer;
    public Ball ball;
    
    // Start is called before the first frame update
    void Start()
    {
        _recordingLimit = (int) Mathf.Round(MaxRecordingTime * (1f / Time.fixedDeltaTime));
        recordedReplayInfo = new List<RecordedReplayInfo>(_recordingLimit);

        var gameManager = GetComponent<GameManager>();
        _player = gameManager.player;
        _aiPlayer = gameManager.aiPlayer;
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
                playerTransform.position, playerTransform.rotation,
                aiPlayerTransform.position, aiPlayerTransform.rotation,
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
        
        _player.ReplayMove(recordedInfo.player1Position, recordedInfo.player1Rotation);
        _aiPlayer.ReplayMove(recordedInfo.player2Position, recordedInfo.player2Rotation);
        ball.ReplayMove(recordedInfo.ballPosition, recordedInfo.ballRotation);
        
        recordedReplayInfo.RemoveAt(0);
    }
    
    public void StopReplay()
    {
        isPlayingReplay = false;
        _player.StopRecordingPlay();
        _aiPlayer.StopRecordingPlay();
    }
}
