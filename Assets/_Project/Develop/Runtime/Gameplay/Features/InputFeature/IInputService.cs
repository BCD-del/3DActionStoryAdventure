using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature
{
    public interface IInputService
    {
        bool IsEnabled { get; set; }

        Vector3 MoveDirection { get; }
        Vector2 LookDirection { get; }
        bool IsAttackKeyPressed { get; }
        float ZoomInput { get; }

    }
}
