using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Configs.Gameplay.Abilities
{
    [CreateAssetMenu(menuName = "Configs/Gameplay/Abilities/AbilitiesConfigsContainer", fileName = "AbilitiesConfigsContainer")]
    public class AbilitiesConfigsContainer : ScriptableObject
    {
        [SerializeField] private List<AbilityConfig> _abilityConfigs;

        public IReadOnlyList<AbilityConfig> AbilityConfigs => _abilityConfigs;

        public AbilityConfig GetConfigBy(string ID) => _abilityConfigs.First(config => config.ID == ID);
    }
}