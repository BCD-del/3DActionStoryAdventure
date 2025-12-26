using System;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.MovementFeatures
{
    public class RigidbodyDirectionalMover
    {
        private Rigidbody _rigidbody;
        private float _movementSpeed;

        public RigidbodyDirectionalMover(Rigidbody rigidbody, float movementSpeed)
        {
            _rigidbody = rigidbody;
            _movementSpeed = movementSpeed;
        }

        public void Move(Vector3 direction)
        {
            _rigidbody.linearVelocity = direction.normalized * _movementSpeed;
        }
    }
}
