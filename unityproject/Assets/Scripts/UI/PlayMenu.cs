﻿using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace UI
{
    public class PlayMenu : MonoBehaviour
    {
        public GameObject title;
        public GameObject mainMenu;
        public GameObject playMenu;

        public TMP_InputField playerNameInput;
        public TMP_Dropdown difficultyDropdown;
    
        // Start is called before the first frame update
        void Start()
        {
            playerNameInput.text = PlayerPrefs.GetString("PlayerName", playerNameInput.text);
            bool controllerPresent = InputSystem.devices.Any(device =>
            {
                var deviceClass = device.description.deviceClass;
                return !deviceClass.Equals("Keyboard") && !deviceClass.Equals("Mouse");
            });
            if (controllerPresent)
            {
                playerNameInput.DeactivateInputField();
            }
        }

        private void OnEnable()
        {
            EventSystem.current.SetSelectedGameObject(playerNameInput.gameObject);
        }

        public void ReturnToMainMenu()
        {
            playMenu.SetActive(false);
            title.SetActive(true);
            mainMenu.SetActive(true);
        }

        public void DifficultySelection()
        {
            PlayerPrefs.SetInt("Difficulty", difficultyDropdown.value);
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
