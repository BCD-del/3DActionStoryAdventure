using UnityEngine;

namespace Assets._Project.Develop.Runtime.InputFeature
{
    public class PCPlayerInput : IPlayerInput
    {
        public Vector3 GetMovementDirection()
        {
            float horizontalInput = Input.GetAxisRaw("Horizontal");
            float verticalInput = Input.GetAxisRaw("Vertical");

            return new Vector3(horizontalInput, 0, verticalInput);
        }
    }
}
