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

        // Update is called once per frame
        void Update()
        {
            PlayerPrefs.SetString("PlayerName", playerNameInput.text);
        }
    
        public void ReturnToMainMenu()
        {
            playMenu.SetActive(false);
            title.SetActive(true);
            mainMenu.SetActive(true);
        }
    
        public void PlayGame()
        {
            PlayerPrefs.Save();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }
}
