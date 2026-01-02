using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.PhysicsFeatures
{
    public class GroundChecker
    {
        private Transform _legs;
        private float _legsRange;
        private LayerMask _jumpableMask;

        public GroundChecker(Transform legs, float legsRange, LayerMask jumpableMask)
        {
            _legs = legs;
            _legsRange = legsRange;
            _jumpableMask = jumpableMask;
        }

        public bool IsTouched()
        {
            return Physics.OverlapCapsule(_legs.position + Vector3.up * _legsRange, _legs.position + Vector3.down * _legsRange, _legsRange, _jumpableMask).Length > 0;
        }
    }
}
