using Assets._Project.Develop.Runtime.Gameplay.Features.StatsFeature;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.AbilitiesFeature
{
    public class AbilityOnAddActivatorSystem : IInitializableSystem, IDisposableSystem
    {
        private AbilitiesList _abilitiesList;

        public void OnInit(Entity entity)
        {
            _abilitiesList = entity.Abilities;

            _abilitiesList.Added += OnAbilityAdded;

            foreach (Ability ability in _abilitiesList.Elements)
                ability.Activate();
        }

        private void OnAbilityAdded(Ability ability)
        {
            ability.Activate();
        }

        public void OnDispose()
        {
            _abilitiesList.Added -= OnAbilityAdded;
        }
    }
}