using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Utilites.Reactive;
using System;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.LootFeature
{
    public class CollectHealthToTargetSystem : IInitializableSystem, IDisposableSystem
    {
        private ReactiveVariable<Entity> _target;
        private ReactiveVariable<float> _health;
        private ReactiveVariable<bool> _isCollected;

        private IDisposable _collectedChangeDisposable;

        public void OnInit(Entity entity)
        {
            _target = entity.CurrentTarget;
            _health = entity.CurrentHealth;
            _isCollected = entity.IsCollected;

            _collectedChangeDisposable = _isCollected.Subscribe(OnIsCollectedChanged);
        }

        private void OnIsCollectedChanged(bool arg1, bool isCollected)
        {
            if (isCollected)
            {
                //по хорошему сделать реализацию через реквест и определить единожды логику хила,
                //но пока логика хила только тут - не так принципиально
                ReactiveVariable<float> currentHealth = _target.Value.CurrentHealth;
                ReactiveVariable<float> maxHealth = _target.Value.MaxHealth;

                if (currentHealth.Value + _health.Value > maxHealth.Value)
                {
                    currentHealth.Value = maxHealth.Value;
                    return;
                }

                currentHealth.Value += _health.Value;
            }
        }

        public void OnDispose()
        {
            _collectedChangeDisposable.Dispose();
        }
    }
}