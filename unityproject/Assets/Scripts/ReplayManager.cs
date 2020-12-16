using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

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
    
    private BallHitReplayInfo player1BallHitInfo;
    private BallHitReplayInfo player2BallHitInfo;

    private GameObject mainCamera;
    public GameObject replayCamera;
    public Transform[] replayCameraPOVUp;
    public Transform[] replayCameraPOVDown;
    private const int Left = 0;
    private const int Right = 1;
    
    [HideInInspector] public Vector3 lastBallHitPosition;

    private GameObject _scoreboard;
    
    private PointManager _pointManager;
    public InputAction skipReplay;

    // Start is called before the first frame update
    void Start()
    {
        _recordingLimit = (int) Mathf.Round(MaxRecordingTime * (1f / Time.fixedDeltaTime));
        recordedReplayInfo = new List<RecordedReplayInfo>(_recordingLimit);

        var gameManager = GetComponent<GameManager>();
        _player = gameManager.player;
        _aiPlayer = gameManager.aiPlayer;
        _pointManager = gameManager.pointManager;
        
        player1BallHitInfo = new BallHitReplayInfo(_player.playerId);
        player2BallHitInfo = new BallHitReplayInfo(_aiPlayer.playerId);

        mainCamera = GameObject.FindWithTag("MainCamera");
        _scoreboard = GameObject.FindGameObjectWithTag("Scoreboard");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDisable()
    {
        skipReplay.Disable();
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
            if (skipReplay.triggered)
            {
                _pointManager.StopWaitForNextServe();
            }
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
        mainCamera.SetActive(false);
        replayCamera.SetActive(true);
        _player.InitializeRecordingPlay();
        _aiPlayer.InitializeRecordingPlay();
        _scoreboard.SetActive(false);
        skipReplay.Enable();
    }

    // TODO: fix animation when hitting ball. Store some hitting Ball variable in RecordedReplayInfo
    private void PlayRecording()
    {
        var recordedInfo = recordedReplayInfo[0];

        //MoveReplayCamera(recordedInfo.ballPosition);
        
        _player.ReplayMove(recordedInfo.player1Position, recordedInfo.player1Rotation, recordedInfo.player1BallHitInfo);
        _aiPlayer.ReplayMove(recordedInfo.player2Position, recordedInfo.player2Rotation, recordedInfo.player2BallHitInfo);
        ball.ReplayMove(recordedInfo.ballPosition, recordedInfo.ballRotation);
        
        recordedReplayInfo.RemoveAt(0);
    }

    private void MoveReplayCamera(Vector3 ballPosition)
    {
        var cameraPos = replayCamera.transform.position;
        replayCamera.transform.position = new Vector3(cameraPos.x, cameraPos.y, ballPosition.z);
    }

    public void StopReplay()
    {
        skipReplay.Disable();
        // isRecording is set to true when service takes place
        isPlayingReplay = false;
        mainCamera.SetActive(true);
        replayCamera.SetActive(false);
        _player.StopRecordingPlay();
        _aiPlayer.StopRecordingPlay();
        _scoreboard.SetActive(true);
        recordedReplayInfo.Clear();
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

    public void SetCameraPosition(int pointWinnerId)
    {
        var ballPosition = ball.GetPosition();

        var parallelHit = lastBallHitPosition.z * ballPosition.z > 0;
        var isEvenSide = ballPosition.x * ballPosition.z > 0;
        var ballUpZAxis = pointWinnerId == _player.playerId? 
            (isEvenSide ? Left : Right) : // Player won point
            (isEvenSide ? (parallelHit? Right : Left) : (parallelHit? Left : Right)); // AI Player won point
        var upPOV = replayCameraPOVUp[ballUpZAxis];
        var downPOV = replayCameraPOVDown[(ballUpZAxis + (parallelHit? 0 : 1)) % 2];
        
        var useUpPOV = Random.Range(0, 2) == 0;
        replayCamera.transform.position = useUpPOV? upPOV.position : downPOV.position;
        replayCamera.transform.rotation = useUpPOV? upPOV.rotation : downPOV.rotation;
    }
}
