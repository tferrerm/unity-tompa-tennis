using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TechniqueAttributes
{
    private float directionEpsilon;
    private float forceMultiplier;

    public TechniqueAttributes(float epsilon, float forceMult)
    {
        directionEpsilon = epsilon;
        forceMultiplier = forceMult;
    }

    public float DirectionEpsilon => directionEpsilon;

    public float ForceMultiplier => forceMultiplier;
}
