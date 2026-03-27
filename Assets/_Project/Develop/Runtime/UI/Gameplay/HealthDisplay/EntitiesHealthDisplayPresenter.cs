using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.UI.CommonViews;
using System.Collections.Generic;
using System;
using UnityEngine;
using Assets._Project.Develop.Runtime.UI.Core;
using Assets._Project.Develop.Runtime.Gameplay.Features.MainHero;

namespace Assets._Project.Develop.Runtime.UI.Gameplay.HealthDisplay
{
    public class EntitiesHealthDisplayPresenter : IPresenter
    {
        private readonly EntitiesLifeContext _entitiesLifeContext;
        private readonly EntitiesHealthDisplay _view;

        private readonly GameplayPresentersFactory _presentersFactory;
        private readonly ViewsFactory _viewsFactory;

        private Dictionary<Entity, EntityHealthBarInfo> _entityToHealthBarInfo = new();

        public EntitiesHealthDisplayPresenter(
            EntitiesLifeContext entitiesLifeContext,
            EntitiesHealthDisplay view,
            ViewsFactory viewsFactory,
            GameplayPresentersFactory presentersFactory)
        {
            _entitiesLifeContext = entitiesLifeContext;
            _view = view;
            _viewsFactory = viewsFactory;
            _presentersFactory = presentersFactory;
        }

        public void Initialize()
        {
            _entitiesLifeContext.Added += OnEntityAdded;
            _entitiesLifeContext.Released += OnEntityReleased;

            foreach (Entity entity in _entitiesLifeContext.Entities)
                OnEntityAdded(entity);
        }

        public void Dispose()
        {
            _entitiesLifeContext.Added -= OnEntityAdded;
            _entitiesLifeContext.Released -= OnEntityReleased;

            foreach (EntityHealthBarInfo info in _entityToHealthBarInfo.Values)
                DisposeFor(info);

            _entityToHealthBarInfo.Clear();
        }

        private void OnEntityReleased(Entity entity)
        {
            if (_entityToHealthBarInfo.ContainsKey(entity))
                RemoveHealthBarFor(entity);
        }

        private void OnEntityAdded(Entity entity)
        {
            if (entity.TryGetHealthBarPoint(out Transform healthBarPoint))
            {
                BarWithText healthBarView = null;

                if (entity.HasComponent<IsMainHero>())
                    healthBarView = _viewsFactory.Create<BarWithText>(ViewIDs.MainHeroHealthBar);
                else
                    healthBarView = _viewsFactory.Create<BarWithText>(ViewIDs.SimpleHealthBar);

                _view.Add(healthBarView);

                EntityHealthPresenter entityHealthPresenter = _presentersFactory.CreateEntityHealthPresenter(entity, healthBarView);
                entityHealthPresenter.Initialize();

                IDisposable removeReason = entity.IsDead.Subscribe((oldValue, isDead) =>
                {
                    if (isDead)
                        RemoveHealthBarFor(entity);
                });

                _entityToHealthBarInfo.Add(entity, new EntityHealthBarInfo(healthBarPoint, removeReason, entityHealthPresenter));
            }
        }

        public void LateUpdate()
        {
            foreach (KeyValuePair<Entity, EntityHealthBarInfo> info in _entityToHealthBarInfo)
                _view.UpdatePositionFor(info.Value.HealthPresenter.Bar, info.Value.HealthBarPoint.position);
        }

        private void RemoveHealthBarFor(Entity entity)
        {
            EntityHealthBarInfo info = _entityToHealthBarInfo[entity];
            DisposeFor(info);
            _entityToHealthBarInfo.Remove(entity);
        }

        private void DisposeFor(EntityHealthBarInfo info)
        {
            info.RemoveReason.Dispose();

            _view.Remove(info.HealthPresenter.Bar);
            _viewsFactory.Release(info.HealthPresenter.Bar);

            info.HealthPresenter.Dispose();
        }

        private class EntityHealthBarInfo
        {
            public EntityHealthBarInfo(
                Transform healthBarPoint,
                IDisposable removeReason,
                EntityHealthPresenter healthPresenter)
            {
                HealthBarPoint = healthBarPoint;
                RemoveReason = removeReason;
                HealthPresenter = healthPresenter;
            }

            public Transform HealthBarPoint { get; }
            public IDisposable RemoveReason { get; }
            public EntityHealthPresenter HealthPresenter { get; }
        }
    }
}