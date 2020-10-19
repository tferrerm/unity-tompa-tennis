using System;
using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float runSpeed;
    public float sprintSpeed;

    public float maxMovSpeed;
    public float movAcceleration;

    private CharacterController _characterController;
    private Animator _animator;

    private float _moveLeftRightValue;
    private float _moveUpDownValue;

    private const float Epsilon = 0.001f;
    
    void Start()
    {
        _moveLeftRightValue = 0;
        _moveUpDownValue = 0;
        _characterController = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
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
        /*_animator.SetFloat("moveSpeed", spd / sprintSpeed);*/
        float dx = dt * spd * _moveLeftRightValue;
        float dz = dt * spd * _moveUpDownValue;
        _characterController.Move(new Vector3(dx, 0, dz));
        
        if (Mathf.Abs(_moveLeftRightValue) > Epsilon || Mathf.Abs(_moveUpDownValue) > Epsilon)
        {
            transform.forward = new Vector3(dx, 0f, dz);
        }
    }

    void ReadInput()
    {
        _moveLeftRightValue = ActionMapper.GetMoveHorizontal(); 
        _moveUpDownValue = ActionMapper.GetMoveVertical();

        _animator.SetBool("isMoving", Math.Abs(_moveLeftRightValue) > Epsilon || Math.Abs(_moveUpDownValue) > Epsilon);
    }

}
