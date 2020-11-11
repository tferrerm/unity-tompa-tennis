using System;
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

    public GameObject pauseMenu;
    [HideInInspector] public bool canOpenPauseMenu = true;
    

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

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && canOpenPauseMenu)
        {
            Time.timeScale = pauseMenu.activeSelf ? 1 : 0;
            pauseMenu.SetActive(!pauseMenu.activeSelf);
            AudioListener.pause = pauseMenu.activeSelf;
            player._movementBlocked = !pauseMenu.activeSelf;
        }

    }
    
    public void PauseMenuResume()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1;
    }
    
    public void GoToMainMenu()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1;
        AudioListener.pause = false;
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
