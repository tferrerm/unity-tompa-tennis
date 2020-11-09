using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    // Min move distance to consider the ball stopped
    private const float MinDistanceToConsiderBallStopped = 0.002f;
    
    // Radius of the ball
    private const float Radius = 0.15f;
    
    // Ball bounciness
    private float bounciness = 0.85f;
    
    // Ball basic information
    private BallInfo ballInfo;
    
    // Ball hits array
    RaycastHit[] ballHits = new RaycastHit[10];

    // Ball Physics
    private BallPhysics ballPhysics;
    public float netBounceCoef = 0.25f;

    // Latest Y position of the ball
    private float[] ballPrevPosY;

    public PointManager pointManager;
    public SoundManager soundManager;

    public LayerMask layerMask;
    public int netLayer;

    ///////////////////
    public Player player1;
    public AIPlayer player2;
    public Transform ground;
    public Transform ballThrower; // TODO DELETE
    public Vector3 ballThrowerForce = new Vector3(-1, 0, 0);
    
    void Start()
    {
        ballInfo.Position = transform.position;
        
        ballPhysics = new BallPhysics(Radius, bounciness, ground.position.y, soundManager, pointManager);
        
        // Array to store the latests positions of the ball
        ballPrevPosY = new float[2];
        //Vector3 targetPosition = GameObject.FindWithTag("GameController").GetComponent<GameManager>().courtManager.GetHitTargetPosition(1, HitDirectionVertical.Back, HitDirectionHorizontal.Center);
        //HitBall(targetPosition);

        netLayer = LayerMask.NameToLayer("Net");
    }
    
    void Update()
    {
        ballInfo = ballPhysics.UpdateBallInfo(ballInfo, Time.deltaTime);
		
        CheckCollisions();

        UpdateFromBallInfo();
        
        /*if (Input.GetKeyDown(KeyCode.P))
        {
            transform.position = ballThrower.position;
            ResetVelocity();
            _rigidBody.AddForce(ballThrowerForce, ForceMode.Impulse);
        }

        ApplyTargetVelocity();*/
    }

    /*private void ApplyTargetVelocity()
    {
        velocity += new Vector3(0.0f, Physics.gravity.y * Time.deltaTime, 0.0f);
        transform.position += velocity * Time.deltaTime;
        /*
        var pStart = transform.position;
        var pEnd = targetPosition.position;
        var time = Vector3.Distance(pStart, pEnd) / speed;
        var velocity = new Vector3(
            (pEnd.x - pStart.x) / time,
                ((pEnd.y - pStart.y) - 0.5f * gravity * (float)Math.Pow(Time.deltaTime, 2)) / time,
                (pEnd.z - pStart.z) / time
            );
        Debug.Log(velocity);
        transform.position += velocity * Time.deltaTime;//
    }*/
    
    public float GetRadius()
    {
        return Radius;
    }
    
    public Vector3 GetPosition()
    {
        return transform.position;
    }
    
    bool UpdateFromBallInfo()
    {
        var speed = Vector3.Distance(ballInfo.Position, transform.position) / Time.deltaTime;

        // Change the speed of the trail
        //trail.SetSpeed(speed * 0.25f);

        transform.position = ballInfo.Position;
        transform.Rotate(ballInfo.eulerAngles);

        return true;
    }

    public void HitBall(Vector3 posEnd, float speed, bool applySpeedYAtt, float speedYAttDivisor = 0f)
    {
        HitBall(ballInfo.Position, posEnd, speed, applySpeedYAtt, speedYAttDivisor);
    }
    
    public void HitBall(Vector3 posStart, Vector3 posEnd, float speed, bool applySpeedYAtt, float speedYAttDivisor = 0f)
    {
        // TODO Add target position epsilon for unaccurate shots...
        
        float dis = Vector2.Distance(new Vector2(posStart.x, posStart.z), new Vector2(posEnd.x, posEnd.z));

        float speedYAtt = applySpeedYAtt ? (dis / speedYAttDivisor) : 1f;
        
        Vector3 throwDir = new Vector3(posEnd.x - posStart.x, (posEnd.y - posStart.y) * speed * speedYAtt, posEnd.z - posStart.z);

        float time = throwDir.magnitude / speed;
     
        float velx = (posEnd.x - posStart.x) / time;
        float velz = (posEnd.z - posStart.z) / time;
        
        float speedVertical = ((posEnd.y - posStart.y) - 0.5f * Physics.gravity.y * time * time ) / time;
        Vector3 velocity = throwDir.normalized * speed;
        velocity.y = speedVertical;

        TelePort(posStart);
        ballInfo.velocity = new Vector3(velx, velocity.y, velz);
        
        //SoundManager.Instance.PlaySound("ThrowWhoosh");
    }
    
    void CheckCollisions()
    {
        var pos = ballInfo.Position;
        var prevpos = ballInfo.PrevPosition;
        var dir = pos - prevpos;
        var distance = dir.magnitude;

        var ballIsMoving = distance > MinDistanceToConsiderBallStopped;

        // If the ball is not moving, I need to modify some parameters in order to make the
        // sphere cast to work
        if (!ballIsMoving)
        {
            dir = Vector3.down;
            distance = Radius;
        }

        // Check collisions against everything but player & ball layers
        if (Physics.RaycastNonAlloc(prevpos, dir.normalized, ballHits, distance, ~layerMask) > 0)
        {
            var hitLayer = ballHits[0].transform.gameObject.layer;
            var bounceVelocity = ballInfo.velocity.magnitude * bounciness * ballHits[0].normal;

            if (netLayer == hitLayer)
            {
                soundManager.PlayNetHit(ballInfo.velocity);
                bounceVelocity *= netBounceCoef;
            }
            else
            {
                soundManager.PlayBallBounce(ballInfo.velocity);
            }

            ballInfo.Position = ballHits[0].point + ballHits[0].normal * Radius;
            ballInfo.velocity = bounceVelocity;

            Debug.Log("COLLISION");
        }
    }
    
    public void TelePort(Vector3 pos)
    {
        // Do not allow y position below the field
        if (pos.y < Radius) pos = new Vector3(pos.x, Radius, pos.z);

        transform.position = pos;
        ballInfo.Position = pos;
        ballInfo.velocity = Vector3.zero;
		
        ballInfo.ResetPrevPosition();

        //if (trail != null) trail.StopTrail();
    }

    /*public void ResetVelocity()
    {
        //_rigidBody.velocity = Vector3.zero;
        //_rigidBody.angularVelocity = Vector3.zero;
    }*/

    private void OnTriggerEnter(Collider other)
    {
        if (other == player1.BallCollider)
        {
            player1.ballInsideHitZone = true;
        }
        else if (other == player2.ballCollider)
        {
            player2.ballInsideHitZone = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other == player1.BallCollider && player1.ballInsideHitZone)
        {
            player1.ballInsideHitZone = false;
        }
        else if (other == player2.ballCollider && player2.ballInsideHitZone)
        {
            player2.ballInsideHitZone = false;
        }
    }

    /*public bool IsInPlayer2Side()
    {
        return transform.position.x > 0;
    }*/
}
