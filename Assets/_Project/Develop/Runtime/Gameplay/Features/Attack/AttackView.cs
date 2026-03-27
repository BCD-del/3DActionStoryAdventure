using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Mono;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Utilites.Reactive;
using System;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.Attack
{
    [RequireComponent(typeof(Animator))]
    public class AttackView : EntityView
    {
        private readonly int IsAttackingKey = Animator.StringToHash("IsAttacking");

        [SerializeField] private Animator _animator;

        private IReadOnlyVariable<bool> _inAttackProcess;

        private IDisposable _inAttackProcessChangedDisposable;

        private void OnValidate()
        {
            _animator ??= GetComponent<Animator>();
        }

        protected override void OnEntityStartedWork(Entity entity)
        {
            _inAttackProcess = entity.InAttackProcess;

            _inAttackProcessChangedDisposable = _inAttackProcess.Subscribe(OninAttackProcessChanged);
            UpdateInAttackProcess(_inAttackProcess.Value);
        }

        public override void Cleanup(Entity entity)
        {
            base.Cleanup(entity);

            _inAttackProcessChangedDisposable.Dispose();
        }

        private void UpdateInAttackProcess(bool value) => _animator.SetBool(IsAttackingKey, value);

        private void OninAttackProcessChanged(bool oldIsDead, bool isDead) => UpdateInAttackProcess(isDead);
    }
}
