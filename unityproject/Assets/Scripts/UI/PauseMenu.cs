using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI
{
    public class PauseMenu : MonoBehaviour
    {
        public GameObject pauseMenu;
        [HideInInspector] public bool canOpenPauseMenu = true;
    
        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) && canOpenPauseMenu)
            {
                Time.timeScale = pauseMenu.activeSelf ? 1 : 0;
                pauseMenu.SetActive(!pauseMenu.activeSelf);
                AudioListener.pause = pauseMenu.activeSelf;
                // player._movementBlocked = !pauseMenu.activeSelf;
            }

        }

        public void PauseMenuResume()
        {
            pauseMenu.SetActive(false);
            Time.timeScale = 1;
            AudioListener.pause = false;
            // player._movementBlocked = false;
        }
    
        public void GoToMainMenu()
        {
            pauseMenu.SetActive(false);
            Time.timeScale = 1;
            AudioListener.pause = false;
            SceneManager.LoadScene(0);
        }
    }
}
