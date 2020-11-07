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
    
    [Header("Service Hit Targets - AI Player Left Side")]
    public Transform player2ServiceLeftLeft;
    public Transform player2ServiceLeftCenter;
    public Transform player2ServiceLeftRight;
    
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

    // Start is called before the first frame update
    void Start()
    {
        
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
}
