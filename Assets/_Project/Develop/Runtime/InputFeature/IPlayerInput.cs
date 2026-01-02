using UnityEngine;

namespace Assets._Project.Develop.Runtime.InputFeature
{
    public interface IPlayerInput
    {
        Vector3 GetMovementDirection();
        bool IsJumpKeyPressed();
    }
}
