using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.MainHero;
using Assets._Project.Develop.Runtime.Infrastructure.DI;
using Assets._Project.Develop.Runtime.Utilites.AssetsManagment;
using System;
using Unity.Cinemachine;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.Camera
{
    public class CameraService : IInitializable, IDisposable
    {
        private const string MainHeroCameraPath = "Camera/MainHeroCamera";

        // config
        public float OrbitSensitivityX = 0.15f;
        public float OrbitSensitivityY = 0.10f;

        public float MinPitchAngle = -30f;
        public float MaxPitchAngle = 50f;

        public float DefaultDistance = 5f;

        public float MinDistance = 2f;
        public float MaxDistance = 10f;

        public float ZoomSpeed = 2f;
        public float ZoomDamping = 10f;
        // config

        private readonly MainHeroHolderService _mainHeroHolderService;
        private readonly ResourcesAssetsLoader _resources;

        private CinemachineCamera _mainHeroCamera;
        private CinemachineOrbitalFollow _orbitalFollow;
        private IDisposable _heroRegistredDisposable;

        private float _yaw;
        private float _pitch;
        private float _currentDist;
        private float _targetDist;

        private Transform _heroTransform;

        public Transform CameraTransform => UnityEngine.Camera.main != null
            ? UnityEngine.Camera.main.transform
            : null;

        public CameraService(
            MainHeroHolderService mainHeroHolderService,
            ResourcesAssetsLoader resources)
        {
            _mainHeroHolderService = mainHeroHolderService;
            _resources = resources;
        }

        public void Initialize()
        {
            _heroRegistredDisposable =
                _mainHeroHolderService.HeroRegistred.Subscribe(OnMainHeroRegistred);

            _mainHeroCamera =
                Object.Instantiate(_resources.Load<CinemachineCamera>(MainHeroCameraPath));

            _orbitalFollow = _mainHeroCamera.GetComponent<CinemachineOrbitalFollow>();

            _currentDist = DefaultDistance;
            _targetDist = DefaultDistance;

            Transform cam = CameraTransform;

            if (cam != null)
            {
                _yaw = cam.eulerAngles.y;
                _pitch = cam.eulerAngles.x;
            }
        }

        public void Dispose() => _heroRegistredDisposable?.Dispose();

        public void Update(float deltaTime, Vector2 lookInput, float zoomInput)
        {
            UpdateOrbitCamera(deltaTime, lookInput, zoomInput);
        }

        private void UpdateOrbitCamera(float deltaTime, Vector2 lookInput, float zoomInput)
        {
            // Вращение
            _yaw += lookInput.x * OrbitSensitivityX;
            _pitch -= lookInput.y * OrbitSensitivityY;
            _pitch = Mathf.Clamp(_pitch, MinPitchAngle, MaxPitchAngle);

            if (Mathf.Abs(zoomInput) > 0.01f)
            {
                _targetDist -= zoomInput * ZoomSpeed;
                _targetDist = Mathf.Clamp(_targetDist, MinDistance, MaxDistance);
            }

            _currentDist = Mathf.Lerp(_currentDist, _targetDist, deltaTime * ZoomDamping);

            ApplyOrbitToCinemachine();
        }

        private void ApplyOrbitToCinemachine()
        {
            if (_orbitalFollow == null)
                return;

            _orbitalFollow.TargetOffset = Vector3.up * 3;
            _orbitalFollow.Radius = _currentDist;

            _orbitalFollow.HorizontalAxis.Value = _yaw;
            _orbitalFollow.VerticalAxis.Value = _pitch;
        }

        private void OnMainHeroRegistred(Entity entity)
        {
            _heroTransform = entity.Transform;
            _mainHeroCamera.Target.TrackingTarget = _heroTransform;
        }
    }
}