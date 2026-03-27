using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Mono;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Utilites.Reactive;
using UnityEngine;
using System;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.ApplyDamage
{
    public class ApplyDamageView : EntityView
    {
        [SerializeField] private ParticleSystem _applyDamageEffectPrefab;
        [SerializeField] private Transform _effectSpawnPoint;
        [SerializeField] private ParticleSystemStopAction _vfxStopAction;

        private ReactiveEvent<float> _damageEvent;

        private IDisposable _damageEventDisposable;

        protected override void OnEntityStartedWork(Entity entity)
        {
            _damageEvent = entity.TakeDamageEvent;
            _damageEventDisposable = _damageEvent.Subscribe(OnDamaged);
        }


        public override void Cleanup(Entity entity)
        {
            base.Cleanup(entity);

            _damageEventDisposable.Dispose();
        }

        private void OnDamaged(float value)
        {
            ParticleSystem vfx = Instantiate(_applyDamageEffectPrefab, _effectSpawnPoint.position, Quaternion.identity);

            ParticleSystem.MainModule main = vfx.main;
            main.stopAction = _vfxStopAction;
        }
    }
}
