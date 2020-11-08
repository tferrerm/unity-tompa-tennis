using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPlayer : MonoBehaviour
{
    public int playerId;
    public string playerName;
    
    public float runSpeed = 8;
    public float sprintSpeed = 10;
    public float backSpeed = 5.5f;

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
    private float serviceTossSpeed = 20f;
    private float maxXBallHit;

    public GameManager gameManager;
    private PointManager _pointManager;
    private SoundManager _soundManager;
    
    private bool _serveBallReleased = false;

    private int desiredDistance = 1;
    
    private float? _targetZ = null;
    private bool _sprinting = false;
    private float _lastZDifference = float.MaxValue;
    private const float HitDistanceToBall = 3f;
    private bool waitingForBall;

    void Start()
    {
        _pointManager = gameManager.pointManager;
        _soundManager = gameManager.soundManager;
        _characterController = GetComponent<CharacterController>();
        _audioSource = GetComponent<AudioSource>();
        _animator = GetComponent<Animator>();
        CalculateAnimatorHashes();
        maxXBallHit = transform.position.x - 2;
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
    }
    
    void Update()
    {
        if (_targetZ != null)
        {
            if (!ballInsideHitZone && ball.GetPosition().x > maxXBallHit)
            {
                ResetTargetMovementVariables();
                return;
            }
            
            var currentZDifference = Mathf.Abs(transform.position.z - _targetZ.Value);
            Move();
            if (currentZDifference > _lastZDifference)
            {
                // Reached target
                ResetTargetMovementVariables();
                waitingForBall = true;
            }
            else
            {
                _lastZDifference = currentZDifference;
            }
        }
        
        // CHECK BALL INSIDE HIT ZONE AND WAITING FOR BALL
        //SelectHitMethod
        // reset waiting for ball
        // hit ball
    }

    private void ResetTargetMovementVariables()
    {
        _targetZ = null;
        _lastZDifference = float.MaxValue;
        _animator.SetFloat(_strafeHash, 0);
        _animator.SetFloat(_forwardHash, 0);
    }

    // private void Move()
    // {
    //     if (!ball.gameObject.activeSelf)
    //         return;
    //     
    //     var position = transform.position;
    //     if (position.x <= desiredDistance)
    //         StayInNet();
    //     else if (DistanceToBall().magnitude <= desiredDistance)
    //         PrepareToHit();
    //     else
    //         MoveTowardsBall();
    // }
    
    private void Move()
    {
        
        float dt = Time.deltaTime;
        Vector2 movingDir = new Vector2(_moveLeftRightValue, _moveUpDownValue);
        float manhattanNorm = Math.Abs(movingDir[0]) + Math.Abs(movingDir[1]);
        if (manhattanNorm == 0)
            manhattanNorm = 1;
        float spd = (_sprinting ? sprintSpeed : runSpeed) * movingDir.magnitude;
        float dx = dt * (_moveUpDownValue < 0 ? backSpeed : spd) * _moveUpDownValue / manhattanNorm;
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
        if (ballPosition.z >= transform.position.z) // Should take into account ball direction
        {
            _animator.SetTrigger(_driveHash);
            _hitMethod = HitMethod.Drive;
        }
        else
        {
            _animator.SetTrigger(_backhandHash);
            _hitMethod = HitMethod.Backhand;
        }
        _hitDirectionHoriz = HitDirectionHorizontal.Center; // Default value if no keys pressed
        _hitDirectionVert = HitDirectionVertical.Back;
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
    
    void SwitchBallType(bool attachBall) 
    {
        _serveBallReleased = !attachBall;
        attachedBall.SetActive(attachBall);
        ball.gameObject.SetActive(!attachBall);
    }

    private void StayInNet()
    {
        
    }

    private void PrepareToHit()
    {
        
    }

    private void MoveTowardsBall()
    {
        float dt = Time.deltaTime;
        var distance = DistanceToBall();
        float spd = (distance.magnitude > 5 ? sprintSpeed : runSpeed);
        float dx = dt * (distance.x < 0 ? backSpeed : spd) * Math.Sign(distance.x * -1);
        float dz = dt * spd * Math.Sign(distance.z * -1);
        if (Math.Abs(distance.z) < 0.25f)
            dz = 0;

        _characterController.SimpleMove(Vector3.zero);
        
        Vector3 move = new Vector3(dx, 0, dz);
        
        Debug.Log($"MOVE {move}");

        _animator.SetFloat(_strafeHash, dx);
        _animator.SetFloat(_forwardHash, dz);
        _characterController.Move(move);
    }

    private Vector3 DistanceToBall()
    {
        return transform.position - ball.transform.position;
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
        
        _targetZ = ballTargetPos.z + (ballTargetPos.z > startPos.z ? 1 : -1) * playerBallTargetXDiff * ratio 
                                   + (ballTargetPos.z > startPos.z ? -1 : 1) * HitDistanceToBall;
        _moveUpDownValue = 0;
        _moveLeftRightValue = _targetZ - transform.position.z > 0 ? 1 : -1;
    }
    
    // Ball entered collision zone (sphere) and is in front of the player
    private bool CanHitBall()
    {
        return ballInsideHitZone && _pointManager.CanHitBall(playerId) &&
               ball.transform.position.x < maxXBallHit;
    }
}
