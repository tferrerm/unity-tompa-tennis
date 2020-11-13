﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [HideInInspector] public CourtManager courtManager;
    [HideInInspector] public PointManager pointManager;
    [HideInInspector] public SoundManager soundManager;
    public AIPlayer aiPlayer;
    public Player player;

    private void Awake()
    {
        courtManager = GetComponent<CourtManager>();
        pointManager = GetComponent<PointManager>();
        soundManager = GetComponent<SoundManager>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void GameFinished()
    {
        SceneManager.LoadScene(0);
    }

    public void PlayerHitBall(Vector3 startPos, Vector3 ballTargetPos)
    {
        TriggerAIPlayerMovement(startPos, ballTargetPos);
    }
    
    private void TriggerAIPlayerMovement(Vector3 startPos, Vector3 ballTargetPos)
    {
        aiPlayer.UpdateTargetPosition(startPos, ballTargetPos);
    }
}
