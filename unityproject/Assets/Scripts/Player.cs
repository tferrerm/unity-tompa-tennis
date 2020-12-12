using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
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

    private CharacterController _characterController;

    private AudioSource _audioSource;
    
    private Animator _animator;
    public AnimationClip cheerClip;
    public AnimationClip defeatedClip;
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
    private TennisVariables _tv;
    
    //private readonly Dictionary<HitMethod, TechniqueAttributes> _techniqueAttrs = new Dictionary<HitMethod, TechniqueAttributes>();

    // Ball interaction variables
    public Collider ballCollider;
    [HideInInspector] public bool ballInsideHitZone;
    private HitMethod? _hitMethod;
    private bool _movementBlocked;
    private HitDirectionHorizontal? _hitDirectionHoriz;
    private HitDirectionVertical? _hitDirectionVert;
    public Transform hitBallSpawn;
    public Transform hitServiceBallSpawn;
    [HideInInspector] public bool serveDone; // Prevents movement until serve animation is finished
    [HideInInspector] public bool hitServiceBall; // Prevents serving twice in the same point
    public Transform driveVolleyHitBallSpawn;
    public Transform backhandVolleyHitBallSpawn;

    [Header("Input Actions")]
    public InputAction wasd;
    public InputAction hitBall;
    private ActionMapper _actionMapper;

    public List<RecordedReplayInfo> recordedReplayInfo;
    private int _recordingLimit;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _audioSource = GetComponent<AudioSource>();
        _animator = GetComponent<Animator>();
        _actionMapper = new ActionMapper(wasd, hitBall);
        _recordingLimit = TennisVariables.RecordingLimit();
        recordedReplayInfo = new List<RecordedReplayInfo>(_recordingLimit);
    }

    private void OnEnable()
    {
        wasd.Enable();
        hitBall.Enable();
    }

    private void OnDisable()
    {
        wasd.Disable();
        hitBall.Disable();
    }

    void Start()
    {
        _courtManager = gameManager.courtManager;
        _pointManager = gameManager.pointManager;
        _soundManager = gameManager.soundManager;
        _tv = gameManager.tennisVariables;
        
        CalculateAnimatorHashes();

        //InitTechniqueAttrs();
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
        if (!TennisVariables.isPlayingReplay)
        {
            ReadInput();
            Move();
        }
    }

    private void FixedUpdate()
    {
        if (TennisVariables.isRecording)
        {
            RecordPosition();
        }
        else if (TennisVariables.isPlayingReplay && recordedReplayInfo.Count > 0)
        {
            PlayRecording();
        }
    }

    private void RecordPosition()
    {
        var tf = transform;
        recordedReplayInfo.Add(new RecordedReplayInfo(tf.position, tf.rotation));
        if (recordedReplayInfo.Count > _recordingLimit)
        {
            recordedReplayInfo.RemoveAt(0);
        }
    }

    // TODO: fix animation when hitting ball. Store some hitting Ball variable in RecordedReplayInfo
    private void PlayRecording()
    {
        var recordedInfo = recordedReplayInfo[0];
        var distance = recordedInfo.position - transform.position;
        _animator.SetFloat(_strafeHash, distance.z / Time.fixedDeltaTime);
        _animator.SetFloat(_forwardHash, distance.x / Time.fixedDeltaTime);
        transform.position = recordedInfo.position;
        transform.rotation = recordedInfo.rotation;
        recordedReplayInfo.RemoveAt(0);
    }

    public void InitializeRecordingPlay()
    {
        _characterController.enabled = false;
        _animator.SetFloat(_strafeHash, 0);
        _animator.SetFloat(_forwardHash, 0);
        wasd.Disable();
        hitBall.Disable();
    }

    public void StopRecordingPlay()
    {
        _characterController.enabled = true;
        _animator.SetFloat(_strafeHash, 0);
        _animator.SetFloat(_forwardHash, 0);
        wasd.Enable();
        hitBall.Enable();
    }

    private void Move()
    {
        if (_movementBlocked) return;

        var dt = Time.deltaTime;
        var movingDir = new Vector2(_moveLeftRightValue, _moveUpDownValue);
        
        if (_pointManager.IsServing(playerId) && !serveDone)
        {
            var posZ = transform.position.z;
            var spd = _tv.WalkSpeed * movingDir.magnitude;
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
            
            var spd = (_actionMapper.IsSprinting() ? _tv.SprintSpeed : _tv.RunSpeed) * movingDir.magnitude;
            var dx = transform.position.x < 0 ? dt * (_moveUpDownValue < 0 ? _tv.BackSpeed : spd) * _moveUpDownValue / manhattanNorm : 0;
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
        if (_actionMapper.RacquetSwing())
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
            _moveLeftRightValue = _actionMapper.GetMoveHorizontal(); 
            _moveUpDownValue = _actionMapper.GetMoveVertical();
        }
    }

    // Ball entered collision zone (sphere) and is in front of the player
    private bool CanHitBall()
    {
        return ballInsideHitZone && _pointManager.CanHitBall(playerId) &&
               ball.transform.position.x > transform.position.x + _tv.BallColliderFrontDelta;
    }

    private void SelectHitMethod()
    {
        var ballPosition = ball.transform.position;
        var ballSpeed = ball.GetSpeed();
        if (ballPosition.z < transform.position.z) // Should take into account ball direction
        {
            if (ballSpeed < _tv.FastHitAnimationThresholdSpeed)
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
            if (ballSpeed < _tv.FastHitAnimationThresholdSpeed)
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
        TennisVariables.isRecording = true;
        _animator.SetTrigger(_serviceTriggerHash);
        _hitMethod = HitMethod.Serve;
        _hitDirectionHoriz = HitDirectionHorizontal.Center;
    }

    private void ReadHitDirection()
    {
        if (_hitMethod == null) return;
        
        var hittingStraight = _actionMapper.GetForward();
        var hittingDropshot = _actionMapper.GetBackward();
        var hittingLeft = _actionMapper.GetLeft();
        var hittingRight = _actionMapper.GetRight();
        
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
        
        var hittingCenter = _actionMapper.GetForward();
        var hittingLeft = _actionMapper.GetLeft();
        var hittingRight = _actionMapper.GetRight();

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
        ball.Teleport(attachedBallParent.position);
        SwitchBallType(false);
        ball.HitBall(hitServiceBallSpawn.position, _tv.ServiceTossSpeed, false, false);
    }

    private void HitServiceBall() // Called as animation event
    {
        var targetPosition = _courtManager.GetServiceTargetPosition(playerId, _hitDirectionHoriz);
        targetPosition = RandomizeBallTarget(targetPosition, _tv.BallServeTargetRadius);
        ball.HitBall(targetPosition, _tv.ServiceSpeed, false, true, _tv.ServiceYAttenuation);
        _soundManager.PlayService(_audioSource);
        _hitDirectionHoriz = null;
        _hitMethod = null;
        hitServiceBall = true;
        
        gameManager.PlayerHitBall(ball.GetPosition());
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
        
        var isVolley = _pointManager.PositionInVolleyArea(playerId, transform.position.x);
        ball.Teleport(isVolley ? (_hitMethod == HitMethod.Drive ? 
            driveVolleyHitBallSpawn.position : backhandVolleyHitBallSpawn.position) : hitBallSpawn.position);
        
        var targetPosition = _courtManager.GetHitTargetPosition(playerId, _hitDirectionVert, _hitDirectionHoriz);
        targetPosition = RandomizeBallTarget(targetPosition, _tv.BallHitTargetRadius);
        _soundManager.PlayRacquetHit(_audioSource);

        float speed;
        float speedYAtt;
        if (_hitDirectionVert == HitDirectionVertical.Deep)
        {
            if (isVolley)
            {
                speed = _tv.DeepVolleyHitSpeed;
                speedYAtt = _tv.DeepVolleyHitYAttenuation;
            }
            else
            {
                speed = _tv.DeepHitSpeed;
                speedYAtt = _tv.DeepHitYAttenuation;
            }
        }
        else
        {
            if (isVolley)
            {
                speed = _tv.DropshotVolleyHitSpeed;
                speedYAtt = _tv.DropshotVolleyHitYAttenuation;
            }
            else
            {
                speed = _tv.DropshotHitSpeed;
                speedYAtt = _tv.DropshotHitYAttenuation;
            }
        }
        
        ball.HitBall(targetPosition, speed, _hitDirectionVert == HitDirectionVertical.Dropshot, true, speedYAtt);
        _hitDirectionVert = null;
        _hitDirectionHoriz = null;
        _hitMethod = null;

        gameManager.PlayerHitBall(ball.GetPosition());
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
        var coroutine = PointReactionCoroutine(_cheerHash);
        StartCoroutine(coroutine);
    }
    
    public void Defeated()
    {
        var coroutine = PointReactionCoroutine(_defeatedHash);
        StartCoroutine(coroutine);
    }

    private IEnumerator PointReactionCoroutine(int animationHash)
    {
        _movementBlocked = true;
        _animator.SetTrigger(animationHash);
        yield return new WaitForSeconds(animationHash == _cheerHash ? cheerClip.length : defeatedClip.length);
        _movementBlocked = false;
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
