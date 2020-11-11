﻿using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class ScoreManager : MonoBehaviour
{
    public enum ServingSide
    {
        Even = 0,
        Odd = 1,
    }

    public UIManager uiManager;
    
    private List<Vector2Int> sets = new List<Vector2Int>();
    private int _currentSetIndex = 0;
    private Vector2Int _currentGame = new Vector2Int(0, 0);
    private bool _gameWon = false;

    public Player player1;
    public AIPlayer player2;
    [HideInInspector] public int player1Id;
    [HideInInspector] public int player2Id;
    public int setsNeededToWin = 3;

    private int _player1SetsWon = 0;
    private int _player2SetsWon = 0;

    private bool _matchFinished;
    [HideInInspector] public bool currentlyInTiebreak;
    [HideInInspector] public ServingSide currentServingSide = ServingSide.Even;
    private int _servingPlayerId;

    private SoundManager _soundManager;

    private void Awake()
    {
        player1Id = player1.playerId;
        player2Id = player2.playerId;
        
        _servingPlayerId = player1Id;
        
        var totalSets = setsNeededToWin == 3 ? 5 : 3;
        for (int i = 0; i < totalSets; i++)
        {
            sets.Add(new Vector2Int(0, 0));
        }

        _soundManager = GetComponent<SoundManager>();
    }

    // Start is called before the first frame update
    void Start()
    {
        //StartCoroutine(ExampleCoroutine());
    }

    IEnumerator ExampleCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);
            WinPoint(Random.Range(0,2) == 0? player1Id : player2Id);
        }
    }

    public void WinPoint(int playerId)
    {
        if (currentlyInTiebreak)
        {
            WinTiebreakPoint(playerId);
            uiManager.SetPlayerGameScore(sets, _currentGame, _currentSetIndex + 1, _servingPlayerId == player1Id);
            return;
        }
        if (player1Id == playerId)
        {
            if (_currentGame[0] < 40)
            {
                _currentGame = new Vector2Int(IncreaseGameScore(_currentGame[0]), _currentGame[1]);
            }
            else switch (_currentGame[0])
            {
                case 40 when _currentGame[1] < 40:
                    _gameWon = true;
                    player1.Cheer();
                    break;
                case 40 when _currentGame[1] == 40:
                    // 40 40 to AD 40
                    _currentGame = new Vector2Int(45, 40);
                    break;
                case 40 when _currentGame[1] == 45:
                    // 40 ad to 40 40
                    _currentGame = new Vector2Int(40, 40);
                    break;
                case 45:
                    // AD 40 to game
                    _gameWon = true;
                    player1.Cheer();
                    break;
            }
        }
        else if(player2Id == playerId)
        {
            if (_currentGame[1] < 40)
            {
                _currentGame = new Vector2Int(_currentGame[0], IncreaseGameScore(_currentGame[1]));
            }
            else switch (_currentGame[1])
            {
                case 40 when _currentGame[0] < 40:
                    _gameWon = true;
                    player2.Cheer();
                    break;
                case 40 when _currentGame[0] == 40:
                    // 40 40 to 40 AD
                    _currentGame = new Vector2Int(40, 45);
                    break;
                case 40 when _currentGame[0] == 45:
                    // AD 40 to 40 40
                    _currentGame = new Vector2Int(40, 40);
                    break;
                case 45:
                    _gameWon = true;
                    player2.Cheer();
                    break;
            }
        }

        if (_gameWon)
            WinGame(playerId);
        else
            SwapServingSide();
        
        _soundManager.PlayCrowdSounds();
        uiManager.SetPlayerGameScore(sets, _currentGame, _currentSetIndex + 1, _servingPlayerId == player1Id);
    }

    private void WinTiebreakPoint(int playerId)
    {
        if (player1Id == playerId)
        {
            _currentGame = new Vector2Int(_currentGame[0] + 1, _currentGame[1]);
        }
        else if (player2Id == playerId)
        {
            _currentGame = new Vector2Int(_currentGame[0], _currentGame[1] + 1);
        }
        if (_currentGame[0] >= 7 && _currentGame[1] < _currentGame[0] - 1)
        {
            sets[_currentSetIndex] = new Vector2Int(sets[_currentSetIndex][0] + 1, sets[_currentSetIndex][1]);
            currentlyInTiebreak = false;
            _currentGame = new Vector2Int(0, 0);
            uiManager.ShowEventMessage(UIManager.MessageType.Game);
        }
        if (_currentGame[1] >= 7 && _currentGame[0] < _currentGame[1] - 1)
        {
            sets[_currentSetIndex] = new Vector2Int(sets[_currentSetIndex][0], sets[_currentSetIndex][1] + 1);
            currentlyInTiebreak = false;
            _currentGame = new Vector2Int(0, 0);
            uiManager.ShowEventMessage(UIManager.MessageType.Game);
        }
        CheckSetWon(sets[_currentSetIndex]);
        SwapServingSide();
        if ((_currentGame[1] + _currentGame[0]) % 2 == 1)
        {
            SwitchServingPlayer();
        }
    }

    private void WinGame(int playerId)
    {
        uiManager.ShowEventMessage(UIManager.MessageType.Game);
        
        var currentSet = sets[_currentSetIndex];
        if (player1Id == playerId)
        {
            sets[_currentSetIndex] = new Vector2Int(sets[_currentSetIndex][0] + 1, sets[_currentSetIndex][1]);
        }
        else if (player2Id == playerId)
        {
            sets[_currentSetIndex] = new Vector2Int(sets[_currentSetIndex][0], sets[_currentSetIndex][1] + 1);
        }
        CheckSetWon(sets[_currentSetIndex]);
        if (sets[_currentSetIndex][0] == 6 && sets[_currentSetIndex][1] == 6)
        {
            currentlyInTiebreak = true;
        }
        _currentGame = new Vector2Int(0, 0);
        currentServingSide = ServingSide.Even;
        _gameWon = false;
        SwitchServingPlayer();
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
        if (set[0] == 6 && set[1] < 5 || set[0] == 7)
        {
            _player1SetsWon += 1;
            _currentSetIndex += 1;
            currentServingSide = ServingSide.Even;
            _gameWon = false;
            uiManager.ShowEventMessage(UIManager.MessageType.Set);
        }
        if (set[1] == 6 && set[0] < 5 || set[1] == 7)
        {
            _player2SetsWon += 1;
            _currentSetIndex += 1;
            currentServingSide = ServingSide.Even;
            _gameWon = false;
            uiManager.ShowEventMessage(UIManager.MessageType.Set);
        }
        if (_player1SetsWon == setsNeededToWin || _player2SetsWon == setsNeededToWin)
        {
            _matchFinished = true;
            currentServingSide = ServingSide.Even;
            _gameWon = false;
            uiManager.ShowEventMessage(_player1SetsWon == setsNeededToWin ? UIManager.MessageType.Victory : UIManager.MessageType.Defeat);
        }
    }

    public ServingSide GetServingSide()
    {
        if (currentlyInTiebreak)
        {
            return (_currentGame[0] + _currentGame[1]) % 2 == 0 ? ServingSide.Even : ServingSide.Odd;
        }
        switch (_currentGame[0]) 
            {
                case 0:
                case 30:
                    if (_currentGame[1] == 0 || _currentGame[1] == 30)
                    {
                        return ServingSide.Even;
                    }
                    return ServingSide.Odd;
                case 15:
                case 40:
                    if (_currentGame[1] == 15 || _currentGame[1] == 40)
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

    private void SwitchServingPlayer()
    {
        if (player1Id == _servingPlayerId)
        {
            _servingPlayerId = player2Id;
        }
        else
        {
            _servingPlayerId = player1Id;
        }  
    }

    private void SwapServingSide()
    {
        currentServingSide = (currentServingSide == ServingSide.Even)
            ? ServingSide.Odd : ServingSide.Even;
    }
}