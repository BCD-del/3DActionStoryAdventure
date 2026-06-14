using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Utilites.Reactive;
using System;
using UnityEngine;


namespace Assets._Project.Develop.Runtime.Gameplay.Features.Attack.MeleeAttack
{
    public class AttackSystem : IInitializableSystem, IUpdatableSystem
    {
        private ReactiveVariable<float> _attackStrenght;

        public void OnInit(Entity entity)
        {
            _attackStrenght = entity.AttackStrenght;
        }

        public void OnUpdate(float deltaTime)
        {
            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                Debug.Log($"Hit: {_attackStrenght}");
            }
        }
    }
}
