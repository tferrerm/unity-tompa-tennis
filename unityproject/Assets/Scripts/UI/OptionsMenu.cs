using UnityEngine;
using UnityEngine.Audio;

namespace UI
{
    public class OptionsMenu : MonoBehaviour
    {
        public AudioMixer audioMixer;
        public UnityEngine.UI.Slider slider;

        public GameObject optionsMenu;
        public GameObject title;
        public GameObject mainMenu;

        public GameObject pauseMenuButtons;
    
        // Start is called before the first frame update
        void Start()
        {
            var volume = PlayerPrefs.GetFloat("Volume", 1);
            slider.value = volume;
        }

        public void ReturnToMainMenu()
        {
            PlayerPrefs.Save();
            optionsMenu.SetActive(false);
            title.SetActive(true);
            mainMenu.SetActive(true);
        }

        public void ReturnToPauseMenu()
        {
            PlayerPrefs.Save();
            optionsMenu.SetActive(false);
            pauseMenuButtons.SetActive(true);
        }

        public void SetVolume(float volume)
        {
            SetAudioMixerVolume(audioMixer, volume);
            PlayerPrefs.SetFloat("Volume", volume);
        }

        public static void SetAudioMixerVolume(AudioMixer audioMixer, float volume)
        {
            volume = Mathf.Log10(volume) * 20;
            audioMixer.SetFloat("Volume", volume);
        }
    }
}
