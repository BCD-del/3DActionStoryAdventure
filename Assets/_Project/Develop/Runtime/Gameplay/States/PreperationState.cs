using Assets._Project.Develop.Runtime.Gameplay.Features.StageFeature;
using Assets._Project.Develop.Runtime.Utilites.SceneManagement;
using Assets._Project.Develop.Runtime.Utilites.StateMachineCore;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.States
{
    public class PreperationState : State, IUpdatableState
    {
        private PreperationTriggerService _preperationTriggerService;
        private readonly GameplayInputArgs _inputArgs;

        public PreperationState(PreperationTriggerService preperationTriggerService, GameplayInputArgs inputArgs)
        {
            _preperationTriggerService = preperationTriggerService;
            _inputArgs = inputArgs;
        }

        public override void Enter()
        {
            base.Enter();

            Vector3 nextStageTriggerPosition = _inputArgs.LevelSpawnPointPosition;
            _preperationTriggerService.Create(nextStageTriggerPosition);
        }

        public void Update(float deltaTime)
        {
            _preperationTriggerService.Update(deltaTime);
        }

        public override void Exit()
        {
            base.Exit();

            _preperationTriggerService.Cleanup();
        }
    }
}
