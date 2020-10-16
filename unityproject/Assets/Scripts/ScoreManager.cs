using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public enum ServingSide
    {
        Even = 0,
        Odd = 1,
    }
    
    private List<Vector2Int> sets = new List<Vector2Int>();
    private int _currentSetIndex = 0;
    private Vector2Int _currentGame = new Vector2Int(0, 0);
    
    public int player1Id;
    public int player2Id;
    public int setsNeededToWin = 3;

    private int _player1SetsWon = 0;
    private int _player2SetsWon = 0;

    private bool _matchFinished;
    private bool _currentlyInTiebreak;
    private ServingSide _currentServingSide = ServingSide.Even;
    private int _servingPlayerId;
        
    // Start is called before the first frame update
    void Start()
    {
        _servingPlayerId = player1Id;
        var totalSets = setsNeededToWin == 3 ? 5 : 3;
        for (int i = 0; i < totalSets; i++)
        {
            sets.Add(new Vector2Int(0, 0));
        }
    }

    public void WinPoint(int playerId)
    {
        if (_currentlyInTiebreak)
        {
            WinTiebreakPoint(playerId);
            return;
        }
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

    private void WinTiebreakPoint(int playerId)
    {
        if (player1Id == playerId)
        {
            _currentGame = new Vector2Int(_currentGame.x + 1, _currentGame.y);
        }
        else if (player2Id == playerId)
        {
            _currentGame = new Vector2Int(_currentGame.x, _currentGame.y + 1);
        }
        if (_currentGame.x >= 7 && _currentGame.y < _currentGame.x - 1)
        {
            sets[_currentSetIndex] = new Vector2Int(sets[_currentSetIndex].x + 1, sets[_currentSetIndex].y);
            _currentlyInTiebreak = false;
            _currentGame = new Vector2Int(0, 0);
        }
        if (_currentGame.y >= 7 && _currentGame.x < _currentGame.y - 1)
        {
            sets[_currentSetIndex] = new Vector2Int(sets[_currentSetIndex].x, sets[_currentSetIndex].y + 1);
            _currentlyInTiebreak = false;
            _currentGame = new Vector2Int(0, 0);
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
        if (sets[_currentSetIndex].x == 6 && sets[_currentSetIndex].y == 6)
        {
            _currentlyInTiebreak = true;
        }
        _currentGame = new Vector2Int(0, 0);
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

    public ServingSide GetServingSide()
    {
        if (_currentlyInTiebreak)
        {
            return (_currentGame.x + _currentGame.y) % 2 == 0 ? ServingSide.Even : ServingSide.Odd;
        }
        switch (_currentGame.x) 
            {
                case 0:
                case 30:
                    if (_currentGame.y == 0 || _currentGame.y == 30)
                    {
                        return ServingSide.Even;
                    }
                    return ServingSide.Odd;
                case 15:
                case 40:
                    if (_currentGame.y == 15 || _currentGame.y == 40)
                    {
                        return ServingSide.Even;
                    }
                    return ServingSide.Odd;
                case 45:
                    return ServingSide.Odd;
            }
        return ServingSide.Even;
    }

    public int GetServingPlayerId()
    {
        return _servingPlayerId;
    }

    public int GetReceivingPlayerId()
    {
        return player1Id == _servingPlayerId ? player2Id : player1Id;
    }
}