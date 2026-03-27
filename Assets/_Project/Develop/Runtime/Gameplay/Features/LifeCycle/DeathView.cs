using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Mono;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Utilites.Reactive;
using System;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.LifeCycle
{
    [RequireComponent(typeof(Animator))]
    public class DeathView : EntityView
    {
        private readonly int IsDyingKey = Animator.StringToHash("IsDying");

        [SerializeField] private Animator _animator;

        private IReadOnlyVariable<bool> _isDead;

        private IDisposable _isDeadChangedDisposable;

        private void OnValidate()
        {
            _animator ??= GetComponent<Animator>();
        }

        protected override void OnEntityStartedWork(Entity entity)
        {
            _isDead = entity.IsDead;

            _isDeadChangedDisposable = _isDead.Subscribe(OnIsDeadChanged);
            UpdateIsDead(_isDead.Value);
        }

        public override void Cleanup(Entity entity)
        {
            base.Cleanup(entity);

            _isDeadChangedDisposable.Dispose();
        }

        private void UpdateIsDead(bool value) => _animator.SetBool(IsDyingKey, value);

        private void OnIsDeadChanged(bool oldIsDead, bool isDead) => UpdateIsDead(isDead);
    }
}
