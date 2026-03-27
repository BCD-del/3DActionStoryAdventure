using Assets._Project.Develop.Runtime.Configs.Gameplay.Abilities;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.AbilitiesFeature;
using Assets._Project.Develop.Runtime.UI.Core;
using System;
using System.Linq;

namespace Assets.CourseGame.Develop.Gameplay.Features.AbilitiesFeature.View
{
    public class SelectableAbilityPresenter : IPresenter
    {
        public event Action<SelectableAbilityPresenter> Selected;

        private AbilityFactory _abilityFactory;
        private Entity _entity;

        private int _level;

        public SelectableAbilityPresenter(
           AbilityConfig abilityConfig,
           SelectableAbilityView view,
           AbilityFactory abilityFactory,
           Entity entity,
           int level)
        {
            AbilityConfig = abilityConfig;
            View = view;
            _abilityFactory = abilityFactory;
            _entity = entity;
            _level = level;
        }

        public AbilityConfig AbilityConfig { get; }
        public SelectableAbilityView View { get; }

        public void Initialize()
        {
            View.SetName(AbilityConfig.Name);
            View.SetDescription(AbilityConfig.Description);
            View.Icon.SetIcon(AbilityConfig.Icon);

            InitByAbilityConfig();

            View.Clicked += OnViewClicked;
        }

        public void Dispose()
        {
            View.Clicked -= OnViewClicked;
        }

        public void Provide()
        {
            Ability ability;

            if (AbilityConfig.IsUpgradable())
            {
                ability = _entity.Abilities.Elements.FirstOrDefault(abil => abil.ID == AbilityConfig.ID);

                if (ability != null)
                {
                    ability.AddLevel(_level);
                    return;
                }
            }

            ability = _abilityFactory.CreateAbilityFor(_entity, AbilityConfig, _level);
            _entity.Abilities.Add(ability);
        }

        private void OnViewClicked() => Selected?.Invoke(this);

        private void InitByAbilityConfig()
        {
            if (AbilityConfig.IsUpgradable())
            {
                Ability ability = _entity.Abilities.Elements.FirstOrDefault(abil => abil.ID == AbilityConfig.ID);

                if (ability != null)
                {
                    View.Icon.ShowLevel();
                    View.Icon.SetLevel("LvL" + ability.CurrentLevel.Value);
                    View.SetTabletText("LvL" + ability.CurrentLevel.Value + "->" + "LvL" + (ability.CurrentLevel.Value + _level));
                }
                else
                {
                    View.Icon.HideLevel();
                    View.SetTabletText("NEW LvL" + _level);
                }
            }
            else
            {
                View.Icon.HideLevel();
                View.SetTabletText("NEW");
            }
        }
    }
}