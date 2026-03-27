using Assets._Project.Develop.Runtime.Configs.Gameplay.Abilities;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using System.Linq;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.AbilitiesDroppingFeature
{
    public class AbilityDropingRulesService
    {
        public bool IsAvailable(AbilityConfig config, Entity entity, int abilityLevel)
        {
            if (config.IsUpgradable())
            {
                if (entity.Abilities.Elements.Any(ability =>
                ability.ID == config.ID
                && ability.CurrentLevel.Value + abilityLevel > ability.MaxLevel))
                {
                    return false;
                }
            }

            switch (config)
            {
                case StatChangeAbilityConfig statChangeAbilityConfig:
                    return entity.TryGetModifiedStats(out var modifiedStats)
                        && modifiedStats.ContainsKey(statChangeAbilityConfig.StatType);
            }

            return true;
        }
    }
}