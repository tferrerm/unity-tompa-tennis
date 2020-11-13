using UnityEngine;

namespace UI
{
    public class MainMenu : MonoBehaviour
    {
        public GameObject title;
        public GameObject mainMenu;
        public GameObject playMenu;
        public GameObject optionsMenu;
    
        public void OpenPlayMenu()
        {
            title.SetActive(false);
            mainMenu.SetActive(false);
            playMenu.SetActive(true);   
        }

        public void OpenOptionsMenu()
        {
            title.SetActive(false);
            mainMenu.SetActive(false);
            optionsMenu.SetActive(true);
        }

        public void QuitGame()
        {
            Application.Quit();
        }
    }
}
