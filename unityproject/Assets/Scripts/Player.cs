using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public enum HitMethod
{
    Drive = 0, Backhand = 1, Serve = 2
}

public enum HitDirectionHorizontal
{
    Left = 0, Center = 1, Right = 2
}

public enum HitDirectionVertical
{
    Deep = 0, Dropshot = 1
}

public class Player : MonoBehaviour
{
    public int playerId;
    public string playerName;
    
    public float runSpeed = 8;
    public float sprintSpeed = 10;
    public float backSpeed = 5.5f;
    public float walkingSpeed = 5f;
    private float ballTargetRadius = 2.5f;
    private float serveBallTargetRadius = 2f;

    private CharacterController _characterController;

    private AudioSource _audioSource;
    
    private Animator _animator;
    private int _strafeHash;
    private int _forwardHash;
    private int _serviceTriggerHash;
    private int _serviceStartHash;
    private int _serviceEndHash;
    private int _driveHash;
    private int _backhandHash;
    private int _cheerHash;
    private int _defeatedHash;
    private int _fastDriveHash;
    private int _fastBackhandHash;
    
    private bool _reachedServingBound;

    private float _moveLeftRightValue;
    private float _moveUpDownValue;

    public Ball ball;
    public GameObject attachedBall; // Ball copy attached to player's hand for service animation
    public Transform attachedBallParent;

    public GameManager gameManager;
    private CourtManager _courtManager;
    private PointManager _pointManager;
    private SoundManager _soundManager;
    
    //private readonly Dictionary<HitMethod, TechniqueAttributes> _techniqueAttrs = new Dictionary<HitMethod, TechniqueAttributes>();

    // Ball interaction variables
    public Collider ballCollider;
    [HideInInspector] public bool ballInsideHitZone;
    private HitMethod? _hitMethod;
    public bool _movementBlocked;
    private HitDirectionHorizontal? _hitDirectionHoriz;
    private HitDirectionVertical? _hitDirectionVert;
    public Transform hitBallSpawn;
    public Transform hitServiceBallSpawn;
    private float serviceTossSpeed = 20f;
    // Prevents movement until serve animation is finished
    [HideInInspector] public bool serveDone;
    // Prevents serving twice in the same point
    [HideInInspector] public bool hitServiceBall;
    
    public TrailRenderer ballTrailRenderer;

    void Start()
    {
        _courtManager = gameManager.courtManager;
        _pointManager = gameManager.pointManager;
        _soundManager = gameManager.soundManager;
        _characterController = GetComponent<CharacterController>();
        _audioSource = GetComponent<AudioSource>();
        _animator = GetComponent<Animator>();
        CalculateAnimatorHashes();

        //InitTechniqueAttrs();
        //_movementBlocked = _pointManager.IsServing(playerId);
    }

    /*private void InitTechniqueAttrs()
    {
        _techniqueAttrs.Add(HitMethod.Drive, new TechniqueAttributes(0.1f, 2.25f));
        _techniqueAttrs.Add(HitMethod.Backhand, new TechniqueAttributes(0.1f, 2.25f));
        _techniqueAttrs.Add(HitMethod.Serve, new TechniqueAttributes(0.1f, 1f));
    }*/

    private void CalculateAnimatorHashes()
    {
        _strafeHash = Animator.StringToHash("Strafe");
        _forwardHash = Animator.StringToHash("Forward");
        _serviceTriggerHash = Animator.StringToHash("Service Trigger");
        _serviceStartHash = Animator.StringToHash("Service Start");
        _serviceEndHash = Animator.StringToHash("Service End");
        _driveHash = Animator.StringToHash("Drive Trigger");
        _backhandHash = Animator.StringToHash("Backhand Trigger");
        _cheerHash = Animator.StringToHash("Cheer Trigger");
        _defeatedHash = Animator.StringToHash("Defeated Trigger");
        _fastDriveHash = Animator.StringToHash("Fast Drive Trigger");
        _fastBackhandHash = Animator.StringToHash("Fast Backhand Trigger");
    }
    
    void Update()
    {
        ReadInput();
        Move();
    }

    private void Move()
    {
        if (_movementBlocked) return;

        var dt = Time.deltaTime;
        var movingDir = new Vector2(_moveLeftRightValue, _moveUpDownValue);
        
        if (_pointManager.IsServing(playerId) && !serveDone)
        {
            var posZ = transform.position.z;
            var spd = walkingSpeed * movingDir.magnitude;
            var dz = dt * spd * _moveLeftRightValue;
            if (_pointManager.ServicePositionOutOfBounds(playerId, posZ + dz + 1 * Mathf.Sign(_moveLeftRightValue)))
            {
                _reachedServingBound = true;
                dz = posZ;
            }
            else
            {
                _reachedServingBound = false;
            }
            
            _characterController.SimpleMove(Vector3.zero);
            if (!_reachedServingBound)
            {
                var move = new Vector3(0, 0, dz);
                _characterController.Move(move);
            }

            _animator.SetFloat(_strafeHash, _reachedServingBound ? 0 : _moveLeftRightValue * 0.5f);
            _animator.SetFloat(_forwardHash, 0);
            
        }
        else
        {
            var manhattanNorm = Math.Abs(movingDir[0]) + Math.Abs(movingDir[1]);
            if (manhattanNorm == 0)
                manhattanNorm = 1;
            
            var spd = (ActionMapper.IsSprinting() ? sprintSpeed : runSpeed) * movingDir.magnitude;
            var dx = transform.position.x < 0 ? dt * (_moveUpDownValue < 0 ? backSpeed : spd) * _moveUpDownValue / manhattanNorm : 0;
            var dz = dt * spd * _moveLeftRightValue / manhattanNorm;
            
            _characterController.SimpleMove(Vector3.zero);
            var move = new Vector3(dx, 0, dz);

            _animator.SetFloat(_strafeHash, _moveLeftRightValue);
            _animator.SetFloat(_forwardHash, _moveUpDownValue);
            _characterController.Move(move);
        }
    }

    private void ReadInput()
    {
        if (ActionMapper.RacquetSwing())
        {
            if (_pointManager.IsServing(playerId) && !hitServiceBall && _hitMethod == null)
            {
                StartService();
                _movementBlocked = true;
            } else if (CanHitBall())
            {
                ballInsideHitZone = false; // Cannot hit ball twice
                SelectHitMethod();
                _movementBlocked = true;
            }
        }

        if (_movementBlocked)
        {
            if (_hitMethod == HitMethod.Serve)
            {
                ReadServiceDirection();
            }
            else
            {
                ReadHitDirection();
            }
        }
        else
        {
            ReadMovementInput();
        }
    }

    private void ReadMovementInput()
    {
        var currentStateHash = _animator.GetCurrentAnimatorStateInfo(0).shortNameHash;
        if (_movementBlocked || currentStateHash == _serviceStartHash || currentStateHash == _serviceEndHash) // TODO ADD SERVICE BOOL INSTEAD?
        {
            _moveLeftRightValue = _moveUpDownValue = 0;
        }
        else
        {
            _moveLeftRightValue = ActionMapper.GetMoveHorizontal(); 
            _moveUpDownValue = ActionMapper.GetMoveVertical();
        }
    }

    // Ball entered collision zone (sphere) and is in front of the player
    private bool CanHitBall()
    {
        return ballInsideHitZone && _pointManager.CanHitBall(playerId) &&
               ball.transform.position.x > transform.position.x + TennisVariables.BallColliderFrontDelta;
    }

    private void SelectHitMethod()
    {
        var ballPosition = ball.transform.position;
        var ballSpeed = ball.GetSpeed();
        if (ballPosition.z < transform.position.z) // Should take into account ball direction
        {
            if (ballSpeed < TennisVariables.FastHitAnimationThresholdSpeed)
            {
                _animator.SetTrigger(_driveHash);
            }
            else
            {
                PlayerGrunt();
                _animator.SetTrigger(_fastDriveHash);
            }
            
            _hitMethod = HitMethod.Drive;
        }
        else
        {
            if (ballSpeed < TennisVariables.FastHitAnimationThresholdSpeed)
            {
                _animator.SetTrigger(_backhandHash);
            }
            else
            {
                PlayerGrunt();
                _animator.SetTrigger(_fastBackhandHash);
            }
            _hitMethod = HitMethod.Backhand;
        }
        _hitDirectionHoriz = HitDirectionHorizontal.Center; // Default value if no keys pressed
        _hitDirectionVert = HitDirectionVertical.Deep;
    }

    private void StartService()
    {
        _animator.SetTrigger(_serviceTriggerHash);
        _hitMethod = HitMethod.Serve;
        _hitDirectionHoriz = HitDirectionHorizontal.Center;
    }

    private void ReadHitDirection()
    {
        if (_hitMethod == null) return;
        
        var hittingStraight = ActionMapper.GetForward();
        var hittingDropshot = ActionMapper.GetBackward();
        var hittingLeft = ActionMapper.GetLeft();
        var hittingRight = ActionMapper.GetRight();
        
        if (hittingStraight) // Vertical input resets horizontal input
        {
            _hitDirectionVert = HitDirectionVertical.Deep;
            _hitDirectionHoriz = HitDirectionHorizontal.Center;
        } else if (hittingDropshot)
        {
            _hitDirectionVert = HitDirectionVertical.Dropshot;
            _hitDirectionHoriz = HitDirectionHorizontal.Center;
        }

        if (hittingLeft)
        {
            _hitDirectionHoriz = HitDirectionHorizontal.Left;
        } else if (hittingRight)
        {
            _hitDirectionHoriz = HitDirectionHorizontal.Right;
        }
    }
    
    private void ReadServiceDirection()
    {
        if (_hitMethod == null) return;
        
        var hittingCenter = ActionMapper.GetForward();
        var hittingLeft = ActionMapper.GetLeft();
        var hittingRight = ActionMapper.GetRight();

        if (hittingCenter)
        {
            _hitDirectionHoriz = HitDirectionHorizontal.Center;
        } 
        else if (hittingLeft)
        {
            _hitDirectionHoriz = HitDirectionHorizontal.Left;
        } 
        else if (hittingRight)
        {
            _hitDirectionHoriz = HitDirectionHorizontal.Right;
        }
    }

    private void TossServiceBall() // Called as animation event
    {
        ball.TelePort(attachedBallParent.position);
        SwitchBallType(false);
        ball.HitBall(hitServiceBallSpawn.position, serviceTossSpeed, false, false);
    }

    private void HitServiceBall() // Called as animation event
    {
        var targetPosition = _courtManager.GetServiceTargetPosition(playerId, _hitDirectionHoriz);
        targetPosition = RandomizeBallTarget(targetPosition, serveBallTargetRadius);
        ball.HitBall(targetPosition, TennisVariables.ServiceSpeed, false, true, TennisVariables.ServiceYAttenuation);
        _soundManager.PlayService(_audioSource);
        _hitDirectionHoriz = null;
        _hitMethod = null;
        hitServiceBall = true;
        
        gameManager.PlayerHitBall(ball.GetPosition(),targetPosition);
    }

    public void SwitchBallType(bool attachBall) 
    {
        attachedBall.SetActive(attachBall);
        ball.gameObject.SetActive(!attachBall);
    }

    private void Step(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5)
            _soundManager.PlayFootstep(_audioSource);
    }

    public void HitBall() // Called as animation event
    {
        _pointManager.SetPlayerHitBall(playerId);
        _pointManager.HandleBallBounce(null);
        
        ball.TelePort(hitBallSpawn.position);
        var targetPosition = _courtManager.GetHitTargetPosition(playerId, _hitDirectionVert, _hitDirectionHoriz);
        targetPosition = RandomizeBallTarget(targetPosition, ballTargetRadius);
        _soundManager.PlayRacquetHit(_audioSource);
        
        var speed = _hitDirectionVert == HitDirectionVertical.Deep
            ? TennisVariables.DeepHitSpeed
            : TennisVariables.FrontHitSpeed;
        var speedYAtt = _hitDirectionVert == HitDirectionVertical.Deep
            ? TennisVariables.DeepHitYAttenuation
            : TennisVariables.FrontHitYAttenuation;
        
        ball.HitBall(targetPosition, speed, _hitDirectionVert == HitDirectionVertical.Dropshot, true, speedYAtt);
        _hitDirectionVert = null;
        _hitDirectionHoriz = null;
        _hitMethod = null;

        gameManager.PlayerHitBall(ball.GetPosition(),targetPosition);
    }
    
    private Vector3 RandomizeBallTarget(Vector3 posEnd, float randomRadius)
    {
        return new Vector3(
            posEnd.x + Random.Range(-randomRadius, randomRadius),
            posEnd.y,
            posEnd.z + Random.Range(-randomRadius, randomRadius));
    }

    private void ResetHittingBall() // Called as animation event
    {
        _movementBlocked = false;
    }
    
    private void ResetHittingServiceBall() // Called as animation event
    {
        _movementBlocked = false;
        serveDone = true;
    }

    public Collider BallCollider => ballCollider;

    public void StopMovementAnimation()
    {
        _animator.SetFloat(_strafeHash, 0);
        _animator.SetFloat(_forwardHash, 0);
    }

    public void Cheer()
    {
        _animator.SetTrigger(_cheerHash);
    }
    
    public void Defeated()
    {
        _animator.SetTrigger(_defeatedHash);
    }

    public void ToggleCharacterController()
    {
        _characterController.enabled = !_characterController.enabled;
    }
    
    private void PlayerGrunt()
    {
        var rand = Random.value;
        if (rand > 0.85f)
            _soundManager.PlayGrunt(_audioSource);
    }
}
