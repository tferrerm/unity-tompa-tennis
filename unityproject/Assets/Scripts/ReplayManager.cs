using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class ReplayManager : MonoBehaviour
{
    public bool isRecording;
    public bool isPlayingReplay;
    private int _replayInfoCounter;
    
    public List<RecordedReplayInfo> recordedReplayInfo;

    private Player _player;
    private AIPlayer _aiPlayer;
    public Ball ball;
    
    private BallHitReplayInfo player1BallHitInfo;
    private BallHitReplayInfo player2BallHitInfo;
    
    private bool _playerWonPoint;
    private int _celebrationTriggerId;

    private GameObject mainCamera;
    public GameObject replayCamera;
    public Transform[] replayCameraPOVUp;
    public Transform[] replayCameraPOVDown;
    private const int Left = 0;
    private const int Right = 1;
    
    [HideInInspector] public Vector3 lastBallHitPosition;

    private int _lastHitCheckpointId = -1;
    private int _checkpointCounter;
    private const float FramePercentageToShowBeforeInitialCheckpoint = 0.4f;
    private const float MinTimeScale = 0.75f;
    private const float TimeScaleMultiplier = 0.975f;
    
    private int _cameraRotationEndId;
    private const float CameraRotationInitialSpeed = 0.05f;
    private float _cameraRotationSpeed;
    private float _cameraRotationStopMultiplier = 0.95f;

    private GameObject _scoreboard;
    
    private PointManager _pointManager;
    public InputAction skipReplay;

    private SoundManager _soundManager;
    private AudioSource _musicAudioSource;

    // Start is called before the first frame update
    void Start()
    {
        recordedReplayInfo = new List<RecordedReplayInfo>();

        var gameManager = GetComponent<GameManager>();
        _player = gameManager.player;
        _aiPlayer = gameManager.aiPlayer;
        _pointManager = gameManager.pointManager;
        _soundManager = gameManager.soundManager;
        _musicAudioSource = replayCamera.GetComponent<AudioSource>();
        
        player1BallHitInfo = new BallHitReplayInfo(_player.playerId);
        player2BallHitInfo = new BallHitReplayInfo(_aiPlayer.playerId);

        mainCamera = GameObject.FindWithTag("MainCamera");
        _scoreboard = GameObject.FindGameObjectWithTag("Scoreboard");
    }
    
    private void OnDisable()
    {
        skipReplay.Disable();
    }

    private void Update()
    {
        if (isPlayingReplay && skipReplay.triggered)
        {
            _pointManager.StopWaitForNextServe();
        }
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
        
        recordedReplayInfo.Add(new RecordedReplayInfo(_replayInfoCounter,
                playerTransform.position, playerTransform.rotation, player1BallHitInfo,
                aiPlayerTransform.position, aiPlayerTransform.rotation, player2BallHitInfo,
                ballTransform.position, ballTransform.rotation
            ));
        
        
        _replayInfoCounter++;
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
        _cameraRotationSpeed = CameraRotationInitialSpeed;
        _replayInfoCounter = 0;
        ball.ReplayMove(recordedReplayInfo[0].ballPosition, recordedReplayInfo[0].ballRotation);
        ball.trail.Clear();
        _soundManager.PlayReplayMusic();
    }

    private void PlayRecording()
    {
        var recordedInfo = recordedReplayInfo[0];

        if (recordedInfo.id > _lastHitCheckpointId && Time.timeScale > MinTimeScale)
        {
            Time.timeScale *= TimeScaleMultiplier;
        }

        MoveReplayCamera(recordedInfo.id, recordedInfo.ballPosition);

        _player.ReplayMove(recordedInfo.player1Position, recordedInfo.player1Rotation, recordedInfo.player1BallHitInfo,
            recordedInfo.id == _celebrationTriggerId, _playerWonPoint);
        _aiPlayer.ReplayMove(recordedInfo.player2Position, recordedInfo.player2Rotation, recordedInfo.player2BallHitInfo,
            recordedInfo.id == _celebrationTriggerId, !_playerWonPoint);
        ball.ReplayMove(recordedInfo.ballPosition, recordedInfo.ballRotation);
        
        recordedReplayInfo.RemoveAt(0);
    }

    private void MoveReplayCamera(int replayInfoId, Vector3 ballPosition)
    {
        if (replayInfoId > _cameraRotationEndId)
        {
            // Camera starts to slow down after point has been won
            _cameraRotationSpeed *= _cameraRotationStopMultiplier;
        }

        Vector3 targetDirection = ballPosition - replayCamera.transform.position;
        float singleStep = _cameraRotationSpeed * Time.fixedDeltaTime;
        Vector3 newDirection = Vector3.RotateTowards(replayCamera.transform.forward, targetDirection, singleStep, 0.0f);
        replayCamera.transform.rotation = Quaternion.LookRotation(newDirection);
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
        _aiPlayer.ResetTargetMovementVariables();
        _scoreboard.SetActive(true);
        recordedReplayInfo.Clear();
        _lastHitCheckpointId = -1;
        _checkpointCounter = 0;
        Time.timeScale = 1f;
        StartCoroutine(FadeOutAudioSource.StartFade(_musicAudioSource, 100f, 0));
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
        _cameraRotationEndId = _replayInfoCounter;
            
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

        _playerWonPoint = pointWinnerId == _player.playerId;
        _celebrationTriggerId = _replayInfoCounter;
    }

    public void SetHitCheckpoint()
    {
        if (_checkpointCounter > 2)
        {
            var removeIndex = _lastHitCheckpointId - recordedReplayInfo[0].id;
            
            // If two or more hits were recorded, start replay a short time before the penultimate
            var framesToShowBeforeCheckpoint =
                (int) ((_lastHitCheckpointId - recordedReplayInfo[0].id) * FramePercentageToShowBeforeInitialCheckpoint);
            removeIndex -= framesToShowBeforeCheckpoint;
            
            recordedReplayInfo.RemoveRange(0, removeIndex);
        }
        _lastHitCheckpointId = _replayInfoCounter;
        _checkpointCounter++;
    }

    public float GetReplayTime()
    {
        return Time.fixedDeltaTime * recordedReplayInfo.Count;
    }
}
