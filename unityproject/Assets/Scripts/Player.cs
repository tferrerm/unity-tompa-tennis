using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public enum HitMethod
{
    Drive = 0, Backhand = 1, Serve = 2
}

public enum HitDirection
{
    BackStraight = 0, BackLeft = 1, BackRight = 2
}

public class Player : MonoBehaviour
{
    public int playerId;
    public string playerName;
    
    public float runSpeed = 8;
    public float sprintSpeed = 10;
    public float backSpeed = 5.5f;

    private CharacterController _characterController;

    private AudioSource _audioSource;
    
    private Animator _animator;
    private int _isMovingHash;
    private int _speedHash;
    private int _strafeHash;
    private int _forwardHash;
    private int _serviceTriggerHash;
    private int _serviceStartHash;
    private int _serviceEndHash;
    private int _driveHash;
    private int _backhandHash;

    private float _moveLeftRightValue;
    private float _moveUpDownValue;

    private const float Epsilon = 0.001f;
    private const float RotationEpsilon = 1e-4f;

    public Rigidbody ball;
    private Ball _ballComponent;
    public GameObject attachedBall;
    public Transform attachedBallParent;

    public GameManager gameManager;
    private PointManager _pointManager;
    private SoundManager _soundManager;
    
    private readonly Dictionary<HitMethod, TechniqueAttributes> _techniqueAttrs = new Dictionary<HitMethod, TechniqueAttributes>();
    private readonly Vector3 _serviceTossForce = new Vector3(0f, 0.35f, -0.025f);

    // Ball interaction variables
    public Collider ballCollider;
    [HideInInspector] public bool ballInsideHitZone;
    private HitMethod? _hitMethod;
    private bool _hittingBall;
    private HitDirection _hitDirection;
    public Transform hitBallSpawn;
    public TrailRenderer ballTrailRenderer;

    // Angles for each hit direction
    private const float BackSwingHorizontalAngle = 10f * Mathf.Deg2Rad;
    private const float BackSwingVerticalAngle = 10f * Mathf.Deg2Rad;

    // Unit vectors for each hit direction
    private readonly Vector3 _backStraightSwingUv = Vector3.Normalize(
        new Vector3(Mathf.Cos(BackSwingHorizontalAngle), Mathf.Tan(BackSwingVerticalAngle), 0));
    private readonly Vector3 _backLeftSwingUv = Vector3.Normalize(
        new Vector3(Mathf.Cos(BackSwingHorizontalAngle), Mathf.Tan(BackSwingVerticalAngle), Mathf.Sin(BackSwingHorizontalAngle)));
    private readonly Vector3 _backRightSwingUv = Vector3.Normalize(
        new Vector3(Mathf.Cos(BackSwingHorizontalAngle), Mathf.Tan(BackSwingVerticalAngle), - Mathf.Sin(BackSwingHorizontalAngle)));

    void Start()
    {
        _pointManager = gameManager.pointManager;
        _soundManager = gameManager.soundManager;
        _characterController = GetComponent<CharacterController>();
        _audioSource = GetComponent<AudioSource>();
        _animator = GetComponent<Animator>();
        CalculateAnimatorHashes();

        _ballComponent = ball.GetComponent<Ball>();
        InitTechniqueAttrs();
    }

    private void InitTechniqueAttrs()
    {
        _techniqueAttrs.Add(HitMethod.Drive, new TechniqueAttributes(0.1f, 2.25f));
        _techniqueAttrs.Add(HitMethod.Backhand, new TechniqueAttributes(0.1f, 2.25f));
        _techniqueAttrs.Add(HitMethod.Serve, new TechniqueAttributes(0.1f, 1f));
    }

    private void CalculateAnimatorHashes()
    {
        _isMovingHash = Animator.StringToHash("IsMoving");
        _speedHash = Animator.StringToHash("Speed");
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
        ReadInput();
        Move();
    }

    private void Move()
    {
        if (_hittingBall) return;
        
        float dt = Time.deltaTime;
        Vector2 movingDir = new Vector2(_moveLeftRightValue, _moveUpDownValue);
        float manhattanNorm = Math.Abs(movingDir[0]) + Math.Abs(movingDir[1]);
        if (manhattanNorm == 0)
            manhattanNorm = 1;
        float spd = (ActionMapper.IsSprinting() ? sprintSpeed : runSpeed) * movingDir.magnitude;
        float dx = dt * (_moveUpDownValue < 0 ? backSpeed : spd) * _moveUpDownValue / manhattanNorm;
        float dz = dt * spd * _moveLeftRightValue / manhattanNorm;

        _characterController.SimpleMove(Vector3.zero);
        
        Vector3 move = new Vector3(dx, 0, dz);

        _animator.SetFloat(_strafeHash, _moveLeftRightValue);
        _animator.SetFloat(_forwardHash, _moveUpDownValue);
        _characterController.Move(move);
    }

    private void ReadInput()
    {
        if (ActionMapper.RacquetSwing())
        {
            if (_pointManager.IsServing(playerId))
            {
                StartService();
            } else if (CanHitBall())
            {
                ballInsideHitZone = false; // Cannot hit ball twice
                SelectHitMethod();
                _hittingBall = true;
            }
        }

        ReadMovementInput();
    }

    private void ReadMovementInput()
    {
        var currentStateHash = _animator.GetCurrentAnimatorStateInfo(0).shortNameHash;
        if (_hittingBall || currentStateHash == _serviceStartHash || currentStateHash == _serviceEndHash) // TODO ADD SERVICE BOOL INSTEAD?
        {
            _moveLeftRightValue = _moveUpDownValue = 0;
        }
        else
        {
            _moveLeftRightValue = ActionMapper.GetMoveHorizontal(); 
            _moveUpDownValue = ActionMapper.GetMoveVertical();
        }
    }

    // Ball entered collision zone (sphere) and is in front of the player
    private bool CanHitBall()
    {
        return ballInsideHitZone && _pointManager.CanHitBall(playerId) &&
               ball.transform.position.x > transform.position.x + 2;
    }

    private void SelectHitMethod()
    {
        var ballPosition = ball.transform.position;
        if (ballPosition.z < transform.position.z) // Should take into account ball direction
        {
            _animator.SetTrigger(_driveHash);
            _hitMethod = HitMethod.Drive;
        }
        else
        {
            _animator.SetTrigger(_backhandHash);
            _hitMethod = HitMethod.Backhand;
        }
        _hitDirection = HitDirection.BackStraight; // Default value if no keys pressed
    }

    private void StartService()
    {
        _animator.SetTrigger(_serviceTriggerHash);
        SwitchBallType(true);
        _hitMethod = HitMethod.Serve;
    }

    private void ReadHitDirection()
    {
        if (_hitMethod == null) return;
        
        var hittingStraight = ActionMapper.GetForward();
        var hittingLeft = ActionMapper.GetLeft();
        var hittingRight = ActionMapper.GetRight();
        if (hittingStraight)
        {
            if (hittingLeft)
            {
                _hitDirection = HitDirection.BackLeft;
            } else if (hittingRight)
            {
                _hitDirection = HitDirection.BackRight;
            }
            else
            {
                _hitDirection = HitDirection.BackStraight;
            }
        }
    }

    private void TossServiceBall() // Called as animation event
    {
        ball.transform.position = attachedBallParent.transform.position;
        SwitchBallType(false);
        _ballComponent.ResetVelocity();
        ball.AddForce(_serviceTossForce, ForceMode.Impulse);
        _hitMethod = null;
    }

    private void HitServiceBall() // Called as animation event
    {
        _ballComponent.ResetVelocity();
        ball.AddForce(new Vector3(1.75f, 0.25f, 0.5f), ForceMode.Impulse);
    }

    void SwitchBallType(bool attachBall) 
    {
        attachedBall.SetActive(attachBall);
        ball.gameObject.SetActive(!attachBall);
    }

    private void Step(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5)
            _soundManager.PlayFootstep(_audioSource);
    }

    private void HitBall() // Called as animation event
    {
        _ballComponent.ResetVelocity();
        ball.position = hitBallSpawn.position;
        ballTrailRenderer.Clear();
        ReadHitDirection();
        if (_hitDirection == HitDirection.BackStraight)
        {
            ball.AddForce(_backStraightSwingUv * _techniqueAttrs[_hitMethod.GetValueOrDefault()].ForceMultiplier, ForceMode.Impulse);
        } else if (_hitDirection == HitDirection.BackLeft)
        {
            ball.AddForce(_backLeftSwingUv * _techniqueAttrs[_hitMethod.GetValueOrDefault()].ForceMultiplier, ForceMode.Impulse);
        }
        else if (_hitDirection == HitDirection.BackRight)
        {
            ball.AddForce(_backRightSwingUv * _techniqueAttrs[_hitMethod.GetValueOrDefault()].ForceMultiplier, ForceMode.Impulse);
        }
        
        _hitMethod = null;
    }

    private void ResetHittingBall() // Called as animation event
    {
        _hittingBall = false;
    }

    public Collider BallCollider => ballCollider;
}
