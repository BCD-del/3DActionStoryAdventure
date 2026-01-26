using UnityEngine;

namespace Assets._Project.Develop.Runtime.Рефакторинг.Components
{
    public class PlayerMoverManager : MonoBehaviour
    {
        [SerializeField] private Rigidbody _rigidbody;
        private float _moveSpeed = 5;
        private float _acceleration = 10;
        private float _deceleration = 15;
        private Vector3 _moveInput;

        public void SetMoveInput(Vector3 moveInput)
        {
            _moveInput = moveInput.normalized;
        }

        public void FixedUpdate()
        {
            Vector3 desiredVelocityXZ = _moveInput * _moveSpeed;

            Vector3 currentVelocityXZ = new Vector3(_rigidbody.linearVelocity.x, 0f, _rigidbody.linearVelocity.z);


            if (_moveInput.magnitude > 0.1f)
            {
                currentVelocityXZ = Vector3.MoveTowards(currentVelocityXZ, desiredVelocityXZ, _acceleration * Time.fixedDeltaTime);
            }
            else
            {
                currentVelocityXZ = Vector3.MoveTowards(currentVelocityXZ, Vector3.zero, _deceleration * Time.fixedDeltaTime);
            }

            _rigidbody.linearVelocity = new Vector3(currentVelocityXZ.x, _rigidbody.linearVelocity.y, currentVelocityXZ.z);
        }
    }
}
