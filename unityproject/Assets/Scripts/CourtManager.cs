using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CourtManager : MonoBehaviour
{
    private const float TotalCourtWidth = 18.0f;
    private const float CourtLength = 39.0f;
    private const float ServingAreaWidth = 13.5f;
    private const float ServingAreaLength = 21.0f;
    
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
}
