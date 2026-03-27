using System;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

namespace Assets._Project.Develop.Runtime.UI.Core
{
    public abstract class PopupViewBase : MonoBehaviour, IShowableView
    {
        [SerializeField] private CanvasGroup _mainGroup;
        [SerializeField] private CanvasGroup _body;
        [SerializeField] private Image _anticlicker;

        [SerializeField] private PopupAnimationTypes _animationType;

        private float _anticlickerDefaultAlpha;

        private Tween _currentAnimation;

        public event Action CloseRequest;

        private void Awake()
        {
            _anticlickerDefaultAlpha = _anticlicker.color.a;
            _mainGroup.alpha = 0;
            _mainGroup.blocksRaycasts = false;
        }

        public Tween Show()
        {
            KillCurrentAnimation();

            OnPreShow();

            _mainGroup.alpha = 1;
            _mainGroup.blocksRaycasts = true;

            Sequence animation = PopupAnimationsCreator
                .CreateShowAnimation(_body, _anticlicker, _animationType, _anticlickerDefaultAlpha);

            ModifyShowAnimation(animation);

            animation.OnComplete(() => OnPostShow());

            return _currentAnimation = animation.SetUpdate(true).Play();
        }

        public Tween Hide()
        {
            KillCurrentAnimation();

            OnPreHide();

            _mainGroup.alpha = 0;
            _mainGroup.blocksRaycasts = false;

            Sequence animation = PopupAnimationsCreator
                .CreateHideAnimation(_body, _anticlicker, _animationType, _anticlickerDefaultAlpha);

            ModifyHideAnimation(animation);

            animation.OnComplete(() => OnPostHide());

            return _currentAnimation = animation.SetUpdate(true).Play();
        }

        public void OnCloseButtonClicked() 
        {
            print("close button clicked");
            CloseRequest?.Invoke(); 
        }

        protected virtual void ModifyShowAnimation(Sequence animation)
        {

        }

        protected virtual void ModifyHideAnimation(Sequence animation)
        {

        }

        protected virtual void OnPostHide() { }

        protected virtual void OnPreHide() { }

        protected virtual void OnPostShow() { }

        protected virtual void OnPreShow() { }

        private void OnDestroy()
        {
            KillCurrentAnimation();
        }

        private void KillCurrentAnimation()
        {
            if (_currentAnimation != null)
                _currentAnimation.Kill();
        }
    }
}
