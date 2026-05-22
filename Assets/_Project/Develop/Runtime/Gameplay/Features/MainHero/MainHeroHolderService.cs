using Assets._Project.Develop.Infrastructure.DI;
using Assets._Project.Develop.Runtime.Configs.MainHero;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Infrastructure.DI;
using Assets._Project.Develop.Runtime.Utilites.AssetsManagment;
using Assets._Project.Develop.Runtime.Utilites.Reactive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.MainHero
{
    public class MainHeroHolderService : IInitializable, IDisposable
    {
        private readonly EntitiesLifeContext _entitiesLifeContext;
        private ReactiveEvent<Entity> _heroRegistred = new();   

        private Entity _mainHero;

        public MainHeroHolderService(EntitiesLifeContext entitiesLifeContext)
        {
            _entitiesLifeContext = entitiesLifeContext;
      
        }

        public IReadOnlyEvent<Entity> HeroRegistred => _heroRegistred;
        public Entity MainHero => _mainHero;

        public void Initialize()
        {
            _entitiesLifeContext.Added += OnNewEntityAdded;
        }

        public void Dispose()
        {
            _entitiesLifeContext.Added -= OnNewEntityAdded;
        }

        private void OnNewEntityAdded(Entity entity)
        {
            Debug.Log("Im working");
            if (entity.HasComponent<IsMainHero>())
            {
                _entitiesLifeContext.Added -= OnNewEntityAdded;
                _mainHero = entity;
                _heroRegistred?.Invoke(_mainHero);

                Debug.Log("Hero registred");
        }
    }
}
