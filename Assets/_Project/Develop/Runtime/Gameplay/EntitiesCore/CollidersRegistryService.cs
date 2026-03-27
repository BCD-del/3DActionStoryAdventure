using System.Collections.Generic;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.EntitiesCore
{
    public class CollidersRegistryService
    {
        private Dictionary<Collider, Entity> _colliderToEntity = new();

        public void Register(Collider collider, Entity entity)
        {
            _colliderToEntity.Add(collider, entity);
        }

        public void Unregister(Collider collider)
        {
            _colliderToEntity.Remove(collider);
        }

        public Entity GetBy(Collider collider)
        {
            if (_colliderToEntity.TryGetValue(collider, out Entity entity))
                return entity;

            return null;
        }
    }
}
