using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Ball : MonoBehaviour
{
    // Min move distance to consider the ball stopped
    private const float MinDistanceToConsiderBallStopped = 0.002f;
    
    // Radius of the ball
    private const float Radius = 0.15f;
    
    // Ball bounciness
    private float bounciness;
    
    // Ball basic information
    private BallInfo ballInfo;
    
    // Ball hits array
    RaycastHit[] ballHits = new RaycastHit[10];

    // Ball Physics
    private BallPhysics ballPhysics;

    public PointManager pointManager;
    public SoundManager soundManager;

    public LayerMask layerMask;
    private int _netLayer;
    private int _groundLayer;
    private float _netBounceCoef;
    private float _defaultBounceCoef;
    [HideInInspector] public bool isDropshot;

    ///////////////////
    public Player player1;
    public AIPlayer player2;
    public Transform ground;
    
    void Start()
    {
        bounciness = TennisVariables.BallBounciness;
        _netBounceCoef = TennisVariables.NetBounceFrictionMultiplier;
        _defaultBounceCoef = TennisVariables.DefaultBounceFrictionMultiplier;
        
        ballInfo.Position = transform.position;
        
        ballPhysics = new BallPhysics(Radius, bounciness, ground.position.y, soundManager, pointManager);

        _netLayer = LayerMask.NameToLayer("Net");
        _groundLayer = LayerMask.NameToLayer("Ground");
    }
    
    void Update()
    {
        ballInfo = ballPhysics.UpdateBallInfo(ballInfo, Time.deltaTime, isDropshot);
		
        CheckCollisions();

        UpdateFromBallInfo();
    }
    
    public float GetRadius()
    {
        return Radius;
    }
    
    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public float GetSpeed()
    {
        return ballInfo.velocity.magnitude;
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

    public void HitBall(Vector3 posEnd, float speed, bool isDropshot, bool applySpeedYAtt, float speedYAttDivisor = 0f)
    {
        this.isDropshot = isDropshot;
        HitBall(ballInfo.Position, posEnd, speed, applySpeedYAtt, speedYAttDivisor);
    }
    
    public void HitBall(Vector3 posStart, Vector3 posEnd, float speed, bool applySpeedYAtt, float speedYAttDivisor = 0f)
    {
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

            if (_netLayer == hitLayer)
            {
                soundManager.PlayNetHit(ballInfo.velocity);
                bounceVelocity *= _netBounceCoef;
            }
            else
            {
                if (hitLayer != _groundLayer)
                {
                    pointManager.HandleBallBounce(null);
                    bounceVelocity *= _defaultBounceCoef;
                }
                soundManager.PlayBallBounce(ballInfo.velocity);
            }

            ballInfo.Position = ballHits[0].point + ballHits[0].normal * Radius;
            ballInfo.velocity = bounceVelocity;
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
