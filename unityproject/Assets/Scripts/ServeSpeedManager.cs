using System.Collections;
using System.Collections.Generic;
using UI;
using UnityEngine;

public class ServeSpeedManager : MonoBehaviour
{
    private float _powerFactor;
    private readonly float _factorLowerBound = 0.5f;
    private readonly float _factorUpperBound = 1.5f;
    private bool _factorGrowing = true;
    private bool _oscillating;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!_oscillating) return;
        if (_factorGrowing)
        {
            _powerFactor += Time.deltaTime;
            if (_powerFactor >= _factorUpperBound)
            {
                _factorGrowing = false;
            }
        }
        else
        {
            _powerFactor -= Time.deltaTime;
            if (_powerFactor <= _factorLowerBound)
            {
                _factorGrowing = true;
            }
        }
    }

    public float StopPowerOscillation()
    {
        _oscillating = false;
        return _powerFactor;
    }
    
    public void StartPowerOscillation()
    {
        _oscillating = true;
    }
}