using Assets._Project.Develop.Infrastructure.DI;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Moveables;
using Assets._Project.Develop.Runtime.Utilites.AssetsManagment;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.Moveables
{
    public class MoveablesFactory
    {
        DIContainer _container;
        public MoveablesFactory(DIContainer container)
        {
            _container = container;
        }

        public GameObject CreateMoveable(MoveablesConfig config)
        {
            ResourcesAssetsLoader resourcesAssetsLoader = _container.Resolve<ResourcesAssetsLoader>();
            GameObject MoveablePrefab = resourcesAssetsLoader.Load<GameObject>(config.PrefabPath);
            GameObject instance = Object.Instantiate(MoveablePrefab);
            return instance;

        }
    }
}
