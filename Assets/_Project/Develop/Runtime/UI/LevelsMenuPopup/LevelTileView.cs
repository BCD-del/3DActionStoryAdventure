using Assets._Project.Develop.Runtime.UI.Core;
using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Project.Develop.Runtime.UI.LevelsMenuPopup
{
    public class LevelTileView : MonoBehaviour, IShowableView
    {
        public event Action Clicked;

        [SerializeField] private Image _background;
        [SerializeField] private TMP_Text _levelNameText;
        [SerializeField] private Button _button;

        private void OnEnable()
        {
            _button.onClick.AddListener(OnButtonClicked);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(OnButtonClicked);
        }

        public void Init(string levelName, Sprite levelIcon)
        {
            // _levelNameText.text = levelName;
            _background.sprite = levelIcon;
        }

        private void OnDestroy()
        {
            transform.DOKill();
        }

        public Tween Hide()
        {
            transform.DOKill();

            return DOTween.Sequence();
        }

        public Tween Show()
        {
            transform.DOKill();

            return transform
                .DOScale(1, 0.1f)
                .From(0)
                .SetUpdate(true)
                .Play();
        }

        private void OnButtonClicked() => Clicked?.Invoke();

        public void SetComplete()
        {
            _button.interactable = false;
            _background.color = Color.green;
        }

        public void SetActive()
        {
            _button.interactable = true;
            _background.color = Color.white;
        }

        public void SetBlock()
        {
            _button.interactable = false;
            _background.color = Color.red;
        }
    }
}
