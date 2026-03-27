using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Utilites.Reactive;
using System;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.LootFeature
{
    public class CollectCoinsToTargetSystem : IInitializableSystem, IDisposableSystem
    {
        private ReactiveVariable<Entity> _target;
        private ReactiveVariable<int> _coins;
        private ReactiveVariable<bool> _isCollected;

        private IDisposable _collectedChangeDisposable;

        public void OnInit(Entity entity)
        {
            _target = entity.CurrentTarget;
            _coins = entity.Coins;
            _isCollected = entity.IsCollected;

            _collectedChangeDisposable = _isCollected.Subscribe(OnIsCollectedChanged);
        }

        private void OnIsCollectedChanged(bool arg1, bool isCollected)
        {
            //по хорошему сделать реализацию через реквест и определить единожды логику начисления монет,
            //но пока логика монет только тут - не так принципиально
            if (isCollected)
                _target.Value.Coins.Value += _coins.Value;
        }

        public void OnDispose()
        {
            _collectedChangeDisposable.Dispose();
        }
    }
}