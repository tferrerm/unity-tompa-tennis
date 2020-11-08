﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointManager : MonoBehaviour
{
    private bool _ballCollidedWithCourt;
    private bool _ballCollidedWithServerRacket;
    private bool _ballCollidedWithReceiverRacket;
    
    private const float NextPointWaitingTime = 3f;
    
    private ScoreManager _scoreManager;
    public CourtSectionMapper courtSectionMapper;
    public Player player1;

    private enum PointState
    {
        FirstServe = 0,
        SecondServe = 1,
        ReceiverTurn = 2,
        ReceiverHit = 3,
        ServerTurn = 4,
        ServerHit = 5,
        WaitingFirstServe = 6,
        WaitingSecondServe = 7
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
        courtSectionMapper = new CourtSectionMapper(_scoreManager);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void HandleBallBounce(Vector2? bounceCoordinates)
    {
        //if (ballCollidedWithCourt)
        //{
            /*
             * Set collision coordinates
             */
        //    bounceCoordinates = new Vector2Int();
        //}
        
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
                        _pointState = PointState.WaitingSecondServe;
                        ResetPoint(PointState.SecondServe);
                    }
                    else
                    {
                        //Double fault
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
                    _scoreManager.WinPoint(_scoreManager.GetReceivingPlayerId());
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
                    _scoreManager.WinPoint(_scoreManager.GetReceivingPlayerId());
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
        StartCoroutine(WaitForNextServe(nextServeState));
    }

    private IEnumerator WaitForNextServe(PointState nextServeState)
    {
        yield return new WaitForSeconds(NextPointWaitingTime);
        _pointState = nextServeState;
        if (_scoreManager.GetServingPlayerId() == 0)
        {
            player1.SwitchBallType(true);
        }
        else
        {
            // player2.SwitchBallType(true);
        }
    }

    public bool IsServing(int playerId)
    {
        return (_pointState == PointState.FirstServe || _pointState == PointState.SecondServe) && _scoreManager.GetServingPlayerId() == playerId;
    }

    public bool CanHitBall(int playerId)
    {
        return true; // TODO player can hit ball only if incoming ball, prevent hitting twice
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
}
