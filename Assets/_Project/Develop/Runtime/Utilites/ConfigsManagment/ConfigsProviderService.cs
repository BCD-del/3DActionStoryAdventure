using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace Assets._Project.Develop.Runtime.Utilites.ConfigsManagment
{
    public class ConfigsProviderService
    {
        private readonly Dictionary<Type, object> _configs = new();

        private readonly IConfigLoader[] _loaders;

        public ConfigsProviderService(params IConfigLoader[] loaders)
        {
            _loaders = loaders;
        }

        public IEnumerator LoadAsync()
        {
            _configs.Clear();

            foreach (IConfigLoader loader in _loaders)
            {
                yield return loader.LoadAsync(loadedConfigs => _configs.AddRange(loadedConfigs));
            }
        }

        public T GetConfig<T>() where T : class
        {
            if (_configs.ContainsKey(typeof(T)) == false)
                throw new InvalidOperationException($"not found config by {typeof(T)}");

            return (T)_configs[typeof(T)];
        }
    }
}