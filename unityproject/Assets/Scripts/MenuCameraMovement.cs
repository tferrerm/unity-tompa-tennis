using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuCameraMovement : MonoBehaviour
{
    public Transform startPos;
    public Transform endPos;
    private float _t = 0f;
    [Range(0,0.001f)] public float deltaT = 0.00025f;
    private bool _movingRight = true;

    // Start is called before the first frame update
    void Start()
    {
        transform.position = startPos.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (_movingRight)
        {
            if (_t + deltaT > 1)
            {
                _t = 1;
                _movingRight = false;
            }
            else
            {
                _t += deltaT;
            }
        }
        else
        {
            if (_t - deltaT < 0)
            {
                _t = 0;
                _movingRight = true;
            } else
            {
                _t -= deltaT;
            }
        }
        transform.position = Vector3.Lerp(startPos.position, endPos.position, _t);
    }
}
