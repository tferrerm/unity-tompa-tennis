using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class SetSelector : MonoBehaviour
    {
        public TMP_Text setNumberText;
        private int _sets;
        public Button decrementButton;
        public Button incrementButton;
        private const int MinNumberSets = 1;
        private const int MaxNumberSets = 3;
    
        // Start is called before the first frame update
        void Start()
        {
            _sets = 1;
            PlayerPrefs.SetInt("SetNumber", _sets);
            setNumberText.text = _sets.ToString();
        }

        // Update is called once per frame
        void Update()
        {
            setNumberText.text = _sets.ToString();
        }

        public void IncrementSets()
        {
            if (_sets == MaxNumberSets)
                return;
            _sets += 2;
            if (_sets == MaxNumberSets)
                incrementButton.interactable = false;

            if (!decrementButton.interactable)
            {
                decrementButton.interactable = true;
                EventSystem.current.SetSelectedGameObject(decrementButton.gameObject);
            }
                
            PlayerPrefs.SetInt("SetNumber", _sets);
        }
    
        public void DecrementSets()
        {
            if (_sets == MinNumberSets)
                return;
            _sets -= 2;
            if (_sets == MinNumberSets)
                decrementButton.interactable = false;
            
            if (!incrementButton.interactable)
            {
                incrementButton.interactable = true;
                EventSystem.current.SetSelectedGameObject(incrementButton.gameObject);
            }
            
            PlayerPrefs.SetInt("SetNumber", _sets);
        }
    }
}
