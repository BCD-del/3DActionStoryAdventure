using Assets._Project.Develop.Infrastructure.DI;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Abilities;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Levels;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Loot;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Mono;
using Assets._Project.Develop.Runtime.Gameplay.Features.AbilitiesDroppingFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.AbilitiesFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.AI;
using Assets._Project.Develop.Runtime.Gameplay.Features.Enemies;
using Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.LevelUPFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.LootFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.MainHero;
using Assets._Project.Develop.Runtime.Gameplay.Features.NPC;
using Assets._Project.Develop.Runtime.Gameplay.Features.PauseFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.StageFeature;
using Assets._Project.Develop.Runtime.Gameplay.States;
using Assets._Project.Develop.Runtime.Meta.Features.Stats;
using Assets._Project.Develop.Runtime.Meta.Features.StatsUpgrade;
using Assets._Project.Develop.Runtime.UI;
using Assets._Project.Develop.Runtime.UI.Core;
using Assets._Project.Develop.Runtime.UI.Gameplay;
using Assets._Project.Develop.Runtime.Utilites.AssetsManagment;
using Assets._Project.Develop.Runtime.Utilites.ConfigsManagment;
using Assets._Project.Develop.Runtime.Utilites.CoroutinesManagment;
using Assets._Project.Develop.Runtime.Utilites.DataManagment.DataProviders;
using Assets._Project.Develop.Runtime.Utilites.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Assets._Project.Develop.Runtime.Gameplay.Infrastructure
{
    public class GameplayContextRegistrations
    {
        private static GameplayInputArgs _inputArgs;

        public static void Process(DIContainer container, GameplayInputArgs inputArgs)
        {
            _inputArgs = inputArgs;



            container.RegisterAsSingle(CreateAbilityFactory);
            container.RegisterAsSingle(CreateAbilityDropingRulesService);
            container.RegisterAsSingle(CreateAbilityDropService);

            container.RegisterAsSingle(CreateLootFactory);
            container.RegisterAsSingle(CreateDropLootService);
            container.RegisterAsSingle(CreateLootPullingService).NonLazy();

            container.RegisterAsSingle(CreateDropAbilityOnMainHeroLevelUpService).NonLazy();

            container.RegisterAsSingle<IPauseService>(CreateTimeScalePauseService);

            container.RegisterAsSingle(CreateGameplayUIRoot).NonLazy();
            container.RegisterAsSingle(CreateGameplayScreenPresenter).NonLazy();
            container.RegisterAsSingle(CreateGameplayPopupService);
            container.RegisterAsSingle(CreateGameplayPresentersFactory);

            container.RegisterAsSingle(CreateEntitiesFactory);
            container.RegisterAsSingle(CreateEntitiesLifeContext);
            container.RegisterAsSingle(CreateMonoEntitiesFactory).NonLazy();

            container.RegisterAsSingle(CreateMainHeroHolderService).NonLazy();

            container.RegisterAsSingle(CreateCollidersRegistryService);

            container.RegisterAsSingle(CreateBrainsFactory);

            container.RegisterAsSingle(CreateAIBrainContext);

            container.RegisterAsSingle<IInputService>(CreateDesktopInput);

            container.RegisterAsSingle(CreateMainHeroFactory);

            container.RegisterAsSingle(CreateNPCHolderService);

            container.RegisterAsSingle(CreateEnemiesFactory);
            container.RegisterAsSingle(CreateStagesFactory);

            container.RegisterAsSingle(CreateStageProviderService);
            container.RegisterAsSingle(CreatePreperationTriggerService);

            container.RegisterAsSingle(CreateGameplayStatesFactory);
            container.RegisterAsSingle(CreateGameplayStatesContext);

            container.RegisterAsSingle(CreateGameStatsService);
        }


        private static GameStatsService CreateGameStatsService(DIContainer container)
        {
            return new GameStatsService(container.Resolve<PlayerDataProvider>());
        }

        private static NPCFactory CreateNPCHolderService(DIContainer container)
        {
            return new NPCFactory(container);
        }

        private static LootPullingService CreateLootPullingService(DIContainer c)
        {
            return new LootPullingService(c.Resolve<EntitiesLifeContext>());
        }

        private static DropLootService CreateDropLootService(DIContainer c)
        {
            return new DropLootService(
                c.Resolve<ConfigsProviderService>().GetConfig<LootListConfig>(),
                c.Resolve<LootFactory>());
        }

        private static LootFactory CreateLootFactory(DIContainer c)
        {
            return new LootFactory(c);
        }

        private static TimeScalePauseService CreateTimeScalePauseService(DIContainer c)
        {
            return new TimeScalePauseService();
        }

        private static DropAbilityOnMainHeroLevelUpService CreateDropAbilityOnMainHeroLevelUpService(DIContainer c)
        {
            return new DropAbilityOnMainHeroLevelUpService(
                c.Resolve<MainHeroHolderService>(),
                c.Resolve<GameplayPopupService>(),
                c.Resolve<ICoroutinesPerformer>(),
                c.Resolve<IPauseService>());
        }

        private static AbilityDropService CreateAbilityDropService(DIContainer container)
        {
            return new AbilityDropService(
                container.Resolve<ConfigsProviderService>().GetConfig<AbilitiesConfigsContainer>(),
                container.Resolve<AbilityDropingRulesService>());
        }

        private static AbilityDropingRulesService CreateAbilityDropingRulesService(DIContainer container)
        {
            return new AbilityDropingRulesService();
        }

        private static AbilityFactory CreateAbilityFactory(DIContainer container)
        {
            return new AbilityFactory(container);
        }

        private static GameplayStatesContext CreateGameplayStatesContext(DIContainer container)
        {
            return new GameplayStatesContext(
                container.Resolve<GameplayStatesFactory>()
                .CreateGameplayStateMachine(_inputArgs));
        }

        private static GameplayStatesFactory CreateGameplayStatesFactory(DIContainer container)
        {
            return new GameplayStatesFactory(container, _inputArgs);
        }

        private static MainHeroHolderService CreateMainHeroHolderService(DIContainer container)
        {
            return new MainHeroHolderService(container.Resolve<EntitiesLifeContext>());
        }

        private static PreperationTriggerService CreatePreperationTriggerService(DIContainer container)
        {
            return new PreperationTriggerService(
                container.Resolve<EntitiesFactory>(),
                container.Resolve<EntitiesLifeContext>());
        }

        private static StageProviderService CreateStageProviderService(DIContainer container)
        {
            return new StageProviderService(
                container.Resolve<ConfigsProviderService>().GetConfig<LevelsListConfig>().GetBy(_inputArgs.LevelNumber),
                container.Resolve<StagesFactory>()
                );
        }

        private static StagesFactory CreateStagesFactory(DIContainer container)
        {
            return new StagesFactory(container);
        }

        private static MainHeroFactory CreateMainHeroFactory(DIContainer container)
        {
            return new MainHeroFactory(container);
        }

        private static EnemiesFactory CreateEnemiesFactory(DIContainer container)
        {
            return new EnemiesFactory(container);
        }

        private static DesktopInput CreateDesktopInput(DIContainer container)
        {
            return new DesktopInput();
        }

        private static AIBrainsContext CreateAIBrainContext(DIContainer container)
        {
            return new AIBrainsContext();
        }

        private static BrainsFactory CreateBrainsFactory(DIContainer container)
        {
            return new BrainsFactory(container);
        }

        private static CollidersRegistryService CreateCollidersRegistryService(DIContainer container)
        {
            return new CollidersRegistryService();
        }

        private static MonoEntitiesFactory CreateMonoEntitiesFactory(DIContainer container)
        {
            return new MonoEntitiesFactory(
                container.Resolve<ResourcesAssetsLoader>(),
                container.Resolve<EntitiesLifeContext>(),
                container.Resolve<CollidersRegistryService>());
        }

        private static EntitiesLifeContext CreateEntitiesLifeContext(DIContainer container)
        {
            return new EntitiesLifeContext();
        }

        private static EntitiesFactory CreateEntitiesFactory(DIContainer container)
        {
            return new EntitiesFactory(container);
        }

        private static GameplayPopupService CreateGameplayPopupService(DIContainer container)
        {
            return new GameplayPopupService(
                container.Resolve<ViewsFactory>(),
                container.Resolve<ProjectPresentersFactory>(),
                container.Resolve<GameplayUIRoot>(),
                container.Resolve<GameplayPresentersFactory>()
                );
        }

        private static GameplayScreenPresenter CreateGameplayScreenPresenter(DIContainer container)
        {
            GameplayUIRoot uiRoot = container.Resolve<GameplayUIRoot>();

            GameplayScreenView view = container
                .Resolve<ViewsFactory>()
                .Create<GameplayScreenView>(ViewIDs.GameplayScreenView, uiRoot.HUDLayer);

            GameplayScreenPresenter presenter = container.Resolve<GameplayPresentersFactory>().CreateGameplayScreen(view);

            return presenter;
        }

        private static GameplayPresentersFactory CreateGameplayPresentersFactory(DIContainer container)
        {
            return new GameplayPresentersFactory(container, _inputArgs);
        }

        private static GameplayUIRoot CreateGameplayUIRoot(DIContainer container)
        {
            ResourcesAssetsLoader resourcesAssetsLoader = container.Resolve<ResourcesAssetsLoader>();

            GameplayUIRoot gameplayUIRoot = resourcesAssetsLoader
                .Load<GameplayUIRoot>("UI/Gameplay/GameplayUIRoot");

            return Object.Instantiate(gameplayUIRoot);
        }
    }
}
