using UnityEngine;

namespace Assets._Project.Develop.Runtime.PhysicsFeatures
{
    public class GravityHandler
    {
        private Rigidbody _rigidbody;
        private float _gravity;
        private GroundChecker _groundChecker;

        public GravityHandler(float gravity, Rigidbody rigidbody, GroundChecker groundChecker)
        {
            _gravity = gravity;
            _rigidbody = rigidbody;
            _groundChecker = groundChecker;
        }
        public void ApplyGravity()
        {
            Debug.Log("Gravity");
            if (_groundChecker.IsTouched() == true)
            {
                _gravity = 2;
                _rigidbody.linearVelocity = Vector3.down * _gravity;
                Debug.Log("Grounded");
            }

            else
            {
                _gravity = -2;
              //  _rigidbody.linearVelocity = Vector3.down * _gravity;
                _rigidbody.linearVelocity = Vector3.up * _gravity;
                Debug.Log(_rigidbody.linearVelocity);
            }


        }
    }
}
