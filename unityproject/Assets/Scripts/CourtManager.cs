using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CourtManager : MonoBehaviour
{
    private const float TotalCourtWidth = 18.0f;
    private const float CourtLength = 39.0f;
    private const float ServingAreaWidth = 13.5f;
    private const float ServingAreaLength = 21.0f;

    [Header("Deep Hit Targets - AI Player Side")]
    public Transform player2BackCenterHit;
    public Transform player2BackLeftHit;
    public Transform player2BackRightHit;
    
    [Header("Service Hit Spots")]
    public Transform player1ServiceSpotLeft;
    public Transform player1ServiceSpotRight;
    
    [Header("Service Hit Targets - AI Player Left Side")]
    public Transform player2ServiceLeftLeft;
    public Transform player2ServiceLeftCenter;
    public Transform player2ServiceLeftRight;
    
    [Header("Service Hit Targets - AI Player Right Side")]
    public Transform player2ServiceRightLeft;
    public Transform player2ServiceRightCenter;
    public Transform player2ServiceRightRight;
    
    public enum CourtSection
    {
        Player1NoMansLand,
        Player1LeftServiceBox,
        Player1RightServiceBox,
        Player2NoMansLand,
        Player2LeftServiceBox,
        Player2RightServiceBox
    }
    
    public Dictionary<CourtSection, Vector2> courtSections = new Dictionary<CourtSection,Vector2>();

    private ScoreManager _scoreManager;

    // Start is called before the first frame update
    void Start()
    {
        _scoreManager = GetComponent<ScoreManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Vector3 GetHitTargetPosition(int playerId, HitDirectionVertical? vertical, HitDirectionHorizontal? horiz)
    {
        if (playerId == 0)
        {
            if (vertical == HitDirectionVertical.Back)
            {
                return (horiz == HitDirectionHorizontal.Center) ? player2BackCenterHit.position :
                    (horiz == HitDirectionHorizontal.Left) ? player2BackLeftHit.position : player2BackRightHit.position;
            }
        }
        else
        {
            
        }
        
        return Vector3.zero;
    }

    public Vector3 GetServiceTargetPosition(int playerId, HitDirectionHorizontal? horiz)
    {
        var serviceSide = _scoreManager.currentServingSide;
        if (serviceSide == ScoreManager.ServingSide.Even)
        {
            if (playerId == 0)
            {
                return (horiz == HitDirectionHorizontal.Center) ? player2ServiceLeftCenter.position :
                    (horiz == HitDirectionHorizontal.Left) ? player2ServiceLeftLeft.position : player2ServiceLeftRight.position;
            }
            else
            {
                // TODO
            }
        }
        else
        {
            if (playerId == 0)
            {
                return (horiz == HitDirectionHorizontal.Center) ? player2ServiceRightCenter.position :
                    (horiz == HitDirectionHorizontal.Left) ? player2ServiceRightLeft.position : player2ServiceRightRight.position;
            }
            else
            {
                // TODO
            }
        }
        
        return Vector3.zero;
    }
}
