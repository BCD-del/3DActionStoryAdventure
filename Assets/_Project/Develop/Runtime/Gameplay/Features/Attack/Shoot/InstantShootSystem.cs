using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Utilites.Reactive;
using System;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.Attack.Shoot
{
    public class InstantShootSystem : IInitializableSystem, IDisposableSystem
    {
        private readonly EntitiesFactory _entitiesFactory;

        private Entity _entity;

        private ReactiveEvent _attackDelayEndEvent;
        private ReactiveVariable<float> _damage;
        private Transform _shootPoint;

        private IDisposable _attackDelayDisposable;

        public InstantShootSystem(EntitiesFactory entitiesFactory)
        {
            _entitiesFactory = entitiesFactory;
        }

        public void OnInit(Entity entity)
        {
            _entity = entity;

            _attackDelayEndEvent = entity.AttackDelayEndEvent;
            _damage = entity.InstantAttackDamage;
            _shootPoint = entity.ShootPoint;

            _attackDelayDisposable = _attackDelayEndEvent.Subscribe(OnAttackDelayEnd);
        }

        public void OnDispose()
        {
            _attackDelayDisposable.Dispose();
        }

        private void OnAttackDelayEnd()
        {
            _entitiesFactory.CreateFireballProjectile(_shootPoint.position, _shootPoint.forward, _damage.Value, _entity);
        }
    }
}