using Assets._Project.Develop.Runtime.Configs.Gameplay.Abilities;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.AbilitiesDroppingFeature
{
    public class AbilityDropOption
    {
        public AbilityDropOption(AbilityConfig config, int level)
        {
            Config = config;
            Level = level;
        }

        public AbilityConfig Config { get; }
        public int Level { get; }
    }
}