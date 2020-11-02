using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [HideInInspector] public PointManager pointManager;
    [HideInInspector] public SoundManager soundManager;

    private void Awake()
    {
        pointManager = GetComponent<PointManager>();
        soundManager = GetComponent<SoundManager>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
