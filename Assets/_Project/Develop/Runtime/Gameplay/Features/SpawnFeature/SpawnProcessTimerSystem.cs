using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Utilites.Reactive;


namespace Assets._Project.Develop.Runtime.Gameplay.Features.SpawnFeature
{
    public class SpawnProcessTimerSystem : IInitializableSystem, IUpdatableSystem
    {
        private ReactiveVariable<float> _initialTime;
        private ReactiveVariable<float> _currentTime;

        private ReactiveVariable<bool> _inSpawnProcess;

        public void OnInit(Entity entity)
        {
            _initialTime = entity.SpawnInitialTime;
            _currentTime = entity.SpawnCurrentTime;
            _inSpawnProcess = entity.InSpawnProcess;
            
            _currentTime.Value = 0;
            _inSpawnProcess.Value = true;
        }

        public void OnUpdate(float deltaTime)
        {
            if (_inSpawnProcess.Value == false)
                return;

            _currentTime.Value += deltaTime;

            if (_currentTime.Value >= _initialTime.Value)
            {
                _inSpawnProcess.Value = false;
            }
        }
    }
}