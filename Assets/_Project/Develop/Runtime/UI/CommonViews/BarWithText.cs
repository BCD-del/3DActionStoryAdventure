using Assets._Project.Develop.Runtime.UI.Core;
using TMPro;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.UI.CommonViews
{
    public class BarWithText : MonoBehaviour, IView
    {
        [SerializeField] private TMP_Text _text;
        [SerializeField] private Bar _bar;

        public void UpdateText(string text)
        {
            _text.text = text;
        }

        public void UpdateSlider(float sliderValue)
        {
            _bar.UpdateValue(sliderValue);
        }

        public void SetFillerColor(Color color)
        {
            _bar.SetFillerColor(color);
        }
    }
}
