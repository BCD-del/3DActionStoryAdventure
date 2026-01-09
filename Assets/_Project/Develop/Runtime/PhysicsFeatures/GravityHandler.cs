using UnityEngine;

namespace Assets._Project.Develop.Runtime.PhysicsFeatures
{
    public class GravityHandler
    {
        private Rigidbody _rigidbody;
        private GroundChecker _groundChecker;
        private float _gravity;

        public GravityHandler(float gravity, Rigidbody rigidbody, GroundChecker groundChecker)
        {
            _gravity = gravity;
            _rigidbody = rigidbody;
            _groundChecker = groundChecker;
        }

        public void ApplyGravity(float deltaTime )
        {
            if (_groundChecker.IsTouched() == false)
            {
                _rigidbody.linearVelocity += Vector3.down * _gravity * _gravity * deltaTime;
            }
            else
            {
                Vector3 velocity = _rigidbody.linearVelocity;
                velocity.y = 0;
                _rigidbody.linearVelocity = velocity;
            }
        }
    }
}
