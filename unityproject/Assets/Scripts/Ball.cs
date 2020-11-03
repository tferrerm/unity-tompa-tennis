using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    private Rigidbody _rigidBody;
    public Collider tennisBatPlayer1;
    public Collider tennisBatPlayer2;
    public Player player1;

    public Transform ballThrower; // TODO DELETE
    public Vector3 ballThrowerForce = new Vector3(-1, 0, 0);
    
    void Start()
    {
        _rigidBody = GetComponent<Rigidbody>();
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            transform.position = ballThrower.position;
            ResetVelocity();
            _rigidBody.AddForce(ballThrowerForce, ForceMode.Impulse);
        }
    }

    public void ResetVelocity()
    {
        _rigidBody.velocity = Vector3.zero;
        _rigidBody.angularVelocity = Vector3.zero;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == tennisBatPlayer1)
        {
            ResetVelocity();
            _rigidBody.AddForce(new Vector3(1.75f, 0.25f, 0.5f), ForceMode.Impulse);
        }
        else if (other == player1.BallCollider)
        {
            player1.ballInsideHitZone = true;
            //ResetVelocity();
            //_rigidBody.AddForce(player1.DriveForce, ForceMode.Impulse);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other == player1.BallCollider && player1.ballInsideHitZone)
        {
            player1.ballInsideHitZone = false;
        }
    }

    public bool IsInPlayer2Side()
    {
        return transform.position.x > 0;
    }
}
