using TMPro;
using UnityEngine;

namespace UI
{
    public class SetSelector : MonoBehaviour
    {
        public TMP_Text setNumberText;
        private int _sets;
    
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
            if (_sets == 3)
                return;
            _sets++;
            PlayerPrefs.SetInt("SetNumber", _sets);
        }
    
        public void DecrementSets()
        {
            if (_sets == 1)
                return;
            _sets--;
            PlayerPrefs.SetInt("SetNumber", _sets);
        }
    }
}
