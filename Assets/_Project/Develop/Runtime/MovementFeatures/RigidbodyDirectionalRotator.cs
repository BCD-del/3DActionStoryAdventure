using UnityEngine;

namespace Assets._Project.Develop.Runtime.MovementFeatures
{
    public class RigidbodyDirectionalRotator
    {
        private Rigidbody _rigidbody;
        private float _rotationSpeed;

        public RigidbodyDirectionalRotator(Rigidbody rigidbody, float rotationSpeed)
        {
            _rigidbody = rigidbody;
            _rotationSpeed = rotationSpeed;
        }

        public void Rotate(Vector3 direction, float deltaTime)
        {
            Quaternion LookRotation = Quaternion.LookRotation(direction);
            float step = _rotationSpeed * deltaTime;
            Quaternion rotation = Quaternion.RotateTowards(_rigidbody.rotation, LookRotation, step);
            _rigidbody.MoveRotation(rotation);
        }
    }
}
