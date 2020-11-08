using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
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
    Back = 0
}

public class Player : MonoBehaviour
{
    public int playerId;
    public string playerName;
    
    public float runSpeed = 8;
    public float sprintSpeed = 10;
    public float backSpeed = 5.5f;

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

    private float _moveLeftRightValue;
    private float _moveUpDownValue;

    public Ball ball;
    public GameObject attachedBall; // Ball copy attached to player's hand for service animation
    public Transform attachedBallParent;

    public GameManager gameManager;
    private CourtManager _courtManager;
    private PointManager _pointManager;
    private SoundManager _soundManager;
    
    private readonly Dictionary<HitMethod, TechniqueAttributes> _techniqueAttrs = new Dictionary<HitMethod, TechniqueAttributes>();

    // Ball interaction variables
    public Collider ballCollider;
    [HideInInspector] public bool ballInsideHitZone;
    private HitMethod? _hitMethod;
    private bool _hittingBall;
    private HitDirectionHorizontal? _hitDirectionHoriz;
    private HitDirectionVertical? _hitDirectionVert;
    public Transform hitBallSpawn;
    public Transform hitServiceBallSpawn;
    private float serviceTossSpeed = 20f;
    
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

        InitTechniqueAttrs();
    }

    private void InitTechniqueAttrs()
    {
        _techniqueAttrs.Add(HitMethod.Drive, new TechniqueAttributes(0.1f, 2.25f));
        _techniqueAttrs.Add(HitMethod.Backhand, new TechniqueAttributes(0.1f, 2.25f));
        _techniqueAttrs.Add(HitMethod.Serve, new TechniqueAttributes(0.1f, 1f));
    }

    private void CalculateAnimatorHashes()
    {
        _strafeHash = Animator.StringToHash("Strafe");
        _forwardHash = Animator.StringToHash("Forward");
        _serviceTriggerHash = Animator.StringToHash("Service Trigger");
        _serviceStartHash = Animator.StringToHash("Service Start");
        _serviceEndHash = Animator.StringToHash("Service End");
        _driveHash = Animator.StringToHash("Drive Trigger");
        _backhandHash = Animator.StringToHash("Backhand Trigger");   
    }
    
    void Update()
    {
        ReadInput();
        Move();
    }

    private void Move()
    {
        if (_hittingBall) return;
        
        float dt = Time.deltaTime;
        Vector2 movingDir = new Vector2(_moveLeftRightValue, _moveUpDownValue);
        float manhattanNorm = Math.Abs(movingDir[0]) + Math.Abs(movingDir[1]);
        if (manhattanNorm == 0)
            manhattanNorm = 1;
        float spd = (ActionMapper.IsSprinting() ? sprintSpeed : runSpeed) * movingDir.magnitude;
        float dx = dt * (_moveUpDownValue < 0 ? backSpeed : spd) * _moveUpDownValue / manhattanNorm;
        float dz = dt * spd * _moveLeftRightValue / manhattanNorm;

        _characterController.SimpleMove(Vector3.zero);
        
        Vector3 move = new Vector3(dx, 0, dz);

        _animator.SetFloat(_strafeHash, _moveLeftRightValue);
        _animator.SetFloat(_forwardHash, _moveUpDownValue);
        _characterController.Move(move);
    }

    private void ReadInput()
    {
        if (ActionMapper.RacquetSwing())
        {
            if (_pointManager.IsServing(playerId))
            {
                StartService();
            } else if (CanHitBall())
            {
                ballInsideHitZone = false; // Cannot hit ball twice
                SelectHitMethod();
                _hittingBall = true;
            }
        }

        if (_hittingBall)
        {
            ReadHitDirection();
        }
        else
        {
            ReadMovementInput();
        }
    }

    private void ReadMovementInput()
    {
        var currentStateHash = _animator.GetCurrentAnimatorStateInfo(0).shortNameHash;
        if (_hittingBall || currentStateHash == _serviceStartHash || currentStateHash == _serviceEndHash) // TODO ADD SERVICE BOOL INSTEAD?
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
               ball.transform.position.x > transform.position.x + 2;
    }

    private void SelectHitMethod()
    {
        var ballPosition = ball.transform.position;
        if (ballPosition.z < transform.position.z) // Should take into account ball direction
        {
            _animator.SetTrigger(_driveHash);
            _hitMethod = HitMethod.Drive;
        }
        else
        {
            _animator.SetTrigger(_backhandHash);
            _hitMethod = HitMethod.Backhand;
        }
        _hitDirectionHoriz = HitDirectionHorizontal.Center; // Default value if no keys pressed
        _hitDirectionVert = HitDirectionVertical.Back;
    }

    private void StartService()
    {
        _animator.SetTrigger(_serviceTriggerHash);
        SwitchBallType(true);
        _hitMethod = HitMethod.Serve;
    }

    private void ReadHitDirection()
    {
        if (_hitMethod == null) return;
        
        var hittingStraight = ActionMapper.GetForward();
        var hittingLeft = ActionMapper.GetLeft();
        var hittingRight = ActionMapper.GetRight();
        
        if (hittingStraight) // Vertical input resets horizontal input
        {
            _hitDirectionVert = HitDirectionVertical.Back;
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

    private void TossServiceBall() // Called as animation event
    {
        ball.TelePort(attachedBallParent.position);
        SwitchBallType(false);
        ball.HitBall(hitServiceBallSpawn.position, serviceTossSpeed, false);
    }

    private void HitServiceBall() // Called as animation event
    {
        var position = _courtManager.player2ServiceLeftLeft.position;
        ball.HitBall(position, 200f, true, 250f);
        _soundManager.PlayService(_audioSource);
        gameManager.PlayerHitBall(ball.GetPosition(),position);
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
        Vector3 targetPosition = _courtManager.GetHitTargetPosition(playerId, _hitDirectionVert, _hitDirectionHoriz);
        ball.HitBall(targetPosition, 35f, true, 200f);
        _hitDirectionVert = null;
        _hitDirectionHoriz = null;
        _hitMethod = null;

        gameManager.PlayerHitBall(ball.GetPosition(),targetPosition);
    }

    private void ResetHittingBall() // Called as animation event
    {
        _hittingBall = false;
    }

    public Collider BallCollider => ballCollider;
}
