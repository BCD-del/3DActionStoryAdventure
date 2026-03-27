using Assets._Project.Develop.Runtime.Configs.Gameplay.Abilities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.CourseGame.Develop.Configs.Gameplay.Abilities
{
    [CreateAssetMenu(menuName = "Configs/Gameplay/Abilities/AdditionalDirectionsShotAbilityConfig", fileName = "AdditionalDirectionsShotAbilityConfig")]
    public class AdditionalDirectionsShotAbilityConfig : AbilityConfig
    {
        [SerializeField] private List<Config> _additionalArrowsByLevel;
        public override int MaxLevel => _additionalArrowsByLevel.Count;

        public List<DirectionShotConfig> GetBy(int level) => _additionalArrowsByLevel[level - 1].DirectionShotConfigs;


        [Serializable]
        private class Config
        {
            [field: SerializeField] public List<DirectionShotConfig> DirectionShotConfigs { get; private set; }
        }
    }

    [Serializable]
    public class DirectionShotConfig
    {
        [field: SerializeField] public int Angel { get; private set; }
        [field: SerializeField] public int NumberOfProjectiles { get; private set; }
    }
}