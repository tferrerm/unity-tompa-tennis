using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TennisVariables
{
    public const float WalkSpeed = 5f;
    public const float RunSpeed = 10f;
    public const float SprintSpeed = 15f;
    public const float BackSpeed = 7f;

    public const float ServiceTossSpeed = 20f;
    public const float ServiceSpeed = 65f;
    public const float ServiceYAttenuation = 350f;
    public const float DeepHitSpeed = 45f;
    public const float DeepHitYAttenuation = 400f;
    public const float FrontHitSpeed = 35f;
    public const float FrontHitYAttenuation = 200f;

    public const float BallHitTargetRadius = 2.5f;
    public const float BallServeTargetRadius = 2f;
    
    public const float DeepBallBounceFrictionMultiplier = 0.75f;
    public const float DropshotBallBounceFrictionMultiplier = 0.3f;
    public const float BallBounciness = 0.8f;
    public const float NetBounceFrictionMultiplier = 0.25f;
    public const float DefaultBounceFrictionMultiplier = 0.3f;
    
    public const float FastHitAnimationThresholdSpeed = 25f;
    
    public const float BallColliderFrontDelta = 2f;
}
