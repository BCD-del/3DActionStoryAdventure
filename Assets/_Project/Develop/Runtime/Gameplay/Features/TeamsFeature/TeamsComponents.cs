using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Utilites.Reactive;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.TeamsFeature
{
    public class Team : IEntityComponent
    {
        public ReactiveVariable<Teams> Value;
    }
}
