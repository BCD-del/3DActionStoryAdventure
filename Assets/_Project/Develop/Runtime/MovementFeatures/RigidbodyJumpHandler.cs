using System;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.MovementFeatures
{
    public class RigidbodyJumpHandler
    {
        private Rigidbody _rididbody;
        private float _jumpPower;

        public RigidbodyJumpHandler(Rigidbody rididbody, float jumpPower)
        {
            _rididbody = rididbody;
            _jumpPower = jumpPower;
        }

        public void Jump()
        {
            _rididbody.AddForce(Vector3.up * _jumpPower, ForceMode.Impulse);
        }
    }
}
