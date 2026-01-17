using Assets._Project.Develop.Runtime.InputFeature;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Рефакторинг
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private PlayerMotor _motor;
        [SerializeField] private PlayerLook _look;
        [SerializeField] private PlayerJump _jump;
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private Transform _legsPoint;
        [SerializeField] private LayerMask _groundMask;

        private PCPlayerInput _input;
        public bool OnGround => Physics.OverlapSphere(_legsPoint.position, 0.5f, _groundMask).Length > 0;

        public void Start()
        {
            _input = new PCPlayerInput();
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void Update()
        {
            _motor.SetMoveInput(transform.right * _input.Horizontal + transform.forward * _input.Vertical);
            _look.Look(_input.MouseX, _input.MouseY);
            _jump.TryJump(_input.Jump);
        }
    }
}