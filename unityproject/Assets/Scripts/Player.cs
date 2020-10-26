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
        _animator.SetFloat(_speedHash, spd / sprintSpeed);
        float dx = dt * spd * _moveLeftRightValue;
        float dz = dt * spd * _moveUpDownValue;
        _characterController.Move(new Vector3(dx, 0, dz));
        
        var rotation = new Vector3(dx, 0f, dz);
        
        if (rotation.sqrMagnitude > RotationEpsilon)
        {
            transform.forward = rotation;
        }
    }

    void ReadInput()
    {
        _moveLeftRightValue = ActionMapper.GetMoveVertical(); 
        _moveUpDownValue = ActionMapper.GetMoveHorizontal();

        _animator.SetBool(_isMovingHash, Math.Abs(_moveLeftRightValue) > Epsilon || Math.Abs(_moveUpDownValue) > Epsilon);
    }

}
