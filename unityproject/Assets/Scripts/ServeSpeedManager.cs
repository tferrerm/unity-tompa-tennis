﻿using System.Collections;
using System.Collections.Generic;
using UI;
using UnityEngine;

public class ServeSpeedManager : MonoBehaviour
{
    public Healthbar healthbar;
    private float _powerFactor = 1f;
    private readonly float _factorLowerBound = 1f;
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
            if (_powerFactor + (float)(Time.deltaTime * 1.5) > _factorUpperBound)
            {
                _powerFactor = _factorUpperBound;
            }
            else
            {
                _powerFactor += (float)(Time.deltaTime * 1.5);
            }
            print(_powerFactor);
            healthbar.SetHealth(GetPercentage(_powerFactor));
            if (_powerFactor >= _factorUpperBound)
            {
                _factorGrowing = false;
            }
        }
        else
        {
            if (_powerFactor - (float)(Time.deltaTime * 1.5) < _factorLowerBound)
            {
                _powerFactor = _factorLowerBound;
            }
            else
            {
                _powerFactor -= (float)(Time.deltaTime * 1.5);
            }

            print(_powerFactor);
            healthbar.SetHealth(GetPercentage(_powerFactor));
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
    
    private float GetPercentage(float powerFactor)
    {
        return (float)((powerFactor - _factorLowerBound) * 100 / (_factorUpperBound  - _factorLowerBound));
    }
}