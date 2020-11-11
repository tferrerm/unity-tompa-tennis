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
    private readonly CourtArea _backLeftServingSquare;
    private readonly CourtArea _frontRightServingSquare;
    private readonly CourtArea _backRightServingSquare;
    private readonly CourtArea _frontLeftServingSquare;
    private readonly CourtArea _frontFullArea;
    private readonly CourtArea _backFullArea;
    
    private ScoreManager _scoreManager;

    public CourtSectionMapper(ScoreManager scoreManager)
    {
        _backLeftServingSquare = GenerateCourtArea(AreaType.ServingSquare, Depth.Back, Horizontal.Left);
        _frontRightServingSquare = GenerateCourtArea(AreaType.ServingSquare, Depth.Front, Horizontal.Right);
        _backRightServingSquare = GenerateCourtArea(AreaType.ServingSquare, Depth.Back, Horizontal.Right);
        _frontLeftServingSquare = GenerateCourtArea(AreaType.ServingSquare, Depth.Front, Horizontal.Left);
        _frontFullArea = GenerateCourtArea(AreaType.Full, Depth.Front, null);
        _backFullArea = GenerateCourtArea(AreaType.Full, Depth.Back, null);
        
        _scoreManager = scoreManager;
    }

    private enum Depth
    {
        Front = 0,
        Back = 1,
    }

    public enum Horizontal
    {
        Left = 0,
        Right = 1,
    }

    private enum AreaType
    {
        Full = 0,
        ServingSquare = 1,
    }


    public bool ServiceIsIn(Vector2 coordinates)
    {
        var servingSide = _scoreManager.GetServingSide();
        return servingSide == ScoreManager.ServingSide.Even ? EvenServiceIsIn(coordinates) : OddServiceIsIn(coordinates);
    }

    private bool EvenServiceIsIn(Vector2 coordinates)
    {
        // If human player (closer to camera) is serving
        if (_scoreManager.GetServingPlayerId() == _scoreManager.player1Id)
        {
            return CoordinatesBelongToArea(coordinates, _backLeftServingSquare);
        }
        return CoordinatesBelongToArea(coordinates, _frontRightServingSquare);
    }
    
    private bool OddServiceIsIn(Vector2 coordinates)
    {
        // If human player (closer to camera) is serving
        if (_scoreManager.GetServingPlayerId() == _scoreManager.player1Id)
        {
            return CoordinatesBelongToArea(coordinates, _backRightServingSquare);
        }
        return CoordinatesBelongToArea(coordinates, _frontLeftServingSquare);
    }

    public bool BallIsIn(Vector2 coordinates, PointManager.CourtTarget target)
    {
        // If human player (closer to camera) is receiving and target is his court or if he is serving and his court is target
        if ((target == PointManager.CourtTarget.Receiver && _scoreManager.GetReceivingPlayerId() == _scoreManager.player1Id) ||
            (target == PointManager.CourtTarget.Server && _scoreManager.GetServingPlayerId() == _scoreManager.player1Id))
        {
            return CoordinatesBelongToArea(coordinates, _frontFullArea);
        }
        return CoordinatesBelongToArea(coordinates, _backFullArea);
    }

    private bool CoordinatesBelongToArea(Vector2 collision, CourtArea area)
    {
        return collision.y > area.HorizontalStart &&
               collision.y < area.HorizontalLimit &&
               collision.x > area.DepthStart &&
               collision.x < area.DepthLimit;
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

    // When serving, position in Z must be between correct boundaries
    public bool PositionInHorizontalArea(float posZ, Horizontal horiz)
    {
        if (horiz == Horizontal.Left)
        {
            return posZ > _backLeftServingSquare.HorizontalStart && posZ < _backLeftServingSquare.HorizontalLimit;
        }
        else
        {
            return posZ > _backRightServingSquare.HorizontalStart && posZ < _backRightServingSquare.HorizontalLimit;
        }
    }
}