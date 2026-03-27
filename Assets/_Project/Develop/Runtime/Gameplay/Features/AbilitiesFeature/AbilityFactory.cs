using Assets._Project.Develop.Infrastructure.DI;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Abilities;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.AbilitiesFeature.Abilities;
using Assets.CourseGame.Develop.Configs.Gameplay.Abilities;
using System;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.AbilitiesFeature
{
    public class AbilityFactory
    {
        private DIContainer _container;

        public AbilityFactory(DIContainer container)
        {
            _container = container;
        }

        public Ability CreateAbilityFor(Entity entity, AbilityConfig config, int currentLevel)
        {
            switch (config)
            {
                case StatChangeAbilityConfig changeAbilityConfig:
                    return new StatChangeAbility(entity, changeAbilityConfig, currentLevel);


                case AdditionalDirectionsShotAbilityConfig additionalDirectionsShotAbilityConfig:
                    return new AdditionalDirectionsShotAbility(additionalDirectionsShotAbilityConfig, entity, currentLevel);

                case BounceProjectileAbilityConfig bounceProjectileAbilityConfig:
                    return new BounceProjectileAbility(
                        bounceProjectileAbilityConfig,
                        entity,
                        _container.Resolve<EntitiesLifeContext>(),
                        currentLevel);

                default:
                    throw new ArgumentException();
            }
        }
    }
}