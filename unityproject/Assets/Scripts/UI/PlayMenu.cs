using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI
{
    public class PlayMenu : MonoBehaviour
    {
        public GameObject title;
        public GameObject mainMenu;
        public GameObject playMenu;

        public TMP_InputField playerNameInput;
    
        // Start is called before the first frame update
        void Start()
        {
            playerNameInput.text = PlayerPrefs.GetString("PlayerName", playerNameInput.text);
        }
    
        public void ReturnToMainMenu()
        {
            playMenu.SetActive(false);
            title.SetActive(true);
            mainMenu.SetActive(true);
        }
    
        public void PlayGame()
        {
            var playerName = playerNameInput.text;
            PlayerPrefs.SetString("PlayerName", playerName);
            if (string.IsNullOrWhiteSpace(playerName))
            {
                PlayerPrefs.SetString("PlayerName", "Tompa Player");
            }

            PlayerPrefs.SetString("AIPlayerName", "AI Player");
            
            PlayerPrefs.Save();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }
}
