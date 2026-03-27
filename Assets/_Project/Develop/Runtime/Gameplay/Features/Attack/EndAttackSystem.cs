using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Utilites.Reactive;
using System;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.Attack
{
    public class EndAttackSystem : IInitializableSystem, IDisposable
    {
        private ReactiveEvent _endAttackEvent;
        private ReactiveVariable<bool> _inAttackProcess;
        private ReactiveVariable<float> _attackProcessInitialTime;
        private ReactiveVariable<float> _attackProcessCurrentTime;

        private IDisposable _timerDisposable;

        public void OnInit(Entity entity)
        {
            _endAttackEvent = entity.EndAttackEvent;
            _inAttackProcess = entity.InAttackProcess;
            _attackProcessInitialTime = entity.AttackProcessModifiedTime;
            _attackProcessCurrentTime = entity.AttackProcessCurrentTime;

            _timerDisposable = _attackProcessCurrentTime.Subscribe(OnTimerChanged);
        }

        public void Dispose()
        {
            _timerDisposable.Dispose();
        }

        private void OnTimerChanged(float arg1, float currentTime)
        {
            if (TimerIsDone(currentTime))
            {
                // Debug.Log("End of attack");
                _inAttackProcess.Value = false;
                _endAttackEvent.Invoke();
            }
        }

        private bool TimerIsDone(float currentTime) => currentTime >= _attackProcessInitialTime.Value;
    }
}
