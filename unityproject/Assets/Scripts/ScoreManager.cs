using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    
    private List<Vector2Int> sets = new List<Vector2Int>();
    private int _currentSetIndex = 0;
    private Vector2Int _currentGame = new Vector2Int(0, 0);
    
    public int player1Id;
    public int player2Id;
    public int setsNeededToWin = 3;

    private int _player1SetsWon = 0;
    private int _player2SetsWon = 0;

    private bool _matchFinished;
        
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < 3; i++)
        {
            sets.Add(new Vector2Int(0, 0));

        }
    }

    public void WinPoint(int playerId)
    {
        
        if (player1Id == playerId)
        {
            if (_currentGame.x < 40)
            {
                _currentGame = new Vector2Int(IncreaseGameScore(_currentGame.x), _currentGame.y);
            }
            else switch (_currentGame.x)
            {
                case 40 when _currentGame.y < 40:
                    WinGame(playerId);
                    break;
                case 40 when _currentGame.y == 40:
                    // 40 40 to AD 40
                    _currentGame = new Vector2Int(45, 40);
                    break;
                case 40 when _currentGame.y == 45:
                    // 40 ad to 40 40
                    _currentGame = new Vector2Int(40, 40);
                    break;
                case 45:
                    // AD 40 to game
                    WinGame(playerId);
                    break;
            }
        }
        else if(player2Id == playerId)
        {
            if (_currentGame.y < 40)
            {
                _currentGame = new Vector2Int(_currentGame.x, IncreaseGameScore(_currentGame.y));
            }
            else switch (_currentGame.y)
            {
                case 40 when _currentGame.x < 40:
                    WinGame(playerId);
                    break;
                case 40 when _currentGame.x == 40:
                    // 40 40 to 40 AD
                    _currentGame = new Vector2Int(40, 45);
                    break;
                case 40 when _currentGame.x == 45:
                    // AD 40 to 40 40
                    _currentGame = new Vector2Int(40, 40);
                    break;
                case 45:
                    WinGame(playerId);
                    break;
            }
        }
    }

    private void WinGame(int playerId)
    {
        var currentSet = sets[_currentSetIndex];
        if (player1Id == playerId)
        {
            sets[_currentSetIndex] = new Vector2Int(sets[_currentSetIndex].x + 1, sets[_currentSetIndex].y);
        }
        else if (player2Id == playerId)
        {
            sets[_currentSetIndex] = new Vector2Int(sets[_currentSetIndex].x, sets[_currentSetIndex].y + 1);
        }
        CheckSetWon(sets[_currentSetIndex]);
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

    private void CheckSetWon(Vector2Int set)
    {
        if (set.x == 6 && set.y < 5 || set.x == 7)
        {
            _player1SetsWon += 1;
            _currentSetIndex += 1;
        }
        if (set.y == 6 && set.x < 5 || set.y == 7)
        {
            _player2SetsWon += 1;
            _currentSetIndex += 1;
        }
        if (_player1SetsWon == setsNeededToWin || _player2SetsWon == setsNeededToWin)
        {
            _matchFinished = true;
        }
    }
}