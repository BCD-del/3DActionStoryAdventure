using UnityEngine;

namespace Assets._Project.Develop.Runtime.InputFeature
{
    public class PCPlayerInput : IPCPlayerInput
    {
        public float Horizontal => Input.GetAxisRaw("Horizontal");
        public float Vertical => Input.GetAxisRaw("Vertical");
        public bool Jump => Input.GetButtonDown("Jump");
        public float MouseX => Input.GetAxis("Mouse X");
        public float MouseY => Input.GetAxis("Mouse Y");
    }
}
