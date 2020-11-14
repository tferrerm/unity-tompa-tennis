using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PredictionBall : MonoBehaviour
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
    
    public TennisVariables tv;

    public LayerMask layerMask;
    private int _netLayer;
    private int _groundLayer;
    private float _netBounceCoef;
    private float _defaultBounceCoef;
    [HideInInspector] public bool isDropshot;

    
    public Transform ground;
    
    // Start is called before the first frame update
    void Start()
    {
        bounciness = tv.BallBounciness;
        _netBounceCoef = tv.NetBounceFrictionMultiplier;
        _defaultBounceCoef = tv.DefaultBounceFrictionMultiplier;

        _netLayer = LayerMask.NameToLayer("Net");
        _groundLayer = LayerMask.NameToLayer("Ground");
    }

    public void SetupBall(BallPhysics ballPhys, BallInfo ballInf)
    {
        ballPhysics = new BallPhysics(ballPhys);
        ballInfo = new BallInfo(ballInf);
    }

    public bool UpdateBall()
    {
        var bounced = false;
        
        ballInfo = ballPhysics.UpdateBallInfo(ballInfo, Time.fixedDeltaTime, isDropshot); // TODO CHANGE ISDROPSHOT
        if (ballInfo.grounded)
        {
            bounced = true;
        }

        if (CheckCollisions())
        {
            bounced = true;
        }

        UpdateFromBallInfo();

        return bounced;
    }
    
    private void UpdateFromBallInfo()
    {
        transform.position = ballInfo.Position;
        transform.Rotate(ballInfo.eulerAngles);
    }
    
    private bool CheckCollisions()
    {
        var pos = ballInfo.Position;
        var prevPos = ballInfo.PrevPosition;
        var dir = pos - prevPos;
        var distance = dir.magnitude;

        var ballIsMoving = distance > MinDistanceToConsiderBallStopped;

        // If the ball is not moving, I need to modify some parameters in order to make the
        // sphere cast to work
        if (!ballIsMoving)
        {
            dir = Vector3.down;
            distance = Radius;
        }

        var bounced = false;

        // Check collisions against everything but player & ball layers
        if (Physics.RaycastNonAlloc(prevPos, dir.normalized, ballHits, distance, ~layerMask) > 0)
        {
            var hitLayer = ballHits[0].transform.gameObject.layer;
            var bounceVelocity = ballInfo.velocity.magnitude * bounciness * ballHits[0].normal;

            if (_netLayer == hitLayer)
            {
                if (ballHits[0].normal == Vector3.up)
                {
                    // Add X noise to prevent infinite bounces
                    bounceVelocity += new Vector3(ballInfo.velocity.x > 0 ? 1 : -1, 0, 0);
                }
                else
                {
                    bounceVelocity *= _netBounceCoef;
                }
            }
            else
            {
                if (hitLayer != _groundLayer)
                {
                    bounceVelocity *= _defaultBounceCoef;
                }
                
                bounced = true;
            }

            ballInfo.Position = ballHits[0].point + ballHits[0].normal * Radius;
            ballInfo.velocity = bounceVelocity;
        }

        return bounced;
    }
    
    public void Teleport(Vector3 pos)
    {
        // Do not allow y position below the field
        if (pos.y < Radius) pos = new Vector3(pos.x, Radius, pos.z);

        transform.position = pos;
        ballInfo.Position = pos;
        ballInfo.velocity = Vector3.zero;
		
        ballInfo.ResetPrevPosition();

        //if (trail != null) trail.StopTrail();
    }
    
    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public BallInfo BallInfo => ballInfo;
}
