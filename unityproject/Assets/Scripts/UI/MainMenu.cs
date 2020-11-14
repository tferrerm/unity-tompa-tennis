using System;
using UnityEngine;
using UnityEngine.Audio;

namespace UI
{
    public class MainMenu : MonoBehaviour
    {
        public AudioMixer audioMixer;
        
        public GameObject title;
        public GameObject mainMenu;
        public GameObject playMenu;
        public GameObject optionsMenu;

        private void Start()
        {
            var volume = PlayerPrefs.GetFloat("Volume", 1);
            OptionsMenu.SetAudioMixerVolume(audioMixer, volume);
        }

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
