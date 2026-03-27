using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.AbilitiesDroppingFeature;
using Assets._Project.Develop.Runtime.UI.Core;
using Assets._Project.Develop.Runtime.UI.Gameplay;
using Assets._Project.Develop.Runtime.Utilites.CoroutinesManagment;
using System.Collections.Generic;

namespace Assets.CourseGame.Develop.Gameplay.Features.AbilitiesFeature.View
{
    public class AbilitySelectPopupPresenter : PopupPresenterBase
    {
        private const int AbilitiesCount = 3;

        private const string Title = "LEVEL {0} IN THIS ADVENTURE";
        private const string SelectAbilityText = "Select ability";

        private readonly AbilitySelectPopupView _view;

        private readonly Entity _entity;
        private readonly AbilityDropService _abilityDropper;
        private readonly GameplayPresentersFactory _presentersFactory;
        private readonly ViewsFactory _viewsFactory;

        private List<SelectableAbilityPresenter> _presenters = new();
        private SelectableAbilityPresenter _selectedPresenter;

        private int _level;

        public AbilitySelectPopupPresenter(
            ICoroutinesPerformer coroutinesPerformer,
            AbilitySelectPopupView view,
            Entity entity,
            GameplayPresentersFactory presentersFactory,
            AbilityDropService abilityDropper,
            ViewsFactory viewsFactory,
            int level) : base(coroutinesPerformer)
        {
            _view = view;
            _entity = entity;
            _presentersFactory = presentersFactory;
            _abilityDropper = abilityDropper;
            _viewsFactory = viewsFactory;
            _level = level;
        }

        protected override PopupViewBase PopupView => _view;

        public override void Initialize()
        {
            base.Initialize();

            _view.SetTitle(string.Format(Title, _level));
            _view.SetAdditionalText(SelectAbilityText);
            _view.SelectButtonOff();

            _view.SelectButtonClicked += OnSelectButtonClicked;

            List<AbilityDropOption> dropOptions = _abilityDropper.Drop(AbilitiesCount, _entity);

            for (int i = 0; i < dropOptions.Count; i++)
            {
                SelectableAbilityView selectableAbilityView = _viewsFactory.Create<SelectableAbilityView>(ViewIDs.AbilitySelectView);

                _view.AbilityListView.Add(selectableAbilityView);

                SelectableAbilityPresenter presenter = _presentersFactory
                    .CreateSelectableAbilityPresenter(dropOptions[i].Config, selectableAbilityView, _entity, dropOptions[i].Level);

                presenter.Selected += OnPresenterSelected;
                presenter.Initialize();

                _presenters.Add(presenter);
            }
        }

        protected override void OnPreHide()
        {
            base.OnPreHide();

            _view.SelectButtonOff();

            _view.SelectButtonClicked -= OnSelectButtonClicked;

            foreach (SelectableAbilityPresenter abilityPresenter in _presenters)
            {
                abilityPresenter.Selected -= OnPresenterSelected;
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            _view.SelectButtonClicked -= OnSelectButtonClicked;

            foreach (SelectableAbilityPresenter abilityPresenter in _presenters)
            {
                abilityPresenter.Selected -= OnPresenterSelected;
                _view.AbilityListView.Remove(abilityPresenter.View);
                _viewsFactory.Release(abilityPresenter.View);
                abilityPresenter.Dispose();
            }

            _presenters.Clear();
        }

        private void OnSelectButtonClicked()
        {
            _selectedPresenter.Provide();
            OnCloseRequest();
        }

        private void OnPresenterSelected(SelectableAbilityPresenter selected)
        {
            _view.SelectButtonOn();
            _view.AbilityListView.Select(selected.View);
            _selectedPresenter = selected;
        }
    }
}