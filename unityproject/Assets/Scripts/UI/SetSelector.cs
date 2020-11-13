using TMPro;
using UnityEngine;
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
            _sets = PlayerPrefs.GetInt("SetNumber", 1);
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
                decrementButton.interactable = true;
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
                incrementButton.interactable = true;
            PlayerPrefs.SetInt("SetNumber", _sets);
        }
    }
}
