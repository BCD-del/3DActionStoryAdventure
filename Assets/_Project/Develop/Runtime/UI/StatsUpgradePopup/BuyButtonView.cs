using Assets._Project.Develop.Runtime.UI.Core;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Project.Develop.Runtime.UI.StatsUpgradePopup
{
    public class BuyButtonView : MonoBehaviour, IView
    {
        public event Action Click;

        [SerializeField] private Button _button;

        [SerializeField] private Image _background;

        [SerializeField] private Sprite _availableSprite;
        [SerializeField] private Sprite _lockedSprite;

        [Space, SerializeField] private TMP_Text _priceText;
        [SerializeField] private Image _priceIcon;

        private void OnEnable()
        {
            _button.onClick.AddListener(OnClick);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(OnClick);
        }

        protected virtual void OnClick()
        {
            Click?.Invoke();
        }

        public virtual void Lock() => _background.sprite = _lockedSprite;

        public virtual void Unlock() => _background.sprite = _availableSprite;

        public void SetPriceText(string priceText) => _priceText.text = priceText;
    }
}