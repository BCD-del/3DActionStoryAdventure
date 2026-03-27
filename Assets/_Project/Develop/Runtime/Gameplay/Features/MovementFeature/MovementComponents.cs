using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Utilites.Conditions;
using Assets._Project.Develop.Runtime.Utilites.Reactive;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.MovementFeature
{
    public class MoveDirection : IEntityComponent
    {
        public ReactiveVariable<Vector3> Value;
    }

    public class MoveSpeed : IEntityComponent
    {
        public ReactiveVariable<float> Value;
    }

    public class IsMoving : IEntityComponent
    {
        public ReactiveVariable<bool> Value;
    }

    public class CanMove : IEntityComponent
    {
        public ICompositeCondition Value;
    }

    public class RotationDirection : IEntityComponent
    {
        public ReactiveVariable<Vector3> Value;
    }

    public class RotationSpeed : IEntityComponent
    {
        public ReactiveVariable<float> Value;
    }

    public class CanRotate : IEntityComponent
    {
        public ICompositeCondition Value;
    }
}
