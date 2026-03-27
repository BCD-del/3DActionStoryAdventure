using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Utilites.Reactive;
using System;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.StatsFeature
{
    public class AttackTimeByAttackSpeedStatSyncronizerSystem : IInitializableSystem, IDisposableSystem
    {
        private ReactiveVariable<float> _attackPerSecond;

        private ReactiveVariable<float> _attackProcessInitialTime;
        private ReactiveVariable<float> _attackProcessModifiedTime;
        private ReactiveVariable<float> _attackCooldownInitialTime;
        private ReactiveVariable<float> _attackCooldownModifiedTime;
        private ReactiveVariable<float> _attackDelayInitialTime;
        private ReactiveVariable<float> _attackDelayModifiedTime;

        private IDisposable _attackPerSecondChangedDisposable;

        public void OnDispose()
        {
            _attackPerSecondChangedDisposable.Dispose();
        }

        public void OnInit(Entity entity)
        {
            _attackPerSecond = entity.AttacksPerSecond;
            _attackProcessInitialTime = entity.AttackProcessInitialTime;
            _attackProcessModifiedTime = entity.AttackProcessModifiedTime;
            _attackCooldownInitialTime = entity.AttackCooldownInitialTime;
            _attackCooldownModifiedTime = entity.AttackCooldownModifiedTime;
            _attackDelayInitialTime = entity.AttackDelayTime;
            _attackDelayModifiedTime = entity.AttackDelayModifiedTime;

            _attackPerSecondChangedDisposable = _attackPerSecond.Subscribe(OnAttackPerSecondChanged);
            OnAttackPerSecondChanged(0, _attackPerSecond.Value);
        }

        private void OnAttackPerSecondChanged(float arg1, float newAttackPerSecond)
        {
            float totalBaseTime = _attackProcessInitialTime.Value + _attackCooldownInitialTime.Value;

            float targetTotalTime = 1f / newAttackPerSecond;

            float totalTimeRatio = targetTotalTime / totalBaseTime;

            _attackProcessModifiedTime.Value = _attackProcessInitialTime.Value * totalTimeRatio;
            _attackCooldownModifiedTime.Value = _attackCooldownInitialTime.Value * totalTimeRatio;
            _attackDelayModifiedTime.Value = _attackDelayInitialTime.Value * totalTimeRatio;
        }
    }
}