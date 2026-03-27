using Assets._Project.Develop.Infrastructure.DI;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Utilites.Reactive;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.LootFeature
{
    public class LootFactory
    {
        private EntitiesFactory _entityFactory;
        private EntitiesLifeContext _entitiesLifeContext;

        public LootFactory(DIContainer container)
        {
            _entityFactory = container.Resolve<EntitiesFactory>();
            _entitiesLifeContext = container.Resolve<EntitiesLifeContext>();
        }

        public Entity CreateExperienceLoot(string prefabPath, Vector3 position, float experience)
        {
            Entity pullableBase = _entityFactory.CreatePullable(prefabPath, position);

            pullableBase
                .AddExperience(new ReactiveVariable<float>(experience))
                .AddSystem(new CollectExperienceToTargetSystem());

            _entitiesLifeContext.Add(pullableBase);

            return pullableBase;
        }

        public Entity CreateCoinsLoot(string prefabPath, Vector3 position, int coins)
        {
            Entity pullableBase = _entityFactory.CreatePullable(prefabPath, position);

            pullableBase
                .AddCoins(new ReactiveVariable<int>(coins))
                .AddSystem(new CollectCoinsToTargetSystem());

            _entitiesLifeContext.Add(pullableBase);

            return pullableBase;
        }


        public Entity CreateHealthLoot(string prefabPath, Vector3 position, float health)
        {
            Entity pullableBase = _entityFactory.CreatePullable(prefabPath, position);

            pullableBase
                .AddCurrentHealth(new ReactiveVariable<float>(health))
                .AddSystem(new CollectHealthToTargetSystem());

            _entitiesLifeContext.Add(pullableBase);

            return pullableBase;
        }
    }
}