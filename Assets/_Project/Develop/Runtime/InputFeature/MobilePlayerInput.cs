using UnityEngine;

namespace Assets._Project.Develop.Runtime.InputFeature
{
    public class MobilePlayerInput : IPlayerInput
    {
        public Vector3 GetMovementDirection()
        {
            return Vector3.zero;
        }
    }
}
