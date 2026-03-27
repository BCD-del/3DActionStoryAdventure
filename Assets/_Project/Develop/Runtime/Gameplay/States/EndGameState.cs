using Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.PauseFeature;
using Assets._Project.Develop.Runtime.Utilites.StateMachineCore;

namespace Assets._Project.Develop.Runtime.Gameplay.States
{
    public abstract class EndGameState : State
    {
        private readonly IInputService _inputService;
        private readonly IPauseService _pauseService;

        protected EndGameState(IInputService inputService, IPauseService pauseService)
        {
            _inputService = inputService;
            _pauseService = pauseService;
        }

        public override void Enter()
        {
            base.Enter();

            _inputService.IsEnabled = false;
            _pauseService.Pause();
        }

        public override void Exit()
        {
            base.Exit();

            _inputService.IsEnabled = true;
            _pauseService.Unpause();
        }
    }
}
