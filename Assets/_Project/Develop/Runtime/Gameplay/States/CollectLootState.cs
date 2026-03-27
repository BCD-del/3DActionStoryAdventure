using Assets._Project.Develop.Runtime.Gameplay.Features.LootFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.MainHero;
using Assets._Project.Develop.Runtime.Utilites.StateMachineCore;

namespace Assets._Project.Develop.Runtime.Gameplay.States
{
    public class CollectLootState : State, IUpdatableState
    {
        private LootPullingService _lootPullingService;
        private MainHeroHolderService _mainHeroHolderService;

        public CollectLootState(
            LootPullingService lootPullingService,
            MainHeroHolderService mainHeroHolderService)
        {
            _lootPullingService = lootPullingService;
            _mainHeroHolderService = mainHeroHolderService;
        }

        public override void Enter()
        {
            base.Enter();

            _lootPullingService.PullTo(_mainHeroHolderService.MainHero);
        }

        public override void Exit()
        {
            base.Exit();

            _lootPullingService.Reset();
        }

        public void Update(float deltaTime)
        {
        }
    }
}