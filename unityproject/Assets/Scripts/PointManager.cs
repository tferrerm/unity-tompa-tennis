using System;
using System.Collections;
using UnityEngine;

public class PointManager : MonoBehaviour
{
    private bool _ballCollidedWithCourt;
    private bool _ballCollidedWithServerRacket;
    private bool _ballCollidedWithReceiverRacket;
    
    private const float NextPointWaitingTime = 3f; // Waiting time before point reset

    private CourtManager _courtManager;
    private ScoreManager _scoreManager;
    public CourtSectionMapper courtSectionMapper;
    private Player _player1;
    private AIPlayer _player2;
    private ReplayManager _replayManager;

    public enum PointState
    {
        FirstServeBallToss = 0,
        FirstServe = 1,
        SecondServeBallToss = 2,
        SecondServe = 3,
        ReceiverTurn = 4,
        ReceiverHit = 5,
        ServerTurn = 6,
        ServerHit = 7,
        WaitingFirstServe = 8,
        WaitingSecondServe = 9
    }

    public enum CourtTarget
    {
        Receiver = 0,
        Server = 1,
    }

    private PointState _pointState = PointState.FirstServe;

    private bool _isServiceFault;
    private PointState _nextState;
    private IEnumerator _nextServeCoroutine;

    private void Awake()
    {
        _courtManager = GetComponent<CourtManager>();
        _scoreManager = GetComponent<ScoreManager>();
        courtSectionMapper = new CourtSectionMapper(_scoreManager);
        var gameManager = GetComponent<GameManager>();
        _player1 = gameManager.player;
        _player2 = gameManager.aiPlayer;
        _replayManager = GetComponent<ReplayManager>();
    }

    // Start is called before the first frame update
    void Start()
    {
        ResetPlayers();
        AIPlayerServiceCheck();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void HandleBallBounce(Vector2? bounceCoordinates)
    {
        switch (_pointState)
        {
            case PointState.FirstServe:
            case PointState.SecondServe:
                if ( _ballCollidedWithCourt && courtSectionMapper.ServiceIsIn(bounceCoordinates.GetValueOrDefault()))
                {
                    // Service is in
                    _pointState = PointState.ReceiverTurn;
                }
                else
                {
                    // Service is out
                    if (_pointState == PointState.FirstServe)
                    {
                        _isServiceFault = true;
                        _pointState = PointState.WaitingSecondServe;
                        ResetPoint(PointState.SecondServe);
                    }
                    else
                    {
                        //Double fault
                        _isServiceFault = true;
                        _pointState = PointState.WaitingFirstServe;
                        _scoreManager.WinPoint(_scoreManager.GetReceivingPlayerId());
                        ResetPoint(PointState.FirstServe);
                    }
                }
                break;
            case PointState.ReceiverTurn:
                if (_ballCollidedWithReceiverRacket)
                {
                    _pointState = PointState.ReceiverHit;
                }
                else
                {
                    // Collide was anywhere else, receiver didn't reach the ball before second bounce
                    _pointState = PointState.WaitingFirstServe;
                    _scoreManager.WinPoint(_scoreManager.GetServingPlayerId());
                    _replayManager.SetCameraPosition(_scoreManager.GetServingPlayerId());
                    ResetPoint(PointState.FirstServe);
                }
                break;
            case PointState.ReceiverHit:
                if (_ballCollidedWithServerRacket)
                {
                    _pointState = PointState.ServerHit;
                }
                else if(_ballCollidedWithCourt && courtSectionMapper.BallIsIn(bounceCoordinates.GetValueOrDefault(), CourtTarget.Server))
                {
                    _pointState = PointState.ServerTurn;
                }
                else
                {
                    // Receiver hit was out
                    _pointState = PointState.WaitingFirstServe;
                    _scoreManager.WinPoint(_scoreManager.GetServingPlayerId());
                    _replayManager.SetCameraPosition(_scoreManager.GetServingPlayerId());
                    ResetPoint(PointState.FirstServe);
                }
                break;
            case PointState.ServerTurn:
                if (_ballCollidedWithServerRacket)
                {
                    _pointState = PointState.ServerHit;
                }
                else
                {
                    // Collide was anywhere else, server didn't reach the ball before second bounce
                    _pointState = PointState.WaitingFirstServe;
                    _scoreManager.WinPoint(_scoreManager.GetReceivingPlayerId());
                    _replayManager.SetCameraPosition(_scoreManager.GetReceivingPlayerId());
                    ResetPoint(PointState.FirstServe);
                }
                break;
            case PointState.ServerHit:
                if (_ballCollidedWithReceiverRacket)
                {
                    _pointState = PointState.ReceiverHit;
                }
                else if(courtSectionMapper.BallIsIn(bounceCoordinates.GetValueOrDefault(), CourtTarget.Receiver))
                {
                    _pointState = PointState.ReceiverTurn;
                }
                else
                {
                    // Server hit was out
                    _pointState = PointState.WaitingFirstServe;
                    _scoreManager.WinPoint(_scoreManager.GetReceivingPlayerId());
                    _replayManager.SetCameraPosition(_scoreManager.GetReceivingPlayerId());
                    ResetPoint(PointState.FirstServe);
                }
                break;
            case PointState.WaitingFirstServe:
            case PointState.WaitingSecondServe:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void ResetPoint(PointState nextServeState)
    {
        _ballCollidedWithCourt = false;
        _ballCollidedWithReceiverRacket = false;
        _ballCollidedWithServerRacket = false;
        
        _player2.ResetTargetMovementVariables();
        _player2.MovingToCenter = false;
        _player2.VolleyModeActivated = false;

        _nextState = nextServeState;
        _nextServeCoroutine = WaitForNextServe();
        StartCoroutine(_nextServeCoroutine);
    }

    private IEnumerator WaitForNextServe()
    {
        _player1.StopMovementAnimation();
        yield return new WaitForSeconds(NextPointWaitingTime);

        if (!_isServiceFault)
        {
            _replayManager.InitializeReplay();
            yield return new WaitForSeconds(ReplayManager.MaxRecordingTime); // TODO CHANGE WAITING TIME
            _replayManager.StopReplay();
        }
        else
        {
            _isServiceFault = false;
        }

        _player1.StopMovementAnimation();
        ResetPlayers();
        AIPlayerServiceCheck();
        _pointState = _nextState;
    }

    public void StopWaitForNextServe()
    {
        _replayManager.StopReplay();
        StopCoroutine(_nextServeCoroutine);
        _player1.StopMovementAnimation();
        ResetPlayers();
        AIPlayerServiceCheck();
        _pointState = _nextState;
    }

    private void AIPlayerServiceCheck()
    {
        if (_scoreManager.GetServingPlayerId() == _player2.playerId)
        {
            var coroutine = _player2.StartService();
            StartCoroutine(coroutine);
        }
    }

    private void ResetPlayers()
    {
        TogglePlayerCharacterControllers();
        
        var currentServingSide = _scoreManager.currentServingSide;
        _player1.transform.position = currentServingSide == ScoreManager.ServingSide.Even ? 
            _courtManager.player1ServiceSpotRight.position : _courtManager.player1ServiceSpotLeft.position;
        _player2.transform.position = currentServingSide == ScoreManager.ServingSide.Even ? 
            _courtManager.player2ServiceSpotLeft.position : _courtManager.player2ServiceSpotRight.position;

        if (_scoreManager.GetServingPlayerId() == 0)
        {
            _player1.SwitchBallType(true);
            _player1.serveDone = false;
            _player1.hitServiceBall = false;
        }
        else
        {
            _player2.SwitchBallType(true);
        }

        TogglePlayerCharacterControllers();
    }

    private void TogglePlayerCharacterControllers()
    {
        _player1.ToggleCharacterController();
        _player2.ToggleCharacterController();
    }

    public bool IsServing(int playerId)
    {
        return (_pointState == PointState.FirstServe || _pointState == PointState.SecondServe) && _scoreManager.GetServingPlayerId() == playerId;
    }

    public bool CanHitBall(int playerId)
    {
        var isServing = _scoreManager.GetServingPlayerId() == playerId;
        return _pointState == (isServing ? 
            PointState.ServerTurn : PointState.ReceiverTurn) || _pointState == (isServing ? 
            PointState.ReceiverHit : PointState.ServerHit);
    }

    public void SetPlayerHitBall(int playerId)
    {
        if (playerId == _scoreManager.GetReceivingPlayerId())
        {
            _ballCollidedWithReceiverRacket = true;
            _ballCollidedWithServerRacket = false;
        }
        else
        {
            _ballCollidedWithServerRacket = true;
            _ballCollidedWithReceiverRacket = false;
        }

        _ballCollidedWithCourt = false;
    }

    public void SetCourtBallBounce()
    {
        _ballCollidedWithCourt = true;
    }
    
    public bool ServicePositionOutOfBounds(int playerId, float posZ)
    {
        var serviceSide = _scoreManager.currentServingSide;
        if ((playerId == _player1.playerId && serviceSide == ScoreManager.ServingSide.Even) || 
            (playerId == _player2.playerId && serviceSide == ScoreManager.ServingSide.Odd))
        {
            // Right boundaries
            return !courtSectionMapper.PositionInHorizontalArea(posZ, CourtSectionMapper.Horizontal.Right);
        }
        else
        {
            // Left boundaries
            return !courtSectionMapper.PositionInHorizontalArea(posZ, CourtSectionMapper.Horizontal.Left);
        }
    }

    public bool PositionInVolleyArea(int playerId, float posX)
    {
        return courtSectionMapper.PositionInVolleyArea(posX, playerId == _player1.playerId ? 
            CourtSectionMapper.Depth.Front : CourtSectionMapper.Depth.Back);
    }

    public PointState GetPointState()
    {
        return _pointState;
    }
}
