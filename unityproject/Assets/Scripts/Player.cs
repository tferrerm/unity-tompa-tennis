using System;
using System.Collections;
using System.Numerics;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class Player : MonoBehaviour
{
    public int playerId;
    public string playerName;
    
    public float runSpeed = 8;
    public float sprintSpeed = 10;
    public float backSpeed = 5.5f;

    private CharacterController _characterController;
    
    private Animator _animator;
    private int _isMovingHash;
    private int _speedHash;
    private int _strafeHash;
    private int _forwardHash;
    private int _serviceTriggerHash;
    private int _serviceStartHash;
    private int _serviceEndHash;

    private float _moveLeftRightValue;
    private float _moveUpDownValue;

    private const float Epsilon = 0.001f;
    private const float RotationEpsilon = 1e-4f;

    public Rigidbody ball;

    public GameManager gameManager;
    private PointManager _pointManager;

    void Start()
    {
        _pointManager = gameManager.pointManager;
        _characterController = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
        _isMovingHash = Animator.StringToHash("IsMoving");
        _speedHash = Animator.StringToHash("Speed");
        _strafeHash = Animator.StringToHash("Strafe");
        _forwardHash = Animator.StringToHash("Forward");
        _serviceTriggerHash = Animator.StringToHash("Service Trigger");
        _serviceStartHash = Animator.StringToHash("Service Start");
        _serviceEndHash = Animator.StringToHash("Service End");
    }
    
    void Update()
    {
        ReadInput();
        Move();
    }

    private void Move()
    {
        float dt = Time.deltaTime;
        Vector2 movingDir = new Vector2(_moveLeftRightValue, _moveUpDownValue);
        float manhattanNorm = Math.Abs(movingDir[0]) + Math.Abs(movingDir[1]);
        float spd = ActionMapper.IsSprinting() ? sprintSpeed : runSpeed * movingDir.magnitude;
        float dx = dt * (_moveUpDownValue < 0 ? backSpeed : spd) * _moveUpDownValue / manhattanNorm;
        float dz = dt * spd * _moveLeftRightValue / manhattanNorm;

        _characterController.SimpleMove(Vector3.zero);
        
        Vector3 move = new Vector3(dx, 0, dz);
        _animator.SetFloat(_speedHash, spd / sprintSpeed);
        
        _animator.SetFloat(_strafeHash, _moveLeftRightValue);
        _animator.SetFloat(_forwardHash, _moveUpDownValue);
        _characterController.Move(move);
    }

    void ReadInput()
    {
        if (ActionMapper.IsServing() && _pointManager.IsServing(playerId))
        {
            _animator.SetTrigger(_serviceTriggerHash);
        }

        var currentStateHash = _animator.GetCurrentAnimatorStateInfo(0).shortNameHash;
        if (currentStateHash == _serviceStartHash || currentStateHash == _serviceEndHash)
            return;
        
        _moveLeftRightValue = ActionMapper.GetMoveHorizontal(); 
        _moveUpDownValue = ActionMapper.GetMoveVertical();

        _animator.SetBool(_isMovingHash, Math.Abs(_moveLeftRightValue) > Epsilon || Math.Abs(_moveUpDownValue) > Epsilon);
    }

}
