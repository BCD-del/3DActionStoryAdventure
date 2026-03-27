using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature
{
    public interface IInputService
    {
        bool IsEnabled { get; set; }

        Vector3 MoveDirection { get; }
        Vector3 RotateDirection { get; }
        bool IsAttackKeyPressed { get; }
    }
}
