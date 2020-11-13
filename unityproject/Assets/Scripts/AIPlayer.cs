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
    private int _serviceStartHash;
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
    
    private bool _serveBallReleased = false;
    
    private bool _sprinting = false;
    private bool _movementBlocked;
    
    private Vector3? _target = null;
    private float _lastDifference = float.MaxValue;
    private const float HitDistanceToBall = 3f; // Z-Distance from ball target where AI will go
    //private Vector3 waitingForBallPos = new Vector3(39.75f, -3.067426f, 1.59f);
    private Vector3 _backCenter; // Z-Position where AI will return after hitting ball
    private bool _movingToCenter = false;
    private const float DropShotBounceDeltaTarget = 1;
    private const float BackShotBounceDeltaTarget = 6;

    private const float ServiceWaitTime = 1f; // Waiting time before serve after point reset
    private const float MaxReactionTime = 0.75f; // Waiting time before AI starts moving
    private float _reactionWaitTimer;

    void Start()
    {
        _courtManager = gameManager.courtManager;
        _pointManager = gameManager.pointManager;
        _soundManager = gameManager.soundManager;
        _characterController = GetComponent<CharacterController>();
        _audioSource = GetComponent<AudioSource>();
        _animator = GetComponent<Animator>();
        CalculateAnimatorHashes();
        _reactionWaitTimer = Random.Range(0f, MaxReactionTime);
        
        _tv = gameManager.tennisVariables;
        _backCenter = new Vector3(39.75f, -3.067426f, 1.59f);
    }

    private void CalculateAnimatorHashes()
    {
        _strafeHash = Animator.StringToHash("Strafe");
        _forwardHash = Animator.StringToHash("Forward");
        _serviceTriggerHash = Animator.StringToHash("Service Trigger");
        _serviceStartHash = Animator.StringToHash("Service Start");
        _serviceEndHash = Animator.StringToHash("Service End");
        _driveHash = Animator.StringToHash("Drive Trigger");
        _backhandHash = Animator.StringToHash("Backhand Trigger");
        _cheerHash = Animator.StringToHash("Cheer Trigger");
        _defeatedHash = Animator.StringToHash("Defeated Trigger");
        _fastDriveHash = Animator.StringToHash("Fast Drive Trigger");
        _fastBackhandHash = Animator.StringToHash("Fast Backhand Trigger");
    }
    
    void Update()
    {
        var currentDifference = _target != null ? 
            Mathf.Sqrt(Mathf.Pow(Mathf.Abs(transform.position.z - _target.Value.z), 2) + 
                       Mathf.Pow(Mathf.Abs(transform.position.x - _target.Value.x), 2)) : 0.0f;
        
        if (_movingToCenter)
        {
            // Move player to center
            MoveToTarget(currentDifference, false);
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
            MoveToTarget(currentDifference, true);
        }

        
        if (CanHitBall())
        {
            SelectHitMethod();
        }
        // reset waiting for ball
        // hit ball
    }

    private void MoveToTarget(float currentDifference, bool isBall)
    {
        Move();
        if (currentDifference > _lastDifference)
        {
            // Reached target
            ResetTargetMovementVariables();
            if (!isBall)
            {
                _target = null;
                _movingToCenter = false;
            }
        }
        else
        {
            _lastDifference = currentDifference;
        }
    }

    public void ResetTargetMovementVariables()
    {
        _target = null;
        _lastDifference = float.MaxValue;
        _animator.SetFloat(_strafeHash, 0);
        _animator.SetFloat(_forwardHash, 0);
        _reactionWaitTimer = Random.Range(0f, MaxReactionTime);
    }
    
    private void Move()
    {
        
        float dt = Time.deltaTime;
        Vector2 movingDir = new Vector2(_moveLeftRightValue, _moveUpDownValue);
        float manhattanNorm = Math.Abs(movingDir[0]) + Math.Abs(movingDir[1]);
        if (manhattanNorm == 0)
            manhattanNorm = 1;
        float spd = (_sprinting ? _tv.SprintSpeed : _tv.RunSpeed) * movingDir.magnitude;
        float dx = dt * (_moveUpDownValue < 0 ? _tv.BackSpeed : spd) * _moveUpDownValue / manhattanNorm;
        float dz = dt * spd * _moveLeftRightValue / manhattanNorm;

        _characterController.SimpleMove(Vector3.zero);
        
        Vector3 move = new Vector3(dx, 0, dz);

        _animator.SetFloat(_strafeHash, _moveLeftRightValue * -1);
        _animator.SetFloat(_forwardHash, _moveUpDownValue);
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
    
    void CheckServiceStatus()
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


    public void UpdateTargetPosition(Vector3 startPos, Vector3 ballTargetPos)
    {
        var xDiff = Mathf.Abs(ballTargetPos.x - startPos.x);
        var zDiff = Mathf.Abs(ballTargetPos.z - startPos.z);

        var ratio = zDiff / xDiff;

        var playerBallTargetXDiff = Mathf.Abs(transform.position.x - ballTargetPos.x);

        ResetTargetMovementVariables();

        var targetX = ballTargetPos.x + DropShotBounceDeltaTarget;
        var targetZ = ballTargetPos.z + (ballTargetPos.z > startPos.z ? 1 : -1) * (playerBallTargetXDiff) * ratio
                                      + (ballTargetPos.z > startPos.z ? -1 : 1) * HitDistanceToBall;
        
        _target = new Vector3(_backCenter.x,0, targetZ);
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
        _target = _backCenter;
        _moveUpDownValue = _target.Value.x - transform.position.x > 0 ? 1 : -1;
        _moveLeftRightValue = _target.Value.z - transform.position.z > 0 ? 1 : -1;
        _movementBlocked = false;
    }
    
    private void ResetHittingServiceBall() // Called as animation event
    {
        
    }
    
    public IEnumerator StartService()
    {
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
}
