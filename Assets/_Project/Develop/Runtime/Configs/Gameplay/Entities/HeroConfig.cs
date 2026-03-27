using UnityEngine;

namespace Assets._Project.Develop.Runtime.Configs.Gameplay.Entities
{
    [CreateAssetMenu(fileName = "HeroConfig", menuName = "Configs/Gameplay/Entities/New Hero Config")]
    public class HeroConfig : EntityConfig
    {
        [field: SerializeField] public string PrefabPath { get; private set; } = "Entities/Hero";

        [Header("Movement Settings")]
        [field: SerializeField, Min(0)] public float MoveSpeed { get; private set; } = 10;
        [field: SerializeField, Min(0)] public float RotationSpeed { get; private set; } = 900;

        [Header("Attack Settings")]
        [field: SerializeField, Min(0)] public float AttackProcessTime { get; private set; } = 1.5f;
        [field: SerializeField, Min(0)] public float AttackDelayTime { get; private set; } = 0.75f;
        [field: SerializeField, Min(0)] public float AttackCooldown { get; private set; } = 1f;
        [field: SerializeField, Min(0)] public float InstantAttackDamage { get; private set; } = 50;

        [Header("Life Cycle Settings")]
        [field: SerializeField, Min(0)] public float MaxHealth { get; private set; } = 100;
        [field: SerializeField, Min(0)] public float DeathProcessTime { get; private set; } = 2;
        [field: SerializeField, Min(0)] public float SpawnProcessTime { get; private set; } = 2;
        [field: SerializeField, Min(0)] public float AttackSpeedMultiplier { get; private set; } = 1;

        [SerializeField, HideInInspector] private float _lastAttackProcessTime;
        [SerializeField, HideInInspector] private float _lastAttackDelayTime;

        private void OnValidate()
        {
            if (_lastAttackProcessTime <= 0)
            {
                _lastAttackProcessTime = AttackProcessTime;
                _lastAttackDelayTime = AttackDelayTime;
                return;
            }

            if (!Mathf.Approximately(AttackProcessTime, _lastAttackProcessTime))
            {
                if (_lastAttackProcessTime > 0)
                {
                    float ratio = _lastAttackDelayTime / _lastAttackProcessTime;
                    AttackDelayTime = AttackProcessTime * ratio;
                }

                _lastAttackProcessTime = AttackProcessTime;
                _lastAttackDelayTime = AttackDelayTime;
            }

            else if (!Mathf.Approximately(AttackDelayTime, _lastAttackDelayTime))
            {
                _lastAttackDelayTime = AttackDelayTime;
                _lastAttackProcessTime = AttackProcessTime;
            }
        }
    }
}