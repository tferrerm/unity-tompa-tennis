using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UIElements;

public class OptionsMenu : MonoBehaviour
{
    public AudioMixer audioMixer;

    public GameObject optionsMenu;
    public GameObject title;
    public GameObject mainMenu;
    
    // Start is called before the first frame update
    void Start()
    {
        PlayerPrefs.SetFloat("Volume", 1);
    }

    public void ReturnToMainMenu()
    {
        optionsMenu.SetActive(false);
        title.SetActive(true);
        mainMenu.SetActive(true);
    }

    public void SetVolume(float volume)
    {
        audioMixer.SetFloat("Volume", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("Volume", volume);
    }
}
