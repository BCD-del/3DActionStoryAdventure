using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Mono;
using Assets._Project.Develop.Runtime.Utilites.Reactive;
using System;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.Attack
{
    [RequireComponent(typeof(Animator))]
    public class AttackAnimationSpeedView : EntityView
    {
        private readonly int _attackAnimationSpeedMultiplierKey = Animator.StringToHash("AttackAnimationSpeedMultiplier");

        [SerializeField] private Animator _animator;

        private ReactiveVariable<float> _attackProcessInitialTime;
        private ReactiveVariable<float> _attackProcessModifiedTime;

        private IDisposable _attackProcessTimeChangedDisposable;

        private void OnValidate()
        {
            _animator ??= GetComponent<Animator>();
        }

        protected override void OnEntityStartedWork(Entity entity)
        {
            _attackProcessInitialTime = entity.AttackProcessInitialTime;
            _attackProcessModifiedTime = entity.AttackProcessModifiedTime;

            _attackProcessTimeChangedDisposable = _attackProcessModifiedTime.Subscribe(OnAttackProcessTimeChanged);
            OnAttackProcessTimeChanged(0, _attackProcessModifiedTime.Value);
        }

        public override void Cleanup(Entity entity)
        {
            base.Cleanup(entity);

            _attackProcessTimeChangedDisposable.Dispose();
        }

        private void OnAttackProcessTimeChanged(float arg1, float currentAttackProcessTime)
        {
            _animator.SetFloat(_attackAnimationSpeedMultiplierKey, _attackProcessInitialTime.Value / currentAttackProcessTime);
        }
    }
}