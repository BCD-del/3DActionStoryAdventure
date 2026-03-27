using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Utilites.Reactive;
using System;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.Attack.Shoot
{
    public class DirectionsInstantShootSystem : IInitializableSystem, IDisposableSystem
    {
        private readonly EntitiesFactory _entitiesFactory;

        private InstantShootingDirectionArgs _directions;
        private ReactiveEvent _attackDelayEndEvent;

        private Entity _entity;

        private ReactiveVariable<float> _damage;
        private Transform _shootPoint;

        private IDisposable _attackDelayEndDisposable;

        public DirectionsInstantShootSystem(EntitiesFactory entitiesFactory)
        {
            _entitiesFactory = entitiesFactory;
        }

        public void OnInit(Entity entity)
        {
            _entity = entity;

            _attackDelayEndEvent = entity.AttackDelayEndEvent;
            _directions = entity.InstantShootingDirection;

            _damage = entity.InstantAttackDamage;
            _shootPoint = entity.ShootPoint;

            _attackDelayEndDisposable = _attackDelayEndEvent.Subscribe(OnAttackDelayEnd);
        }

        private void OnAttackDelayEnd()
        {
            foreach (var arg in _directions.Args)
                Shoot(arg.Angel, arg.ProjectileCounts);
        }

        private void Shoot(int angel, int projectileCounts)
        {
            Vector3 directionForShoot = Quaternion.Euler(new Vector3(0, angel, 0)) * _shootPoint.forward;
            Vector2 perpindicular = Vector2.Perpendicular(new Vector2(directionForShoot.x, directionForShoot.z)).normalized;

            float offesetBetweenProjectiles = 0.6f;

            for (int i = 0; i < projectileCounts; i++)
            {
                Vector2 offset = perpindicular * (-offesetBetweenProjectiles / 2f * (projectileCounts - 1) + i * offesetBetweenProjectiles);
                Vector3 position = new Vector3(_shootPoint.position.x + offset.x, _shootPoint.position.y, _shootPoint.position.z + offset.y);

                _entitiesFactory.CreateFireballProjectile(position, directionForShoot, _damage.Value, _entity);
            }
        }

        public void OnDispose()
        {
            _attackDelayEndDisposable.Dispose();
        }
    }
}