using Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.PauseFeature;
using Assets._Project.Develop.Runtime.Meta.Features.Stats;
using Assets._Project.Develop.Runtime.Meta.Features.Wallet;
using Assets._Project.Develop.Runtime.UI.Gameplay;
using Assets._Project.Develop.Runtime.Utilites.StateMachineCore;

namespace Assets._Project.Develop.Runtime.Gameplay.States
{
    public class DefeatState : EndGameState, IUpdatableState
    {
        private readonly GameplayPopupService _gameplayPopupService;
        private readonly WalletService _walletService;
        private readonly GameStatsService _gameStatsService;

        public DefeatState(
            IInputService inputService,
            GameplayPopupService gameplayPopupService,
            IPauseService pauseService,
            WalletService walletService,
            GameStatsService gameStatsService) : base(inputService, pauseService)
        {
            _gameplayPopupService = gameplayPopupService;
            _walletService = walletService;
            _gameStatsService = gameStatsService;
        }

        public override void Enter()
        {
            base.Enter();

            _gameplayPopupService.OpenDefeatPopup();

            _walletService.Add(CurrencyTypes.Loses, 1);
            _gameStatsService.Losses.Value++;
        }

        public void Update(float deltaTime)
        {
        }
    }
}
