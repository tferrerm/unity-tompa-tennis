using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    private Rigidbody _rigidBody;
    public GameObject tennisBatPlayer1;
    public GameObject tennisBatPlayer2;
    void Start()
    {
        _rigidBody = GetComponent<Rigidbody>();
    }
    
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject == tennisBatPlayer1)
            _rigidBody.AddForce(new Vector3(1, 1, 1), ForceMode.Impulse);
    }
}
