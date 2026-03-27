using Assets._Project.Develop.Runtime.UI.CommonViews;
using Assets._Project.Develop.Runtime.UI.Core;
using DG.Tweening;

namespace Assets.CourseGame.Develop.Gameplay.Features.AbilitiesFeature.View
{
    public class SelectableAbilityListView : ElementsListView<SelectableAbilityView>, IShowableView
    {
        private Sequence _currentAnimation;

        public void Select(SelectableAbilityView selectableAbilityView)
        {
            foreach (SelectableAbilityView view in Elements)
                view.Unselect();

            selectableAbilityView.Select();
        }

        public Tween Hide()
        {
            _currentAnimation?.Kill();
            _currentAnimation = DOTween.Sequence();

            foreach (SelectableAbilityView view in Elements)
            {
                _currentAnimation.Append(view.Hide());
                _currentAnimation.AppendInterval(0.2f);
            }

            return _currentAnimation.SetUpdate(true).Play();
        }

        public Tween Show()
        {
            _currentAnimation?.Kill();
            _currentAnimation = DOTween.Sequence();

            foreach (SelectableAbilityView view in Elements)
            {
                _currentAnimation.Append(view.Show());
                _currentAnimation.AppendInterval(0.2f);
            }

            return _currentAnimation.SetUpdate(true).Play();
        }
    }
}