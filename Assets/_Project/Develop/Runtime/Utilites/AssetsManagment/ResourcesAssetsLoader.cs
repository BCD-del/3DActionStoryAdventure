using UnityEngine;

namespace Assets._Project.Develop.Runtime.Utilites.AssetsManagment
{
    public class ResourcesAssetsLoader
    {
        public T Load<T>(string resourcePath) where T : Object
            => Resources.Load<T>(resourcePath);
    }
}