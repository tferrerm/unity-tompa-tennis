using System;
using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float movementSpeed;
    
    private CharacterController _characterController;
    void Start()
    {
        _characterController = GetComponent<CharacterController>();
    }
    
    void Update()
    {
        Move();
    }

    private void Move()
    {
        float vertical = Input.GetAxis("Vertical") * movementSpeed;
        float horizontal = Input.GetAxis("Horizontal") * movementSpeed;
        
        Vector3 move = new Vector3(vertical, 0, -1 * horizontal);
        _characterController.Move(move * Time.deltaTime);
    }
}
