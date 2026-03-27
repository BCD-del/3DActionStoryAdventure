using Assets._Project.Develop.Runtime.Gameplay.Features.StatsFeature;
using System;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Configs.Gameplay.Abilities
{
    [CreateAssetMenu(menuName = "Configs/Gameplay/Abilities/StatChangeAbilityConfig", fileName = "StatChangeAbilityConfig")]
    public class StatChangeAbilityConfig : AbilityConfig
    {
        [field: SerializeField] public StatTypes StatType { get; private set; }

        public override int MaxLevel => 1;

        [SerializeField] private StatChangeOperation _operation;
        [SerializeField] private float _value;

        public Func<float, float> GetApplyEffect()
        {
            switch (_operation)
            {
                case StatChangeOperation.Add:
                    return stat => stat += _value;

                case StatChangeOperation.Multiply:
                    return stat => stat *= _value;

                default:
                    throw new InvalidOperationException();
            }
        }

        private enum StatChangeOperation
        {
            Multiply,
            Add
        }
    }
}
