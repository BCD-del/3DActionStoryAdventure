using Assets._Project.Develop.Runtime.Infrastructure.DI;
using System;

namespace Assets._Project.Develop.Infrastructure.DI
{
    public class Registration : IRegistrationOptions
    {
        private Func<DIContainer, object> _creator;
        private object _cachedInstance;
        
        public bool IsNonLazy { get; private set; }

        public Registration(Func<DIContainer, object> creator) => _creator = creator;

        public object CreateInstanceFrom(DIContainer container)
        {
            if (_cachedInstance != null)
                return _cachedInstance;

            if (_creator == null)
                throw new InvalidOperationException("No has instance or creator");

            _cachedInstance = _creator.Invoke(container);

            return _cachedInstance;
        }

        public void OnInitialize()
        {
            if (_cachedInstance != null)
                if (_cachedInstance is IInitializable initializable)
                    initializable.Initialize();
        }

        public void OnDispose()
        {
            if (_cachedInstance != null)
                if (_cachedInstance is IDisposable disposable)
                    disposable.Dispose();
        }

        public void NonLazy() => IsNonLazy = true;
    }
}
