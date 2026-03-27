using Assets._Project.Develop.Infrastructure.DI;
using Assets._Project.Develop.Runtime.Configs.Gameplay;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Abilities;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.AbilitiesDroppingFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.AbilitiesFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.LevelUPFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.MainHero;
using Assets._Project.Develop.Runtime.Gameplay.Features.StageFeature;
using Assets._Project.Develop.Runtime.UI.CommonViews;
using Assets._Project.Develop.Runtime.UI.Core;
using Assets._Project.Develop.Runtime.UI.Gameplay.HealthDisplay;
using Assets._Project.Develop.Runtime.UI.Gameplay.ResultPopups;
using Assets._Project.Develop.Runtime.UI.Gameplay.Stages;
using Assets._Project.Develop.Runtime.Utilites.ConfigsManagment;
using Assets._Project.Develop.Runtime.Utilites.CoroutinesManagment;
using Assets._Project.Develop.Runtime.Utilites.SceneManagement;
using Assets.CourseGame.Develop.Gameplay.Features.AbilitiesFeature.View;
using System;

namespace Assets._Project.Develop.Runtime.UI.Gameplay
{
    public class GameplayPresentersFactory
    {
        private readonly DIContainer _container;
        private readonly GameplayInputArgs _inputArgs;

        public GameplayPresentersFactory(DIContainer container, GameplayInputArgs inputArgs)
        {
            _container = container;
            _inputArgs = inputArgs;
        }

        public EntitiesHealthDisplayPresenter CreateEntitiesHealthDisplayPresenter(EntitiesHealthDisplay view)
        {
            return new EntitiesHealthDisplayPresenter(
                _container.Resolve<EntitiesLifeContext>(),
                view,
                _container.Resolve<ViewsFactory>(),
                this
                );
        }

        public EntityHealthPresenter CreateEntityHealthPresenter(Entity entity, BarWithText view)
        {
            return new EntityHealthPresenter(view, entity);
        }

        public WinPopupPresenter CreateWinPopupPresenter(WinPopupView view)
        {
            return new WinPopupPresenter(
                _container.Resolve<ICoroutinesPerformer>(),
                view,
                _container.Resolve<SceneSwitcherService>()
                );
        }

        public DefeatPopupPresenter CreateDefeatPopupPresenter(DefeatPopupView view)
        {
            return new DefeatPopupPresenter(
                _container.Resolve<ICoroutinesPerformer>(),
                view,
                _container.Resolve<SceneSwitcherService>(),
                _inputArgs
                );
        }

        public GameplayScreenPresenter CreateGameplayScreen(GameplayScreenView view)
        {
            return new GameplayScreenPresenter(
                view, 
                _container.Resolve<GameplayPresentersFactory>(),
                _container.Resolve<MainHeroHolderService>(),
                _container.Resolve<ProjectPresentersFactory>()
                );
        }

        public StagePresenter CreateStagePresenter(IconTextView view)
        {
            return new StagePresenter(
                view,
                _container.Resolve<StageProviderService>()
                );
        }

        public SelectableAbilityPresenter CreateSelectableAbilityPresenter(
                    AbilityConfig abilityConfig,
                    SelectableAbilityView view,
                    Entity entity,
                    int level)
        {
            return new SelectableAbilityPresenter(abilityConfig, view, _container.Resolve<AbilityFactory>(), entity, level);
        }

        public AbilitySelectPopupPresenter CreateAbilitySelectPopupPresenter(
            AbilitySelectPopupView view,
            Entity entity,
            int level)
        {
            return new AbilitySelectPopupPresenter(
                _container.Resolve<ICoroutinesPerformer>(),
                view,
                entity,
                this,
                _container.Resolve<AbilityDropService>(),
                _container.Resolve<ViewsFactory>(),
                level
                );
        }

        public MainHeroExperiencePresenter CreateMainHeroExperiencePresenter(BarWithText view)
        {
            return new MainHeroExperiencePresenter(
                _container.Resolve<MainHeroHolderService>(),
                view,
                _container.Resolve<ConfigsProviderService>().GetConfig<ExperienceForUpgradeLevelConfig>());
        }
    }
}