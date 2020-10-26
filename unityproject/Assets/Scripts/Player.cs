using System;
using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float runSpeed;
    public float sprintSpeed;

    private CharacterController _characterController;
    
    private Animator _animator;
    private int _isMovingHash;
    private int _speedHash;
    private int _directionHash;

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
        _directionHash = Animator.StringToHash("Direction");
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
        Vector3 move = new Vector3(dx, 0, dz);
        _animator.SetFloat(_speedHash, spd / sprintSpeed);
        _animator.SetInteger(_directionHash, CurrentDirection(move));
        _characterController.Move(move);
        
        /*var rotation = move;
        
        if (rotation.sqrMagnitude > RotationEpsilon)
        {
            transform.forward = rotation;
        }*/
    }

    private int CurrentDirection(Vector3 move)
    {
        if (move.z > 0)
            return (int)Direction.Left;
        if (move.z < 0)
            return (int)Direction.Right;
        if (move.x < 0)
            return (int)Direction.Back;

        return (int)Direction.Forward;
    }

    void ReadInput()
    {
        _moveLeftRightValue = ActionMapper.GetMoveHorizontal(); 
        _moveUpDownValue = ActionMapper.GetMoveVertical();

        _animator.SetBool(_isMovingHash, Math.Abs(_moveLeftRightValue) > Epsilon || Math.Abs(_moveUpDownValue) > Epsilon);
    }

}
