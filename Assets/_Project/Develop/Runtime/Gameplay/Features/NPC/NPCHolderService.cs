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
                return null;
            }
            if (string.IsNullOrEmpty(config.PrefabPath))
            {
                Debug.LogError($"CreateNPC: PrefabPath is null or empty for config '{config.name}'");
                return null;
            }

            ResourcesAssetsLoader resourcesAssetsLoader = _container.Resolve<ResourcesAssetsLoader>();
            GameObject NPCPrefab = resourcesAssetsLoader.Load<GameObject>(config.PrefabPath);

          /*  {
                Debug.LogError($"Failed to load NPC prefab at path: {config.PrefabPath}");
                return null;
            }*/
            GameObject instanse = Object.Instantiate(NPCPrefab);
            return instanse;
        }
    }
}
