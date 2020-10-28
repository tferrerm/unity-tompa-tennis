using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointManager : MonoBehaviour
{
    private ScoreManager _scoreManager;
    public CourtSectionMapper courtSectionMapper;

    private enum PointState
    {
        FirstServe = 0,
        SecondServe = 1,
        ReceiverTurn = 2,
        ReceiverHit = 3,
        ServerTurn = 4,
        ServerHit = 5,
    }

    public enum CourtTarget
    {
        Receiver = 0,
        Server = 1,
    }

    private PointState _pointState = PointState.FirstServe;
    // Start is called before the first frame update
    void Start()
    {
        _scoreManager = GetComponent<ScoreManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void HandleBallBounce()
    {
        bool ballCollidedWithCourt = false;
        bool ballCollidedWithServerRacket = false;
        bool ballCollidedWithReceiverRacket = false;
        var bounceCoordinates = new Vector2Int();
        
        if (ballCollidedWithCourt)
        {
            /*
             * Set collision coordinates
             */
            bounceCoordinates = new Vector2Int();
        }
        
        switch (_pointState)
        {
            case PointState.FirstServe:
            case PointState.SecondServe:
                if ( ballCollidedWithCourt && courtSectionMapper.ServiceIsIn(bounceCoordinates))
                {
                    // Service is in
                    _pointState = PointState.ReceiverTurn;
                }
                else
                {
                    // Service is out
                    if (_pointState == PointState.FirstServe)
                    {
                        _pointState = PointState.SecondServe;
                    }
                    else
                    {
                        //Double fault
                        _pointState = PointState.FirstServe;
                        _scoreManager.WinPoint(_scoreManager.GetReceivingPlayerId());
                        ResetPoint();
                    }
                }
                break;
            case PointState.ReceiverTurn:
                if (ballCollidedWithReceiverRacket)
                {
                    _pointState = PointState.ReceiverHit;
                }
                else
                {
                    // Collide was anywhere else, receiver didn't reach the ball before second bounce
                    _scoreManager.WinPoint(_scoreManager.GetServingPlayerId());
                }
                break;
            case PointState.ReceiverHit:
                if (ballCollidedWithServerRacket)
                {
                    _pointState = PointState.ServerHit;
                }
                else if(ballCollidedWithCourt && courtSectionMapper.BallIsIn(bounceCoordinates, CourtTarget.Server))
                {
                    _pointState = PointState.ServerTurn;
                }
                else
                {
                    // Receiver hit was out
                    _scoreManager.WinPoint(_scoreManager.GetServingPlayerId());
                    ResetPoint();
                }
                break;
            case PointState.ServerTurn:
                if (ballCollidedWithServerRacket)
                {
                    _pointState = PointState.ServerHit;
                }
                else
                {
                    // Collide was anywhere else, server didn't reach the ball before second bounce
                    _scoreManager.WinPoint(_scoreManager.GetReceivingPlayerId());
                    ResetPoint();
                }
                break;
            case PointState.ServerHit:
                if (ballCollidedWithReceiverRacket)
                {
                    _pointState = PointState.ReceiverHit;
                }
                else if(courtSectionMapper.BallIsIn(bounceCoordinates, CourtTarget.Receiver))
                {
                    _pointState = PointState.ReceiverTurn;
                }
                else
                {
                    // Server hit was out
                    _scoreManager.WinPoint(_scoreManager.GetReceivingPlayerId());
                    ResetPoint();
                }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void ResetPoint()
    {
        _pointState = PointState.FirstServe;
    }

    public bool IsServing(int playerId)
    {
        return (_pointState == PointState.FirstServe || _pointState == PointState.SecondServe) 
               && _scoreManager.GetServingPlayerId() == playerId;
    }
}
