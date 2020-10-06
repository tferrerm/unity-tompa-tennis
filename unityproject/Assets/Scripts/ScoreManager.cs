using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    
    private List<Vector2Int> sets = new List<Vector2Int>();
    private int currentSetIndex = 0;
    private Vector2Int currentGame = new Vector2Int(0, 0);

    public int player1Id;
    public int player2Id;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < 3; i++)
        {
            sets.Add(new Vector2Int(0, 0));

        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void WinPoint(int playerId)
    {
        
        if (player1Id == playerId)
        {
            if (currentGame.x < 40)
            {
                currentGame = new Vector2Int(IncreaseGameScore(currentGame.x), currentGame.y);
            }
            else if (currentGame.x == 40)
            {
                if (currentGame.y < 40)
                {
                    WinGame(playerId);
                }
                // 40 40 to AD 40
                else if (currentGame.y == 40)
                {
                    currentGame = new Vector2Int(45, 40);
                }
                // 40 AD to 40 40
                else if (currentGame.y == 45)
                {
                    currentGame = new Vector2Int(40, 40);
                }
            }
            // AD 40 to game
            else if(currentGame.x == 45)
            {
                WinGame(playerId);
            }
        }
        else if(player2Id == playerId)
        {
            if (currentGame.y < 40)
            {
                currentGame = new Vector2Int(currentGame.x, IncreaseGameScore(currentGame.y));
            }
            else if (currentGame.y == 40)
            {
                if (currentGame.x < 40)
                {
                    WinGame(playerId);
                }
                // 40 40 to 40 AD
                else if (currentGame.x == 40)
                {
                    currentGame = new Vector2Int(40, 45);
                } 
                // AD 40 to 40 40
                else if (currentGame.x == 45)
                {
                    currentGame = new Vector2Int(40, 40);
                }
            }
            // 40 AD to game
            else if(currentGame.y == 45)
            {
                WinGame(playerId);
            }
        }
        
        Vector2Int currentSet = sets[currentSetIndex];
        
    }

    private void WinGame(int playerId)
    {
        //TODO
    }

    private int IncreaseGameScore(int currentScore)
    {
        switch (currentScore)
        {
            case 0: 
                return 15;
            case 15: 
                return 30;
            case 30:
                return 40;
            default:
                return 0;
        }
    }

    private int GetCurrentGameScoreForPlayer(int playerId)
    {
        if (playerId == player1Id)
        {
            return currentGame.x;
        }

        return currentGame.y;
    }
}
