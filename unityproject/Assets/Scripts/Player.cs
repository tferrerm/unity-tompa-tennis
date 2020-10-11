using System;
using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Vector3 movSpeed;

    public float maxMovSpeed;
    public float movAcceleration;

    private CharacterController _characterController;
    private Animator _animator;
    void Start()
    {
        _characterController = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
    }
    
    void Update()
    {
        Move();
    }

    private void Move()
    {
        CalculateSpeed("Vertical");
        CalculateSpeed("Horizontal");
        if (Math.Abs(movSpeed.x) > 0 || Math.Abs(movSpeed.z) > 0)
        {
            _animator.SetBool("Running", true);
        }
        else
        {
            _animator.SetBool("Running", false);
        }
        _characterController.Move(movSpeed * Time.deltaTime);
    }

    private void CalculateSpeed(string axis)
    {
        float input = Input.GetAxis(axis);
        float speedChange = input * movAcceleration * Time.deltaTime;
        
        switch (axis)
        {
            case "Vertical":
                if (input == 0)
                {
                    movSpeed.x = 0;
                }
                else if (Math.Abs(movSpeed.x + speedChange) < maxMovSpeed)
                {
                    movSpeed.x += input * movAcceleration;
                }
                else
                {
                    movSpeed.x = maxMovSpeed * Math.Sign(input);
                }

                break;
            case "Horizontal":
                if (input == 0)
                {
                    movSpeed.z = 0;
                }
                else if (Math.Abs(movSpeed.z - speedChange) < maxMovSpeed)
                {
                    movSpeed.z -= input * movAcceleration;
                }
                else
                {
                    movSpeed.z = maxMovSpeed * Math.Sign(input) * -1;
                }
                break;
            default:
                throw new ArgumentException("Speed can be \"Vertical\" or \"Horizontal\"");
        }
    }
}
