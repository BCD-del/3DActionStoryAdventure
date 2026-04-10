using Assets._Project.Develop.Infrastructure.DI;
using Assets._Project.Develop.NPCConfigsGameplay.NPCConfigs;
using Assets._Project.Develop.Runtime.Utilites.AssetsManagment;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.NPC
{
    public class NPCHolderService
    {
        private DIContainer _container;

        public NPCHolderService(DIContainer container)
        {
            _container = container;
        }

        public GameObject CreateNPC(NPCConfig config)
        {
            if (config == null)
            {
                Debug.LogError("CreateNPC: config is null");
            }

            ResourcesAssetsLoader resourcesAssetsLoader = _container.Resolve<ResourcesAssetsLoader>();
            GameObject NPCPrefab = resourcesAssetsLoader.Load<GameObject>(config.PrefabPath);
            GameObject instanse = Object.Instantiate(NPCPrefab);
            return instanse;
        }
    }
}
