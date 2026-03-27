using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.Attack.Shoot;
using Assets.CourseGame.Develop.Configs.Gameplay.Abilities;
using System;
using System.Collections.Generic;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.AbilitiesFeature.Abilities
{
    public class AdditionalDirectionsShotAbility : Ability, IDisposable
    {
        private AdditionalDirectionsShotAbilityConfig _config;
        private Entity _entity;

        private IDisposable _currentLevelChangedDisposable;

        public AdditionalDirectionsShotAbility(
            AdditionalDirectionsShotAbilityConfig config,
            Entity entity,
            int currentLevel) : base(config.ID, currentLevel, config.MaxLevel)
        {
            _config = config;
            _entity = entity;
        }

        public override void Activate()
        {
            for (int i = 0; i < CurrentLevel.Value; i++)
            {
                AddShotDirectionsBy(i + 1);
            }

            _currentLevelChangedDisposable = CurrentLevel.Subscribe(OnCurrentLevelChaged);
        }

        private void OnCurrentLevelChaged(int previousLevel, int newLevel)
        {
            for (int i = previousLevel; i < newLevel; i++)
            {
                AddShotDirectionsBy(i + 1);
            }
        }

        private void AddShotDirectionsBy(int level)
        {
            List<DirectionShotConfig> directionShotConfigs = _config.GetBy(level);

            InstantShootingDirectionArgs shootingArgs = _entity.InstantShootingDirection;

            foreach (var directionShotConfig in directionShotConfigs)
            {
                shootingArgs.Add(new InstantShotDirectionArgs(directionShotConfig.Angel, directionShotConfig.NumberOfProjectiles));
            }
        }

        public void Dispose()
        {
            _currentLevelChangedDisposable.Dispose();
        }
    }
}