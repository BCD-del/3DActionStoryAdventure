using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Utilites.Reactive;
using System;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.LootFeature
{
    public class CollectExperienceToTargetSystem : IInitializableSystem, IDisposableSystem
    {
        private ReactiveVariable<Entity> _target;
        private ReactiveVariable<float> _experience;
        private ReactiveVariable<bool> _isCollected;

        private IDisposable _collectedChangeDisposable;

        public void OnInit(Entity entity)
        {
            _target = entity.CurrentTarget;
            _experience = entity.Experience;
            _isCollected = entity.IsCollected;

            _collectedChangeDisposable = _isCollected.Subscribe(OnIsCollectedChanged);
        }

        private void OnIsCollectedChanged(bool arg1, bool isCollected)
        {
            //по хорошему сделать реализацию через реквест и определить единожды логику начисления экспы,
            //но пока логика экспы только тут - не так принципиально
            if (isCollected)
                _target.Value.Experience.Value += _experience.Value;
        }

        public void OnDispose()
        {
            _collectedChangeDisposable.Dispose();
        }
    }
}