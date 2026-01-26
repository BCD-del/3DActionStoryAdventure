using System.Diagnostics;
using UnityEngine;


public class PlayerJump
{
  float _jumpForce = 0.1f;
    private Transform _legs;
    private LayerMask _groundMask;
    private Rigidbody _rigidbody;

    public PlayerJump(float jumpForce, Transform legs, LayerMask groundMask, Rigidbody rigidbody)
    {
        _jumpForce = jumpForce;
        _legs = legs;
        _groundMask = groundMask;
        _rigidbody = rigidbody;
    }

    public void TryJump(bool JumpKeyPressed)
    {
        if (JumpKeyPressed && OnGround())
            _rigidbody.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
        Debug.Log(_jumpForce);
    }

    private bool OnGround()
    {
        return Physics.CheckSphere(_legs.position, 0.2f, _groundMask);
    }
}
