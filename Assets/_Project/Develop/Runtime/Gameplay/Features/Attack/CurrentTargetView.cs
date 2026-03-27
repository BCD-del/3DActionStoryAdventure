using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Mono;
using Assets._Project.Develop.Runtime.Utilites.Reactive;
using System;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.Attack
{
    public class CurrentTargetView : EntityView
    {
        [SerializeField] private ParticleSystem _backlightPrefab;

        private ParticleSystem _backlight;

        private IReadOnlyVariable<Entity> _currentTarget;
        private Transform _currentTargetTransform;

        private IDisposable _currentTargetChangedDisposable;

        protected override void OnEntityStartedWork(Entity entity)
        {
            _currentTarget = entity.CurrentTarget;

            _backlight = Instantiate(_backlightPrefab);

            _currentTargetChangedDisposable = _currentTarget.Subscribe(OnCurrentTargetChanged);

            UpdateBacklightFor(_currentTarget.Value);
        }

        public override void Cleanup(Entity entity)
        {
            base.Cleanup(entity);

            _currentTargetChangedDisposable.Dispose();
            Destroy(_backlight.gameObject);
        }

        private void LateUpdate()
        {
            if (_currentTargetTransform == null)
                return;

            _backlight.transform.position = _currentTargetTransform.position;
        }

        private void OnCurrentTargetChanged(Entity entity1, Entity newTarget)
        {
            UpdateBacklightFor(newTarget);
        }

        private void UpdateBacklightFor(Entity newTarget)
        {
            if (newTarget == null)
            {
                _backlight.gameObject.SetActive(false);
                _currentTargetTransform = null;
                return;
            }

            _backlight.gameObject.SetActive(true);
            _currentTargetTransform = newTarget.Transform;
        }
    }
}
