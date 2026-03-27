using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature
{
    public class DesktopInput : IInputService
    {
        private const string HorizontalAxisName = "Horizontal";
        private const string VerticalAxisName = "Vertical";

        public bool IsEnabled { get; set; } = true;

        public Vector3 MoveDirection
        {
            get
            {
                if (IsEnabled == false)
                    return Vector3.zero;

                Vector3 direction = new Vector3(Input.GetAxisRaw(HorizontalAxisName), 0, Input.GetAxisRaw(VerticalAxisName));

                return direction;    
            }
        }

        public Vector3 RotateDirection
        {
            get
            {
                if (IsEnabled == false)
                    return Vector3.zero;

                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                Plane groundPlane = new Plane(Vector3.up, Vector3.zero); 

                if (groundPlane.Raycast(ray, out float rayDistance) == false)
                    return Vector3.zero;

                Vector3 targetPoint = ray.GetPoint(rayDistance);
                Vector3 lookDirection = new Vector3(targetPoint.x, 0, targetPoint.z);

                return lookDirection;
            }
        }

        public bool IsAttackKeyPressed => Input.GetKeyDown(KeyCode.Mouse0);
    }
}
