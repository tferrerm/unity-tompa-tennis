using System;
using System.Collections;
using System.Numerics;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class Player : MonoBehaviour
{
    public float runSpeed;
    public float sprintSpeed;

    private CharacterController _characterController;
    
    private Animator _animator;
    private int _isMovingHash;
    private int _speedHash;
    private int _strafeHash;
    private int _forwardHash;

    private float _moveLeftRightValue;
    private float _moveUpDownValue;

    private const float Epsilon = 0.001f;
    private const float RotationEpsilon = 1e-4f;

    public Rigidbody ball;

    void Start()
    {
        _characterController = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
        _isMovingHash = Animator.StringToHash("IsMoving");
        _speedHash = Animator.StringToHash("Speed");
        _strafeHash = Animator.StringToHash("Strafe");
        _forwardHash = Animator.StringToHash("Forward");
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
        float spd = ActionMapper.IsSprinting() ? sprintSpeed : runSpeed * movingDir.magnitude;
        float dx = dt * spd * _moveUpDownValue;
        float dz = dt * spd * _moveLeftRightValue;

        _characterController.SimpleMove(Vector3.zero);
        
        Vector3 move = new Vector3(dx, 0, dz);
        _animator.SetFloat(_speedHash, spd / sprintSpeed);
        Vector2 animatorDirections = CurrentDirection(move);
        _animator.SetFloat(_strafeHash, animatorDirections[0]);
        _animator.SetFloat(_forwardHash, animatorDirections[1]);
        _characterController.Move(move);
    }

    private Vector2 CurrentDirection(Vector3 move)
    {
        Vector2 response = new Vector2((float) StrafeDirection.Idle, 
            (float) VerticalDirection.Idle);
        if (move.z > 0)
            response[0] = (float) StrafeDirection.Left;
        else if (move.z < 0)
            response[0] = (float) StrafeDirection.Right;
        if (move.x > 0)
            response[1] = (float) VerticalDirection.Forward;
        else if (move.x < 0)
            response[1] = (float) VerticalDirection.Backward;

        return response;
    }

    void ReadInput()
    {
        _moveLeftRightValue = ActionMapper.GetMoveHorizontal(); 
        _moveUpDownValue = ActionMapper.GetMoveVertical();

        _animator.SetBool(_isMovingHash, Math.Abs(_moveLeftRightValue) > Epsilon || Math.Abs(_moveUpDownValue) > Epsilon);
    }

}
