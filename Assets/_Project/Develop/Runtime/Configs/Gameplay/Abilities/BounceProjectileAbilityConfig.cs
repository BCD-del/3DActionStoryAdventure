using System.Collections.Generic;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Configs.Gameplay.Abilities
{
    [CreateAssetMenu(menuName = "Configs/Gameplay/Abilities/BounceProjectileAbilityConfig", fileName = "BounceProjectileAbilityConfig")]
    public class BounceProjectileAbilityConfig : AbilityConfig
    {
        [SerializeField] private List<int> _bounceCountByLevel;

        [field: SerializeField] public LayerMask LayerBounceRection { get; private set; }

        public override int MaxLevel => _bounceCountByLevel.Count;

        public int GetBounceCountBy(int level) => _bounceCountByLevel[level - 1];
    }
}
