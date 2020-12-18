using System.Collections;
using System.Collections.Generic;
using UI;
using UnityEngine;

public class ServeSpeedManager : MonoBehaviour
{
    public Healthbar healthbar;
    private Player _player;
    private GameObject _servePowerBar;
    private float _powerFactor = 1f;
    private readonly float _factorLowerBound = 1f;
    private readonly float _factorUpperBound = 1.5f;
    private bool _factorGrowing = true;
    private bool _oscillating;

    // Start is called before the first frame update
    void Start()
    {
        _player = GetComponent<Player>();
        _servePowerBar = GameObject.FindWithTag("ServePowerBar");
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
                _factorGrowing = false;

            }
            else
            {
                _powerFactor += (float)(Time.deltaTime * 1.5);
            }
            // print(_powerFactor);
            healthbar.SetHealth(GetPercentage(_powerFactor));
        }
        else
        {
            if (_powerFactor - (float)(Time.deltaTime * 1.5) < _factorLowerBound)
            {
                _powerFactor = _factorLowerBound;
                _factorGrowing = true;
            }
            else
            {
                _powerFactor -= (float)(Time.deltaTime * 1.5);
            }
            // print(_powerFactor);
            healthbar.SetHealth(GetPercentage(_powerFactor));
        }
    }

    public float StopPowerOscillation()
    { 
        _oscillating = false;
        var retValue = _powerFactor ;
        _servePowerBar.SetActive(false);
        _powerFactor = 1f;
        return retValue;
    }
    
    public void StartPowerOscillation()
    {
        _oscillating = true;
        _player.StopMovementAnimation();
        _servePowerBar.SetActive(true);
    }
    
    private float GetPercentage(float powerFactor)
    {
        return (powerFactor - _factorLowerBound) * 100.0f / (_factorUpperBound  - _factorLowerBound);
    }
}