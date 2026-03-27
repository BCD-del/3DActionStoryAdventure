using Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.MainHero;
using Assets._Project.Develop.Runtime.Gameplay.Features.PauseFeature;
using Assets._Project.Develop.Runtime.Meta.Features.LevelsProgression;
using Assets._Project.Develop.Runtime.Meta.Features.Stats;
using Assets._Project.Develop.Runtime.Meta.Features.Wallet;
using Assets._Project.Develop.Runtime.UI.Gameplay;
using Assets._Project.Develop.Runtime.Utilites.CoroutinesManagment;
using Assets._Project.Develop.Runtime.Utilites.DataManagment.DataProviders;
using Assets._Project.Develop.Runtime.Utilites.SceneManagement;
using Assets._Project.Develop.Runtime.Utilites.StateMachineCore;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.States
{
    public class WinState : EndGameState, IUpdatableState
    {
        private readonly LevelsProgressionService _levelsProgressionService;
        private readonly GameplayInputArgs _gameplayInputArgs;
        private readonly PlayerDataProvider _playerDataProvider;
        private readonly ICoroutinesPerformer _coroutinesPerformer;

        private readonly WalletService _walletService;
        private readonly MainHeroHolderService _mainHeroHolderService;

        private readonly GameStatsService _gameStatsService;


        private readonly GameplayPopupService _popupService;

        public WinState(
            IInputService inputService,
            LevelsProgressionService levelsProgressionService,
            GameplayInputArgs gameplayInputArgs,
            PlayerDataProvider playerDataProvider,
            ICoroutinesPerformer coroutinesPerformer,
            GameplayPopupService gameplayPopupService,
            IPauseService pauseService,
            WalletService walletService,
            MainHeroHolderService mainHeroHolderService,
            GameStatsService gameStatsService) : base(inputService, pauseService)
        {
            _levelsProgressionService = levelsProgressionService;
            _gameplayInputArgs = gameplayInputArgs;
            _playerDataProvider = playerDataProvider;
            _coroutinesPerformer = coroutinesPerformer;
            _popupService = gameplayPopupService;
            _walletService = walletService;
            _mainHeroHolderService = mainHeroHolderService;
            _gameStatsService = gameStatsService;
        }

        public override void Enter()
        {
            base.Enter();

            Debug.Log("ПОБЕДА!");

            _walletService.Add(CurrencyTypes.Gold, _mainHeroHolderService.MainHero.Coins.Value);
            _walletService.Add(CurrencyTypes.Wins, 1);
            _gameStatsService.Wins.Value++;

            _levelsProgressionService.AddLevelToCompleted(_gameplayInputArgs.LevelNumber);

            _coroutinesPerformer.StartPerform(_playerDataProvider.SaveAsync());

            _popupService.OpenWinPopup();
        }

        public void Update(float deltaTime)
        {
        }
    }
}