using System;
using JetBrains.Annotations;
using UnityEngine;

public struct CourtArea
{
    public float DepthStart;
    public float DepthLimit;
    public float HorizontalStart;
    public float HorizontalLimit;
}

public class CourtSectionMapper
{
    public ScoreManager scoreManager;

    private enum Depth
    {
        Front = 0,
        Back = 1,
    }

    private enum Horizontal
    {
        Left = 0,
        Right = 1,
    }

    private enum AreaType
    {
        Full = 0,
        ServingSquare = 1,
    }


    public bool ServiceIsIn(Vector2Int coordinates)
    {
        var servingSide = scoreManager.GetServingSide();
        return servingSide == ScoreManager.ServingSide.Even ? EvenServiceIsIn(coordinates) : OddServiceIsIn(coordinates);
    }

    private bool EvenServiceIsIn(Vector2 coordinates)
    {
        var backLeftServingSquare = GenerateCourtArea(AreaType.ServingSquare, Depth.Back, Horizontal.Left);
        var frontRightServingSquare = GenerateCourtArea(AreaType.ServingSquare, Depth.Front, Horizontal.Right);
        
        // If human player (closer to camera) is serving
        if (scoreManager.GetServingPlayerId() == scoreManager.player1Id)
        {
            return CoordinatesBelongToArea(coordinates, backLeftServingSquare);
        }
        return CoordinatesBelongToArea(coordinates, frontRightServingSquare);
    }
    
    private bool OddServiceIsIn(Vector2 coordinates)
    {
        var backRightServingSquare = GenerateCourtArea(AreaType.ServingSquare, Depth.Back, Horizontal.Right);
        var frontLeftServingSquare = GenerateCourtArea(AreaType.ServingSquare, Depth.Front, Horizontal.Left);

        // If human player (closer to camera) is serving
        if (scoreManager.GetServingPlayerId() == scoreManager.player1Id)
        {
            return CoordinatesBelongToArea(coordinates, backRightServingSquare);
        }
        return CoordinatesBelongToArea(coordinates, frontLeftServingSquare);
    }

    public bool BallIsIn(Vector2 coordinates, PointManager.CourtTarget target)
    {
        var frontFullArea = GenerateCourtArea(AreaType.Full, Depth.Front, null);
        var backFullArea = GenerateCourtArea(AreaType.Full, Depth.Back, null);

        // If human player (closer to camera) is receiving and target is his court or if he is serving and his court is target
        if ((target == PointManager.CourtTarget.Receiver && scoreManager.GetReceivingPlayerId() == scoreManager.player1Id) ||
            (target == PointManager.CourtTarget.Server && scoreManager.GetServingPlayerId() == scoreManager.player1Id))
        {
            return CoordinatesBelongToArea(coordinates, frontFullArea);
        }
        return CoordinatesBelongToArea(coordinates, backFullArea);
    }

    private bool CoordinatesBelongToArea(Vector2 collision, CourtArea area)
    {
        return collision.x > area.HorizontalStart &&
               collision.x < area.HorizontalLimit &&
               collision.y > area.DepthStart &&
               collision.y < area.DepthLimit;
    }

    private static CourtArea GenerateCourtArea(AreaType type,  Depth depth, [CanBeNull] Horizontal? horizontal)
    {
        /*
         * References (left, right, front, back) are considered from the camera perspective
         */
        CourtArea area = new CourtArea();
        if (depth == Depth.Front)
        {
            if (type == AreaType.Full)
            {
                //COORDINATES FOR FRONT FULL COURT
                area.DepthLimit = 0;
                area.DepthStart = -39.155f;
                area.HorizontalLimit = 14f;
                area.HorizontalStart = -13.82f;
            }
            else if(type == AreaType.ServingSquare)
            {
                if (horizontal == Horizontal.Left)
                {
                    //COORDINATES FOR FRONT LEFT SERVING SQUARE
                    area.DepthLimit = 0;
                    area.DepthStart = -21.38f;
                    area.HorizontalLimit = 14f;
                    area.HorizontalStart = -0.21f;
                }
                else if(horizontal == Horizontal.Right)
                {
                    //COORDINATES FOR FRONT RIGHT SERVING SQUARE
                    area.DepthLimit = 0;
                    area.DepthStart = -21.38f;
                    area.HorizontalLimit = 0.145f;
                    area.HorizontalStart = -13.82f;
                }
            }
        }
        else if(depth == Depth.Back)
        {
            if (type == AreaType.Full)
            {
                //COORDINATES FOR BACK FULL COURT
                area.DepthLimit = 39.015f;
                area.DepthStart = 0.044f;
                area.HorizontalLimit = 14f;
                area.HorizontalStart = -13.82f;
            }
            else if(type == AreaType.ServingSquare)
            {
                if (horizontal == Horizontal.Left)
                {
                    //COORDINATES FOR BACK LEFT SERVING SQUARE
                    area.DepthLimit = 21.176f;
                    area.DepthStart = 0.044f;
                    area.HorizontalLimit = 14f;
                    area.HorizontalStart = -0.21f;
                }
                else if(horizontal == Horizontal.Right)
                {
                    //COORDINATES FOR BACK RIGHT SERVING SQUARE
                    area.DepthLimit = 21.176f;
                    area.DepthStart = 0.044f;
                    area.HorizontalLimit = 0.145f;
                    area.HorizontalStart = -13.82f;
                }
            }
        }

        return area;
    }

}