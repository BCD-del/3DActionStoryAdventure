using Assets._Project.Develop.Runtime.UI.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Project.Develop.Runtime.UI.CommonViews
{
    public class Bar : MonoBehaviour, IView
    {
        [SerializeField] private Image _filler;
        [SerializeField] private Slider _slider;

        public void UpdateValue(float sliderValue)
        {
            _slider.value = sliderValue;
        }

        public void SetFillerColor(Color color)
        {
            _filler.color = color;
        }
    }
}
