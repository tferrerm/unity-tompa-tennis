using System;
using System.Collections.Generic;
using UnityEngine;

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

    private PointManager _pointManager;
    private SoundManager _soundManager;
    private ReplayManager _replayManager;
    private TennisVariables _tv;

    public LayerMask layerMask;
    private int _netLayer;
    private int _groundLayer;
    private float _netBounceCoef;
    private float _defaultBounceCoef;
    private bool _isDropshot;

    public TrailRenderer trail;

    ///////////////////
    private Player _player1;
    private AIPlayer _player2;
    public Transform ground;

    void Start()
    {
        var gameManager = GameObject.FindWithTag("GameController").GetComponent<GameManager>();
        _pointManager = gameManager.pointManager;
        _soundManager = gameManager.soundManager;
        _replayManager = gameManager.replayManager;
        _tv = gameManager.tennisVariables;
        _player1 = gameManager.player;
        _player2 = gameManager.aiPlayer;
        
        bounciness = _tv.BallBounciness;
        _netBounceCoef = _tv.NetBounceFrictionMultiplier;
        _defaultBounceCoef = _tv.DefaultBounceFrictionMultiplier;
        
        ballInfo.Position = transform.position;
        
        ballPhysics = new BallPhysics(Radius, bounciness, ground.position.y, _soundManager, _pointManager, _tv);

        _netLayer = LayerMask.NameToLayer("Net");
        _groundLayer = LayerMask.NameToLayer("Ground");
    }

    void FixedUpdate()
    {
        if (_replayManager.isRecording)
        {
            ballInfo = ballPhysics.UpdateBallInfo(ballInfo, Time.fixedDeltaTime, _isDropshot);
		
            CheckCollisions();

            UpdateFromBallInfo();
        }
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

    private void UpdateFromBallInfo()
    {
        var speed = Vector3.Distance(ballInfo.Position, transform.position) / Time.deltaTime;

        // Change the speed of the trail
        //trail.SetSpeed(speed * 0.25f);

        transform.position = ballInfo.Position;
        transform.Rotate(ballInfo.eulerAngles);
    }

    public void HitBall(Vector3 posEnd, float speed, bool isDropshot, bool applySpeedYAtt, float speedYAttDivisor = 0f)
    {
        _isDropshot = isDropshot;
        HitBall(ballInfo.Position, posEnd, speed, applySpeedYAtt, speedYAttDivisor);
    }

    private void HitBall(Vector3 posStart, Vector3 posEnd, float speed, bool applySpeedYAtt, float speedYAttDivisor = 0f)
    {
        var dis = Vector2.Distance(new Vector2(posStart.x, posStart.z), new Vector2(posEnd.x, posEnd.z));

        var speedYAtt = applySpeedYAtt ? (dis / speedYAttDivisor) : 1f;
        
        var throwDir = new Vector3(posEnd.x - posStart.x, (posEnd.y - posStart.y) * speed * speedYAtt, posEnd.z - posStart.z);

        var time = throwDir.magnitude / speed;
     
        var velX = (posEnd.x - posStart.x) / time;
        var velZ = (posEnd.z - posStart.z) / time;
        
        var speedVertical = ((posEnd.y - posStart.y) - 0.5f * Physics.gravity.y * time * time ) / time;
        var velocity = throwDir.normalized * speed;
        velocity.y = speedVertical;

        Teleport(posStart);
        ballInfo.velocity = new Vector3(velX, velocity.y, velZ);
    }

    void CheckCollisions()
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

        // Check collisions against everything but player & ball layers
        if (Physics.RaycastNonAlloc(prevPos, dir.normalized, ballHits, distance, ~layerMask) > 0)
        {
            var hitLayer = ballHits[0].transform.gameObject.layer;
            var bounceVelocity = ballInfo.velocity.magnitude * bounciness * ballHits[0].normal;

            if (_netLayer == hitLayer)
            {
                _soundManager.PlayNetHit(ballInfo.velocity);
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
                    _pointManager.HandleBallBounce(null);
                    bounceVelocity *= _defaultBounceCoef;
                }
                _soundManager.PlayBallBounce(ballInfo.velocity);
            }

            ballInfo.Position = ballHits[0].point + ballHits[0].normal * Radius;
            ballInfo.velocity = bounceVelocity;
        }
    }
    
    public void Teleport(Vector3 pos)
    {
        // Do not allow y position below the field
        if (pos.y < Radius) pos = new Vector3(pos.x, Radius, pos.z);

        transform.position = pos;
        ballInfo.Position = pos;
        ballInfo.velocity = Vector3.zero;
		
        ballInfo.ResetPrevPosition();

        trail.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == _player1.BallCollider)
        {
            _player1.ballInsideHitZone = true;
        }
        else if (other == _player2.ballCollider)
        {
            _player2.ballInsideHitZone = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other == _player1.BallCollider && _player1.ballInsideHitZone)
        {
            _player1.ballInsideHitZone = false;
        }
        else if (other == _player2.ballCollider && _player2.ballInsideHitZone)
        {
            _player2.ballInsideHitZone = false;
        }
    }

    public bool IsInPlayerSide(int playerId)
    {
        if(playerId == 0)
            return transform.position.x < 0;
        else
            return transform.position.x >= 0;
    }

    public BallPhysics BallPhysics => ballPhysics;

    public BallInfo BallInfo => ballInfo;

    public bool IsDropshot => _isDropshot;
    
    public void ReplayMove(Vector3 position, Quaternion rotation)
    {
        transform.position = position;
        transform.rotation = rotation;
    }
}
