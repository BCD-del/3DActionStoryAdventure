using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Mono;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.LifeCycle
{
    public class HealthBarPointRegistrator : MonoEntityRegistrator
    {
        [SerializeField] private Transform _point;

        public override void Register(Entity entity)
        {
            entity.AddHealthBarPoint(_point);
        }
    }
}
