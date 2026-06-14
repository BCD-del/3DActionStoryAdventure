using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.Attack.Shoot;
using Assets._Project.Develop.Runtime.Utilites.Conditions;
using Assets._Project.Develop.Runtime.Utilites.Reactive;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.Attack
{
    public class IsProjectile : IEntityComponent
    {
        
    }
   
    public class AttackStrenght : IEntityComponent
    {
        public ReactiveVariable<float> Value;
    }

    public class CooldownTime : IEntityComponent 
    {
        public ReactiveVariable<float> Value;
    }

    public class InstantShootingDirection : IEntityComponent
    {
        public InstantShootingDirectionArgs Value;
    }

    public class StartAttackRequest : IEntityComponent
    {
        public ReactiveEvent Value;
    }

    public class StartAttackEvent : IEntityComponent
    {
        public ReactiveEvent Value;
    }

    public class CanStartAttack : IEntityComponent
    {
        public ICompositeCondition Value;
    }

    public class EndAttackEvent : IEntityComponent
    {
        public ReactiveEvent Value;
    }

    public class AttackProcessInitialTime : IEntityComponent
    {
        public ReactiveVariable<float> Value;
    }

    public class AttackProcessModifiedTime : IEntityComponent
    {
        public ReactiveVariable<float> Value;
    }


    public class AttackProcessCurrentTime : IEntityComponent
    {
        public ReactiveVariable<float> Value;
    }

    public class InAttackProcess : IEntityComponent
    {
        public ReactiveVariable<bool> Value;
    }

    public class AttackRange : IEntityComponent
    {
        public ReactiveVariable<float> Value;
    }

    public class AttackDamage : IEntityComponent
    {
        public ReactiveVariable<float> Value;
    }

    public class AttackDelayTime : IEntityComponent
    {
        public ReactiveVariable<float> Value;
    }

    public class AttackDelayModifiedTime : IEntityComponent
    {
        public ReactiveVariable<float> Value;
    }


    public class AttackDelayEndEvent : IEntityComponent
    {
        public ReactiveEvent Value;
    }

    public class InstantAttackDamage : IEntityComponent
    {
        public ReactiveVariable<float> Value;
    }

    public class ShootPoint : IEntityComponent
    {
        public Transform Value;
    }

    public class MustCancelAttack : IEntityComponent
    {
        public ICompositeCondition Value;
    }

    public class AttackCanceledEvent : IEntityComponent
    {
        public ReactiveEvent Value;
    }

    public class AttackCooldownInitialTime : IEntityComponent
    {
        public ReactiveVariable<float> Value;
    }

    public class AttackCooldownModifiedTime : IEntityComponent
    {
        public ReactiveVariable<float> Value;
    }

    public class AttackCooldownCurrentTime : IEntityComponent
    {
        public ReactiveVariable<float> Value;
    }

    public class InAttackCooldown : IEntityComponent
    {
        public ReactiveVariable<bool> Value;
    }

    public class DamageApplyEvent: IEntityComponent
    {
        public ReactiveEvent Value;
    }
}
