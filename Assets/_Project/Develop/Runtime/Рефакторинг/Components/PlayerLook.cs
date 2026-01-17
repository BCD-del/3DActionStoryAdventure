using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    [SerializeField] private Camera _camera;
    [SerializeField] private float _sensitivity = 100f;
    [SerializeField] private float _verticalLimit = 85f;

    private float _rotationX;

    public void Look(float mouseX, float mouseY)
    {
        mouseX *= _sensitivity * Time.deltaTime;
        mouseY *= _sensitivity * Time.deltaTime;

        _rotationX -= mouseY;
        _rotationX = Mathf.Clamp(_rotationX, -_verticalLimit, _verticalLimit);

        _camera.transform.localRotation = Quaternion.Euler(_rotationX, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }
}
