using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UIManager : MonoBehaviour
    {
        public Transform scoreContainer;
        public Transform pastSetPrefab;
        public Transform currentSetContainer;
        public Transform currentGameContainer;
        public Transform player1Name;
        public Transform player2Name;
        private TMP_Text _player1NameText;
        private TMP_Text _player2NameText;
        private TMP_Text _player1CurrentSetScore;
        private TMP_Text _player2CurrentSetScore;
        private TMP_Text _player1CurrentGameScore;
        private TMP_Text _player2CurrentGameScore;
        private int _setCount = 1;
        private float _scoreSeparator = 2.5f;
    
        public Image player1ServingBall;
        public Image player2ServingBall;

        public bl_ScrollText eventMessenger;
        public Text eventText;
        private string _player1ScoreMessage;
        private string _player2ScoreMessage;
        public string gameMessage;
        public string setMessage;
        public string defeatMessage;
        public string victoryMessage;

        private ScoreManager _scoreManager;
    
        // Start is called before the first frame update
        private void Start()
        {
            _player1NameText = player1Name.GetComponent<TMP_Text>();
            _player2NameText = player2Name.GetComponent<TMP_Text>();
        
            _player1CurrentSetScore = currentSetContainer.Find("Player 1/Player 1 Score").GetComponent<TMP_Text>();
            _player2CurrentSetScore = currentSetContainer.Find("Player 2/Player 2 Score").GetComponent<TMP_Text>();
            _player1CurrentGameScore = currentGameContainer.Find("Player 1/Player 1 Score").GetComponent<TMP_Text>();
            _player2CurrentGameScore = currentGameContainer.Find("Player 2/Player 2 Score").GetComponent<TMP_Text>();
        
            _scoreManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<ScoreManager>();

            var gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
            _player1ScoreMessage = gameManager.player.playerName.Split(' ')[1] + " SCORES";
            _player2ScoreMessage = gameManager.aiPlayer.playerName.Split(' ')[1] + " SCORES";
        }

        public void SetPlayerNames(string player1, string player2)
        {
            _player1NameText.text = player1;
            _player2NameText.text = player2;
        }

        public void SetPlayerGameScore(List<Vector2Int> setsScores, Vector2Int gameScores, int setCount, bool player1Serving)
        {
            if (setCount > _setCount)
            {
                _setCount++;
                InstantiatePastSet(setsScores[setCount - 2]);
                ResetCurrentScores();
            }
            else
            {
                UpdateCurrentScores(setsScores[setCount - 1], gameScores);
            }

            player1ServingBall.enabled = player1Serving;
            player2ServingBall.enabled = !player1Serving;
        }

        private void InstantiatePastSet(Vector2Int pastSetScore)
        {
            var pastSet = Instantiate(pastSetPrefab, currentSetContainer.position, Quaternion.identity, scoreContainer);
            pastSet.Find("Player 1/Player 1 Score").GetComponent<TMP_Text>().text = pastSetScore[0].ToString();
            pastSet.Find("Player 2/Player 2 Score").GetComponent<TMP_Text>().text = pastSetScore[1].ToString();

            // Shift current game and set transform positions
            var currentSetPosition = currentSetContainer.position;
            currentSetContainer.position = new Vector3(currentSetPosition.x + _scoreSeparator + ((RectTransform) pastSet).rect.width,
                currentSetPosition.y, currentSetPosition.z);
            
            var currentGamePosition = currentGameContainer.position;
            currentGameContainer.position = new Vector3(currentGamePosition.x + _scoreSeparator + ((RectTransform) pastSet).rect.width,
                currentGamePosition.y, currentGamePosition.z);
        }
    
        private void ResetCurrentScores()
        {
            _player1CurrentSetScore.text = "0";
            _player2CurrentSetScore.text = "0";
            _player1CurrentGameScore.text = "0";
            _player2CurrentGameScore.text = "0";
        }

        private void UpdateCurrentScores(Vector2Int currentSetScores, Vector2Int currentGameScores)
        {
            _player1CurrentSetScore.text = currentSetScores[0].ToString();
            _player2CurrentSetScore.text = currentSetScores[1].ToString();
            _player1CurrentGameScore.text = GetScoreString(currentGameScores[0]);
            _player2CurrentGameScore.text = GetScoreString(currentGameScores[1]);
        }

        private string GetScoreString(int score)
        {
            return score == 45 && !_scoreManager.currentlyInTiebreak ? "AD" : score.ToString();
        }

        public void ShowEventMessage(MessageType msgType) // TODO ADD QUEUE
        {
            switch (msgType)
            {
                case MessageType.Player1Score:
                    eventText.text = _player1ScoreMessage;
                    break;
                case MessageType.Player2Score:
                    eventText.text = _player2ScoreMessage;
                    break;
                case MessageType.Game:
                    eventText.text = gameMessage;
                    break;
                case MessageType.Set:
                    eventText.text = setMessage;
                    break;
                case MessageType.Defeat:
                    eventText.text = defeatMessage;
                    break;
                case MessageType.Victory:
                    eventText.text = victoryMessage;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            eventMessenger.gameObject.SetActive(true);
            var backgroundColor = eventMessenger.background.color;
            eventMessenger.background.color = new Color(backgroundColor.r, backgroundColor.g, backgroundColor.b, 0.44f);
            eventMessenger.showMessage = true;
        }

        public enum MessageType
        {
            Player1Score = 0,
            Player2Score = 1,
            Game = 2,
            Set = 3,
            Defeat = 4,
            Victory = 5
        }
    }
}
