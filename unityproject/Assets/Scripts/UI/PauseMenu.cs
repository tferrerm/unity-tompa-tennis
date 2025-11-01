using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace UI
{
    public class PauseMenu : MonoBehaviour
    {
        public GameObject pauseMenu;
        public GameObject optionsMenu;
        public GameObject pauseMenuButtons;
        public GameObject resumeButton;
        [HideInInspector] public bool canOpenPauseMenu = true;
        [SerializeField] private bool isPaused = false;

        [Header("Input Actions")]
        public InputActionAsset inputActionsAsset;
        private InputActionMap _pauseActionMap;
        private InputAction _pauseAction;

        private void Awake() {
            if (inputActionsAsset != null) {
                _pauseActionMap = inputActionsAsset.FindActionMap("UI");
                _pauseAction = _pauseActionMap.FindAction("Pause");
            }
        }

        private void OnEnable()
        {
            _pauseAction.Enable();

            _pauseAction.performed += Pause;
        }

        private void OnDisable()
        {
            _pauseAction.Disable();
        }

        private void Pause(InputAction.CallbackContext context)
        {
            if (!canOpenPauseMenu) return;

            isPaused = !isPaused;

            if (isPaused)
            {
                Time.timeScale = 0;
                AudioListener.pause = true;
                pauseMenu.SetActive(true);
                pauseMenuButtons.SetActive(true);
                EventSystem.current.SetSelectedGameObject(resumeButton);
            }
            else if (!isPaused)
            {
                Time.timeScale = 1;
                AudioListener.pause = false;
                pauseMenu.SetActive(false);
                pauseMenuButtons.SetActive(false);
            }
        }

        public void PauseMenuResume()
        {
            Time.timeScale = 1;
            AudioListener.pause = false;
            pauseMenu.SetActive(false);
            pauseMenuButtons.SetActive(false);
            isPaused = false;
        }

        public void OpenOptionsMenu()
        {
            pauseMenuButtons.SetActive(false);
            optionsMenu.SetActive(true);
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
