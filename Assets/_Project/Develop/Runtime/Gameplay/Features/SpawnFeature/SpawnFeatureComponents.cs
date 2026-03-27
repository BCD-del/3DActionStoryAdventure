using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Utilites.Reactive;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.SpawnFeature
{
    public class SpawnInitialTime : IEntityComponent
    {
        public ReactiveVariable<float> Value;
    }

    public class SpawnCurrentTime : IEntityComponent
    {
        public ReactiveVariable<float> Value;
    }

    public class InSpawnProcess : IEntityComponent
    {
        public ReactiveVariable<bool> Value;
    }
}
