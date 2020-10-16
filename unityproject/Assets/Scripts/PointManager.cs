using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointManager : MonoBehaviour
{
    public ScoreManager scoreManager;
    public enum PointState
    {
        FirstServe = 0,
        SecondServe = 1,
        ReceiverTurn = 2,
        ReceiverHit = 3,
        ServerTurn = 4,
        ServerHit = 5,
    }
    

    private PointState _pointState = PointState.FirstServe;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void HandleBallBounce()
    {
        switch (_pointState)
        {
            case PointState.FirstServe:
            case PointState.SecondServe:
                if ( scoreManager.GetServingSide() == ScoreManager.ServingSide.Even  /* REPLACE && el colide fue en el cuadrado de la IZQUIERDA DEL RECEPTOR*/ ||
                     scoreManager.GetServingSide() == ScoreManager.ServingSide.Odd  /* REPLACE && el colide fue en el cuadrado de la DERECHA DEL RECEPTOR*/)
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
                            scoreManager.WinPoint(scoreManager.GetReceivingPlayerId());
                            ResetPoint();
                        }
                    }
                break;
            case PointState.ReceiverTurn:
                if (false /* REPLACE el colide fue en la RAQUETA DEL RECEIVER */)
                {
                    _pointState = PointState.ReceiverHit;
                }
                else
                {
                    // Collide was anywhere else, receiver didn't reach the ball before second bounce
                    scoreManager.WinPoint(scoreManager.GetServingPlayerId());
                }
                break;
            case PointState.ReceiverHit:
                if (false /* REPLACE el collide fue en la RAQUETA DEL SERVER */)
                {
                    _pointState = PointState.ServerHit;
                }
                else if(false /*REPLACE el collide fue en la CANCHA DEL SERVER */)
                {
                    _pointState = PointState.ServerTurn;
                }
                else
                {
                    // Receiver hit was out
                    scoreManager.WinPoint(scoreManager.GetServingPlayerId());
                    ResetPoint();
                }
                break;
            case PointState.ServerTurn:
                if (false /* REPLACE el colide fue en la RAQUETA DEL SERVER */)
                {
                    _pointState = PointState.ServerHit;
                }
                else
                {
                    // Collide was anywhere else, server didn't reach the ball before second bounce
                    scoreManager.WinPoint(scoreManager.GetReceivingPlayerId());
                    ResetPoint();
                }
                break;
            case PointState.ServerHit:
                if (false /* REPLACE el collide fue en la RAQUETA DEL RECEIVER */)
                {
                    _pointState = PointState.ReceiverHit;
                }
                else if(false /* REPLACE el collide fue en la CANCHA DEL RECEIVER */)
                {
                    _pointState = PointState.ReceiverTurn;
                }
                else
                {
                    // Server hit was out
                    scoreManager.WinPoint(scoreManager.GetReceivingPlayerId());
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
}
