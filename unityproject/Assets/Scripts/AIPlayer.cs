using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class AIPlayer : MonoBehaviour
{
    public int playerId;
    public string playerName;

    private CharacterController _characterController;

    private AudioSource _audioSource;
    
    private Animator _animator;
    private int _strafeHash;
    private int _forwardHash;
    private int _serviceTriggerHash;
    private int _serviceEndHash;
    private int _driveHash;
    private int _backhandHash;
    private int _cheerHash;
    private int _defeatedHash;
    private int _fastDriveHash;
    private int _fastBackhandHash;

    private float _moveLeftRightValue;
    private float _moveUpDownValue;

    public Ball ball;
    public GameObject attachedBall;
    public Transform attachedBallParent;
    public Collider ballCollider;
    [HideInInspector] public bool ballInsideHitZone;
    private HitMethod? _hitMethod;
    private HitDirectionHorizontal? _hitDirectionHoriz;
    private HitDirectionVertical? _hitDirectionVert;
    public Transform hitBallSpawn;
    public Transform hitServiceBallSpawn;
    public Transform driveVolleyHitBallSpawn;
    public Transform backhandVolleyHitBallSpawn;

    public GameManager gameManager;
    private CourtManager _courtManager;
    private PointManager _pointManager;
    private SoundManager _soundManager;
    private TennisVariables _tv;
    
    private bool _serveBallReleased;
    
    private bool _sprinting = false;
    private bool _movementBlocked;
    
    private Vector3? _target;
    private const float HitDistanceToBall = 2.5f; // Z-Distance from ball target where AI will go
    private Vector3 _backCenter; // Z-Position where AI will return after hitting ball
    private bool _movingToCenter;
    private const float DepthMovementLimit = 50f;
    private const float LateralMovementLimit = 25f;

    private const float ServiceWaitTime = 1f; // Waiting time before serve after point reset
    private const float MaxReactionTime = 1f; // Waiting time before AI starts moving
    private float _reactionWaitTimer;

    public PredictionBall predictionBall;
    private const float ReachedTargetEpsilon = 0.1f;
    
    private bool _volleyModeActivated;
    private readonly Vector3 _volleyCenterPos = new Vector3(5f, -3.067426f, 0f);
    
    public List<RecordedReplayInfo> recordedReplayInfo;
    private int _recordingLimit;

    void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _audioSource = GetComponent<AudioSource>();
        _animator = GetComponent<Animator>();
        _recordingLimit = TennisVariables.RecordingLimit();
        recordedReplayInfo = new List<RecordedReplayInfo>(_recordingLimit);
    }

    void Start()
    {
        _courtManager = gameManager.courtManager;
        _pointManager = gameManager.pointManager;
        _soundManager = gameManager.soundManager;
        _tv = gameManager.tennisVariables;
        
        CalculateAnimatorHashes();
        
        _reactionWaitTimer = Random.Range(0f, MaxReactionTime);
        _backCenter = new Vector3(39.75f, -3.067426f, 0);
    }

    private void CalculateAnimatorHashes()
    {
        _strafeHash = Animator.StringToHash("Strafe");
        _forwardHash = Animator.StringToHash("Forward");
        _serviceTriggerHash = Animator.StringToHash("Service Trigger");
        _serviceEndHash = Animator.StringToHash("Service End");
        _driveHash = Animator.StringToHash("Drive Trigger");
        _backhandHash = Animator.StringToHash("Backhand Trigger");
        _cheerHash = Animator.StringToHash("Cheer Trigger");
        _defeatedHash = Animator.StringToHash("Defeated Trigger");
        _fastDriveHash = Animator.StringToHash("Fast Drive Trigger");
        _fastBackhandHash = Animator.StringToHash("Fast Backhand Trigger");
    }

    private void FixedUpdate()
    {
        if (TennisVariables.isRecording)
        {
            RecordPosition();
        }
        else if (TennisVariables.isPlayingReplay && recordedReplayInfo.Count > 0)
        {
            PlayRecording();
        }
    }
    
    private void RecordPosition()
    {
        var tf = transform;
        recordedReplayInfo.Add(new RecordedReplayInfo(tf.position, tf.rotation));
        if (recordedReplayInfo.Count > _recordingLimit)
        {
            recordedReplayInfo.RemoveAt(0);
        }
        
    }
    
    // TODO: fix animation when hitting ball. Store some hitting Ball variable in RecordedReplayInfo
    private void PlayRecording()
    {
        var recordedInfo = recordedReplayInfo[0];
        var distance = recordedInfo.position - transform.position;
        _animator.SetFloat(_strafeHash, distance.z / Time.fixedDeltaTime);
        _animator.SetFloat(_forwardHash, distance.x / Time.fixedDeltaTime);
        transform.position = recordedInfo.position;
        transform.rotation = recordedInfo.rotation;
        recordedReplayInfo.RemoveAt(0);
    }
    
    public void InitializeRecordingPlay()
    {
        _characterController.enabled = false;
        _animator.SetFloat(_strafeHash, 0);
        _animator.SetFloat(_forwardHash, 0);
    }

    public void StopRecordingPlay()
    {
        _characterController.enabled = true;
        _animator.SetFloat(_strafeHash, 0);
        _animator.SetFloat(_forwardHash, 0);
    }

    void Update()
    {
        if (TennisVariables.isPlayingReplay)
            return;
        
        if (_movingToCenter && _target != null)
        {
            // Move player to center
            MoveToTarget(false);
        } 
        else if (_target != null)
        {
            
            // Reset if target behind player
            if (!ballInsideHitZone && ball.GetPosition().x > transform.position.x - _tv.BallColliderFrontDelta)
            {
                ResetTargetMovementVariables();
                return;
            }

            // Wait for player reaction after opponent hits ball
            if (_reactionWaitTimer > 0)
            {
                _reactionWaitTimer -= Time.deltaTime;
                return;
            }
            
            // Move player to target position
            MoveToTarget(true);
        }

        
        if (CanHitBall())
        {
            SelectHitMethod();
        }
        // reset waiting for ball
        // hit ball
    }

    private void MoveToTarget(bool targetIsBall)
    {
        Move();
        if (Vector3.Distance(_target.GetValueOrDefault(), transform.position) < ReachedTargetEpsilon)
        {
            // Reached target
            ResetTargetMovementVariables();
            if (!targetIsBall)
            {
                _target = null;
                _movingToCenter = false;
            }
        }
    }

    public void ResetTargetMovementVariables()
    {
        _target = null;
        _animator.SetFloat(_strafeHash, 0);
        _animator.SetFloat(_forwardHash, 0);
        _reactionWaitTimer = Random.Range(0f, MaxReactionTime);
    }
    
    private void Move()
    {
        _characterController.SimpleMove(Vector3.zero);
        _target = new Vector3(_target.GetValueOrDefault().x, transform.position.y, _target.GetValueOrDefault().z);
        
        var pos = transform.position;
        var speed = (_sprinting ? _tv.SprintSpeed : _tv.RunSpeed);
        var step =  speed * Time.deltaTime;
        var frameTarget = Vector3.MoveTowards(pos, _target.Value, step);

        var move = frameTarget - pos;

        _animator.SetFloat(_strafeHash, _moveLeftRightValue * -1);
        _animator.SetFloat(_forwardHash, _moveUpDownValue * -1);
        _characterController.Move(move);
    }
    
    private void SelectHitMethod()
    {
        var ballPosition = ball.GetPosition();
        var ballSpeed = ball.GetSpeed();
        if (ballPosition.z >= transform.position.z) // Should take into account ball direction
        {
            if (ballSpeed < _tv.FastHitAnimationThresholdSpeed)
            {
                _animator.SetTrigger(_driveHash);
            }
            else
            {
                PlayerGrunt();
                _animator.SetTrigger(_fastDriveHash);
            }
            _hitMethod = HitMethod.Drive;
        }
        else
        {
            if (ballSpeed < _tv.FastHitAnimationThresholdSpeed)
            {
                _animator.SetTrigger(_backhandHash);
            }
            else
            {
                PlayerGrunt();
                _animator.SetTrigger(_fastBackhandHash);
            }
            _hitMethod = HitMethod.Backhand;
        }

        _hitDirectionHoriz = (HitDirectionHorizontal)Random.Range(0, 3);
        _hitDirectionVert = (HitDirectionVertical)Random.Range(0, 2);
        _movementBlocked = true;
    }
    
    public void HitBall() // Called as animation event
    {
        _pointManager.SetPlayerHitBall(playerId);
        _pointManager.HandleBallBounce(null);
        
        var isVolley = _pointManager.PositionInVolleyArea(playerId, transform.position.x);
        ball.Teleport(isVolley ? (_hitMethod == HitMethod.Drive ? 
            driveVolleyHitBallSpawn.position : backhandVolleyHitBallSpawn.position) : hitBallSpawn.position);

        var targetPosition = _courtManager.GetHitTargetPosition(playerId, _hitDirectionVert, _hitDirectionHoriz);
        targetPosition = RandomizeBallTarget(targetPosition, _tv.BallHitTargetRadius);
        _soundManager.PlayRacquetHit(_audioSource);
        
        float speed;
        float speedYAtt;
        if (_hitDirectionVert == HitDirectionVertical.Deep)
        {
            if (isVolley)
            {
                speed = _tv.DeepVolleyHitSpeed;
                speedYAtt = _tv.DeepVolleyHitYAttenuation;
            }
            else
            {
                speed = _tv.DeepHitSpeed;
                speedYAtt = _tv.DeepHitYAttenuation;
            }
        }
        else
        {
            if (isVolley)
            {
                speed = _tv.DropshotVolleyHitSpeed;
                speedYAtt = _tv.DropshotVolleyHitYAttenuation;
            }
            else
            {
                speed = _tv.DropshotHitSpeed;
                speedYAtt = _tv.DropshotHitYAttenuation;
            }
        }
        
        ball.HitBall(targetPosition, speed, _hitDirectionVert == HitDirectionVertical.Dropshot, true, speedYAtt);
        _hitDirectionVert = null;
        _hitDirectionHoriz = null;
        _hitMethod = null;
    }
    
    private Vector3 RandomizeBallTarget(Vector3 posEnd, float randomRadius)
    {
        return new Vector3(
            posEnd.x + Random.Range(-randomRadius, randomRadius),
            posEnd.y,
            posEnd.z + Random.Range(-randomRadius, randomRadius));
    }

    private void CheckServiceStatus()
    {
        var currentStateHash = _animator.GetCurrentAnimatorStateInfo(0).shortNameHash;
        if (currentStateHash == _serviceEndHash && !_serveBallReleased)
        {
            ball.transform.position = attachedBallParent.transform.position;
            SwitchBallType(false);
        }
    }

    private void Step(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5)
            _soundManager.PlayFootstep(_audioSource);
    }


    public void UpdateTargetPosition(Vector3 startPos)
    {
        ResetTargetMovementVariables();
        
        if (!_volleyModeActivated && Random.Range(0f, 1f) < _tv.AIVolleyModeProbability) // Active volley mode
        {
            _volleyModeActivated = true;
        }

        var closestPoint = RunBallPrediction(startPos); // Vector2
        
        var targetX = closestPoint.x > 0 ? Mathf.Min(closestPoint.x, DepthMovementLimit) : 0f;
        
        var targetZ = closestPoint.y + (closestPoint.y > startPos.z ? -1 : 1) * HitDistanceToBall;
        targetZ = targetZ > -LateralMovementLimit ? Math.Min(targetZ, LateralMovementLimit) : -LateralMovementLimit;
        
        _target = new Vector3(targetX,0, targetZ);
        
        _moveLeftRightValue = _target.Value.z - transform.position.z > 0 ? 1 : -1;
        _moveUpDownValue = _target.Value.x - transform.position.x > 0 ? 1 : -1;
        _movingToCenter = false;
    }

    // Ball entered collision zone (sphere) and is in front of the player
    private bool CanHitBall()
    {
        return ballInsideHitZone && _hitMethod == null && !_movementBlocked && _pointManager.CanHitBall(playerId) &&
               ball.transform.position.x < transform.position.x - _tv.BallColliderFrontDelta;
    }
    
    private void ResetHittingBall() // Called as animation event
    {
        _movingToCenter = true;
        _target = _volleyModeActivated ? _volleyCenterPos : _backCenter;
        _moveUpDownValue = _target.Value.x - transform.position.x > 0 ? 1 : -1;
        _moveLeftRightValue = _target.Value.z - transform.position.z > 0 ? 1 : -1;
        _movementBlocked = false;
    }
    
    private void ResetHittingServiceBall() // Called as animation event
    {
        
    }
    
    public IEnumerator StartService()
    {
        TennisVariables.isRecording = true;
        yield return new WaitForSeconds(ServiceWaitTime);
        
        _animator.SetTrigger(_serviceTriggerHash);
        _hitMethod = HitMethod.Serve;
        _hitDirectionHoriz = (HitDirectionHorizontal)Random.Range(0, 3);
    }
    
    private void TossServiceBall() // Called as animation event
    {
        ball.Teleport(attachedBallParent.position);
        SwitchBallType(false);
        ball.HitBall(hitServiceBallSpawn.position, _tv.ServiceTossSpeed, false, false);
    }
    
    public void SwitchBallType(bool attachBall) 
    {
        _serveBallReleased = !attachBall;
        attachedBall.SetActive(attachBall);
        ball.gameObject.SetActive(!attachBall);
    }
    
    private void HitServiceBall() // Called as animation event
    {
        var targetPosition = _courtManager.GetServiceTargetPosition(playerId, _hitDirectionHoriz);
        targetPosition = RandomizeBallTarget(targetPosition, _tv.BallServeTargetRadius);
        ball.HitBall(targetPosition, _tv.ServiceSpeed, false, true, _tv.ServiceYAttenuation);
        _soundManager.PlayService(_audioSource);
        _hitDirectionHoriz = null;
        _hitMethod = null;
    }
    
    public void Cheer()
    {
        _animator.SetTrigger(_cheerHash);
    }
    
    public void Defeated()
    {
        _animator.SetTrigger(_defeatedHash);
    }
    
    public void ToggleCharacterController()
    {
        _characterController.enabled = !_characterController.enabled;
    }

    private void PlayerGrunt()
    {
        var rand = Random.value;
        if (rand > 0.85f)
            _soundManager.PlayGrunt(_audioSource);
    }
    
    private Vector2 RunBallPrediction(Vector3 startPos)
    {
        predictionBall.Teleport(startPos);
        Vector2 firstBounce = Vector2.zero, secondBounce = Vector2.zero;
        
        predictionBall.SetupBall(ball.BallPhysics, ball.BallInfo, ball.IsDropshot);

        var bouncedOnce = false;
        var bouncedTwice = false;
        
        while (!bouncedTwice)
        {
            var bounced = predictionBall.UpdateBall();
            
            if (!bounced) continue;

            var bouncePos = predictionBall.GetPosition();
            
            if (!bouncedOnce)
            {
                bouncedOnce = true;
                firstBounce = new Vector2(bouncePos.x,bouncePos.z);
            }
            else
            {
                bouncedTwice = true;
                secondBounce = new Vector2(bouncePos.x,bouncePos.z);
            }
        }

        var position = transform.position;
        return _volleyModeActivated ? 
            ClosestPoint(new Vector2(position.x, position.y), startPos, secondBounce) : 
            ClosestPoint(new Vector2(position.x, position.y), firstBounce, secondBounce);
    }

    private Vector2 ClosestPoint(Vector2 position, Vector2 firstBounce, Vector2 secondBounce)
    {
        var heading = (secondBounce - firstBounce);
        var magnitudeMax = heading.magnitude;
        heading.Normalize();

        //Do projection from the point but clamp it
        var lhs = position - firstBounce;
        var dotP = Vector2.Dot(lhs, heading);
        dotP = Mathf.Clamp(dotP, 0f, magnitudeMax);
        return firstBounce + heading * dotP;
    }

    public bool VolleyModeActivated
    {
        set => _volleyModeActivated = value;
    }

    public bool MovingToCenter
    {
        set => _movingToCenter = value;
    }
}
