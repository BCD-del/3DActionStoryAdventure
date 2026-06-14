using Assets._Project.Develop.Runtime.Configs.MainHero;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Mono;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Gameplay.Features.ApplyDamage;
using Assets._Project.Develop.Runtime.Utilites.Reactive;
using System;
using UnityEngine;


namespace Assets._Project.Develop.Runtime.Gameplay.Features.Attack.MeleeAttack
{
    public class DealAttackSystem : IInitializableSystem, IDisposableSystem
    {
        private ReactiveEvent _attackDelayEndEvent;
        private Transform _transform;

        private IDisposable _attackDelayEndEventDisposable;

        public void OnInit(Entity entity)
        {
            _attackDelayEndEvent = entity.AttackDelayEndEvent;
            _transform = entity.Transform;

            _attackDelayEndEventDisposable = _attackDelayEndEvent.Subscribe(Attack);
        }

        public void OnDispose()
        {
           _attackDelayEndEventDisposable.Dispose();
        }

        private void Attack()
        {
            Collider[] hits = Physics.OverlapSphere(_transform.position, 1); // 1 - range attack, config -> component
            
            // collider registry service
            foreach (var hit in hits)
            {
                if (hit.TryGetComponent(out MonoEntity entity))
                {
                    if (entity.LinkedEntity.HasComponent<TakeDamageRequest>())
                        entity.LinkedEntity.TakeDamageRequest.Invoke(1); // 1 - attack damage, config -> component
                }
            }
        }
    }
}
