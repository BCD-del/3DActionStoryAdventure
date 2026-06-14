using Assets._Project.Develop.Infrastructure.DI;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Camera;
using Assets._Project.Develop.Runtime.Utilites.AssetsManagment;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.CameraFeature
{
    public class CameraFactory
    {
        DIContainer _container;
        public CameraFactory(DIContainer container)
        {
            _container = container;
        }

        public GameObject CreateCamera(CameraConfig config)
        {
            ResourcesAssetsLoader resourcesAssetsLoader = _container.Resolve<ResourcesAssetsLoader>();
            GameObject CameraPrefab = resourcesAssetsLoader.Load<GameObject>(config.PrefabPath);
            GameObject instance = Object.Instantiate(CameraPrefab);
            return instance;
        }
    }
}
