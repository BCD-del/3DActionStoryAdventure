using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerJump : MonoBehaviour
{
    [SerializeField] private float _jumpForce = 7f;
    [SerializeField] private Transform _legs;
    [SerializeField] private LayerMask _groundMask;
    [SerializeField] private Rigidbody _rigidbody;

    public void TryJump(bool JumpKeyPressed)
    {
        if (JumpKeyPressed && OnGround())
            _rigidbody.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
    }

    private bool OnGround()
    {
        return Physics.CheckSphere(_legs.position, 0.2f, _groundMask);
    }
}
