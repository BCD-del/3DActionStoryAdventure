using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Utilites.Reactive;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.AI
{
    public class CurrentTarget : IEntityComponent
    {
        public ReactiveVariable<Entity> Value;
    }
}
