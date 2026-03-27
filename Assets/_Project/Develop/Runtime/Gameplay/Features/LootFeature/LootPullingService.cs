using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Utilites.Reactive;
using DG.Tweening;
using System.Collections.Generic;
using System;
using UnityEngine;
using Assets._Project.Develop.Runtime.Infrastructure.DI;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.LootFeature
{
    public class LootPullingService : IInitializable, IDisposable
    {
        private ReactiveVariable<bool> _allCollected = new();

        private List<Entity> _loot = new();

        private EntitiesLifeContext _entitiesLifeContext;

        private bool _isActivated;

        public LootPullingService(EntitiesLifeContext entitiesLifeContext)
        {
            _entitiesLifeContext = entitiesLifeContext;
        }

        public IReadOnlyVariable<bool> AllCollected => _allCollected;

        public void Initialize()
        {
            _entitiesLifeContext.Added += OnEntityAdded;
            _entitiesLifeContext.Released += OnEntityReleased;
        }

        public void Dispose()
        {
            _entitiesLifeContext.Added -= OnEntityAdded;
            _entitiesLifeContext.Released -= OnEntityReleased;
        }

        public void PullTo(Entity entity)
        {
            if (_isActivated)
                throw new InvalidOperationException();

            _isActivated = true;

            if (_loot.Count == 0)
            {
                _allCollected.Value = true;
                return;
            }

            foreach (Entity loot in _loot)
            {
                loot.CurrentTarget.Value = entity;
                loot.IsPullingProcess.Value = true;
            }
        }

        public void Reset()
        {
            _isActivated = false;
            _allCollected.Value = false;
        }

        private void OnEntityReleased(Entity entity)
        {
            bool lootRemoved = _loot.Remove(entity);

            if (lootRemoved && _loot.Count == 0)
            {
                _allCollected.Value = true;
            }
        }

        private void OnEntityAdded(Entity entity)
        {
            if (entity.HasComponent<IsPullable>() == false)
                return;

            _loot.Add(entity);

            Transform lootTransform = entity.Transform;

            Vector2 randomOffset = UnityEngine.Random.insideUnitCircle;
            Vector3 offset = new Vector3(randomOffset.x, 0, randomOffset.y);
            Vector3 endJumpPosition = lootTransform.position + offset;

            lootTransform
                .DOJump(endJumpPosition, 2, 1, 0.7f)
                .SetEase(Ease.OutBounce)
                .OnComplete(() => entity.InSpawnProcess.Value = false)
                .Play();
        }
    }
}