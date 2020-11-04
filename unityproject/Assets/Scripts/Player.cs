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
    
    private Dictionary<HitMethod, TechniqueAttributes> techniqueAttrs = new Dictionary<HitMethod, TechniqueAttributes>();
    private bool _serveBallReleased = false;
    public Vector3 _tossForce = new Vector3(0f, 0.1f, 0f);
    public Vector3 driveForce = new Vector3(1f, 0f, 0f);

    public Collider ballCollider;
    private Renderer ballRenderer; // TODO DELETE
    [HideInInspector] public bool ballInsideHitZone;
    private HitMethod? _hitMethod;
    private bool hittingBall;

    public Transform hitBallSpawn;

    void Start()
    {
        _pointManager = gameManager.pointManager;
        _soundManager = gameManager.soundManager;
        _characterController = GetComponent<CharacterController>();
        _audioSource = GetComponent<AudioSource>();
        _animator = GetComponent<Animator>();
        CalculateAnimatorHashes();

        _ballComponent = ball.GetComponent<Ball>();
        ballRenderer = ballCollider.GetComponent<Renderer>();
        InitTechniqueAttrs();
    }

    private void InitTechniqueAttrs()
    {
        techniqueAttrs.Add(HitMethod.Drive, new TechniqueAttributes(0.1f, 1f));
        techniqueAttrs.Add(HitMethod.Backhand, new TechniqueAttributes(0.1f, 1f));
        techniqueAttrs.Add(HitMethod.Serve, new TechniqueAttributes(0.1f, 1f));
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
        CheckServiceStatus();
        ReadInput();
        Move();
    }

    private void Move()
    {
        if (hittingBall) return;
        
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

    void ReadInput()
    {
        if (ActionMapper.RacquetSwing())
        {
            if (_pointManager.IsServing(playerId))
            {
                _animator.SetTrigger(_serviceTriggerHash);
                SwitchBallType(true);
                _hitMethod = HitMethod.Serve;
            } else if (ballInsideHitZone && _pointManager.CanHitBall(playerId) && ball.transform.position.x > transform.position.x + 1.5f)
            {
                // Ball entered collision zone (sphere) and is in front of the player
                ballInsideHitZone = false; // Cannot hit ball twice
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

                hittingBall = true;
            }
        }

        var currentStateHash = _animator.GetCurrentAnimatorStateInfo(0).shortNameHash;
        if (currentStateHash == _serviceStartHash || currentStateHash == _serviceEndHash)
        {
            _moveLeftRightValue = _moveUpDownValue = 0;
            return;
        }

        _moveLeftRightValue = ActionMapper.GetMoveHorizontal(); 
        _moveUpDownValue = ActionMapper.GetMoveVertical();
    }

    void CheckServiceStatus()
    {
        var currentStateHash = _animator.GetCurrentAnimatorStateInfo(0).shortNameHash;
        if (currentStateHash == _serviceEndHash && !_serveBallReleased)
        {
            ball.transform.position = attachedBallParent.transform.position;
            SwitchBallType(false);
            ball.GetComponent<Ball>().ResetVelocity();
            ball.AddForce(_tossForce, ForceMode.Impulse);
            _hitMethod = null;
        }
    }

    void SwitchBallType(bool attachBall) 
    {
        _serveBallReleased = !attachBall;
        attachedBall.SetActive(attachBall);
        ball.gameObject.SetActive(!attachBall);
    }

    private void Step(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5)
            _soundManager.PlayFootstep(_audioSource);
    }

    private void ToggleBallCollider()
    {
        //ballCollider.enabled = !ballCollider.enabled;
        //ballRenderer.enabled = !ballRenderer.enabled;
    }

    private void HitBall()
    {
        _ballComponent.ResetVelocity();
        ball.position = hitBallSpawn.position;
        ball.AddForce(new Vector3(1.5f, 0.5f, 0) * techniqueAttrs[_hitMethod.GetValueOrDefault()].ForceMultiplier, ForceMode.Impulse);
        _hitMethod = null;
    }

    private void ResetHittingBall()
    {
        hittingBall = false;
    }

    public Vector3 DriveForce => driveForce;

    public Collider BallCollider => ballCollider;
}
