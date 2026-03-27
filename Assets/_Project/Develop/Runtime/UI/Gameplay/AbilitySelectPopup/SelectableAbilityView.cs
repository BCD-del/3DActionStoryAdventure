using Assets._Project.Develop.Runtime.UI.Core;
using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.CourseGame.Develop.Gameplay.Features.AbilitiesFeature.View
{
    public class SelectableAbilityView : MonoBehaviour, IShowableView
    {
        public event Action Clicked;

        [SerializeField] private CanvasGroup _canvasGroup;

        [SerializeField] private Button _button;

        [SerializeField] private AbilityIcon _icon;
        [SerializeField] private TMP_Text _name;
        [SerializeField] private TMP_Text _description;

        [SerializeField] private TMP_Text _textOnTablet;

        [SerializeField] private Image _selectImage;

        private Sequence _currentAnimation;
        private float _startYOffset = 100;

        public AbilityIcon Icon => _icon;

        private void Awake()
        {
            _canvasGroup.alpha = 0;
        }

        private void OnEnable()
        {
            _button.onClick.AddListener(OnClicked);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(OnClicked);
        }

        public void SetName(string name) => _name.text = name;

        public void SetDescription(string description) => _description.text = description;

        public void SetTabletText(string tabletText) => _textOnTablet.text = tabletText;

        public void Select() => _selectImage.gameObject.SetActive(true);

        public void Unselect() => _selectImage.gameObject.SetActive(false);

        private void OnClicked()
        {
            Clicked?.Invoke();
        }

        public Tween Hide()
        {
            _currentAnimation?.Kill();

            _currentAnimation = DOTween.Sequence();

            return _currentAnimation
                .Append(_canvasGroup.DOFade(0, 0.4f))
                .Join(_canvasGroup.transform.DOLocalMoveY(_startYOffset, 0.4f))
                .SetUpdate(true)
                .Play();
        }

        public Tween Show()
        {
            _currentAnimation?.Kill();

            _currentAnimation = DOTween.Sequence();

            return _currentAnimation
                .Append(_canvasGroup.DOFade(1, 0.4f))
                .Join(_canvasGroup.transform.DOLocalMoveY(0, 0.4f).From(_startYOffset))
                .SetUpdate(true)
                .Play();
        }

        private void OnDestroy()
        {
            _currentAnimation?.Kill();
        }
    }
}