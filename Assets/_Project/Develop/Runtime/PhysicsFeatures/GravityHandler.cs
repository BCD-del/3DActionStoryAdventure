using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.PhysicsFeatures
{
    public class GravityHandler
    {
        private Rigidbody _rigidbody;
        private float _gravity;
        private GroundChecker _groundChecker;

        public GravityHandler(float gravity, Rigidbody rigidbody)
        {
            _gravity = gravity;
            _rigidbody = rigidbody;
        }
        public void ApplyGravity()
        {
            if (_groundChecker.IsTouched())
            {
                _gravity = -2;
            }
            else
            {
                _gravity = -2;
                _rigidbody.linearVelocity = Vector3.down * _gravity;
            }
        }
    }
}
