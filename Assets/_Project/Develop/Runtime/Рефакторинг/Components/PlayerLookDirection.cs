using UnityEngine;

namespace Assets._Project.Develop.Runtime.Рефакторинг.Components
{
    public class PlayerLookDirection
    {
        private Camera _camera;
        private GameObject _targetOfTracking;
        private float _sensitivity = 100f;
        private float _verticalLimit = 85f;

        private float _rotationX;

        public PlayerLookDirection(Camera camera, float sensitivity, float verticalLimit, GameObject targetOfTracking)
        {
            _camera = camera;
            _sensitivity = sensitivity;
            _verticalLimit = verticalLimit;
            _targetOfTracking = targetOfTracking;
        }

        public void Look(float mouseX, float mouseY)
        {
            mouseX *= _sensitivity * Time.deltaTime;
            mouseY *= _sensitivity * Time.deltaTime;

            _rotationX -= mouseY;
            _rotationX = Mathf.Clamp(_rotationX, -_verticalLimit, _verticalLimit);

            _camera.transform.localRotation = Quaternion.Euler(_rotationX, 0f, 0f);
          _targetOfTracking.transform.Rotate(Vector3.up * mouseX);
        }
    }
}

