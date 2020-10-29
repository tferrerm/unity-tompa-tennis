﻿using System;
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
    private int _driveHash;
    private int _backhandHash;

    private float _moveLeftRightValue;
    private float _moveUpDownValue;

    private const float Epsilon = 0.001f;
    private const float RotationEpsilon = 1e-4f;

    public Rigidbody ball;
    public GameObject attachedBall;
    public Transform attachedBallParent;

    public GameManager gameManager;
    private PointManager _pointManager;
    
    private bool _serveBallReleased = false;
    public Vector3 _tossForce = new Vector3(0f, 0.1f, 0f);

    void Start()
    {
        _pointManager = gameManager.pointManager;
        _characterController = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
        CalculateAnimatorHashes();
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
        if (ActionMapper.IsServing() && _pointManager.IsServing(playerId))
        {
            _animator.SetTrigger(_serviceTriggerHash);
            SwitchBallType(true);
        } else if (ActionMapper.Drive())
            _animator.SetTrigger(_driveHash);
        else if (ActionMapper.Backhand())
            _animator.SetTrigger(_backhandHash);

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
            ball.AddForce(_tossForce, ForceMode.Impulse);
        }
    }

    void SwitchBallType(bool attachBall) 
    {
        _serveBallReleased = !attachBall;
        attachedBall.SetActive(attachBall);
        ball.gameObject.SetActive(!attachBall);
    }

}
