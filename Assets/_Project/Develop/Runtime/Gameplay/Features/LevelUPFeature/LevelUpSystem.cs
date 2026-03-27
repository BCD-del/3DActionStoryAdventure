using Assets._Project.Develop.Runtime.Configs.Gameplay;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Utilites.Reactive;
using System;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.LevelUPFeature
{
    public class LevelUpSystem : IInitializableSystem, IDisposableSystem
    {
        private ReactiveVariable<float> _experience;
        private ReactiveVariable<int> _level;
        private ExperienceForUpgradeLevelConfig _config;

        private IDisposable _experienceChangedDisposable;

        public LevelUpSystem(ExperienceForUpgradeLevelConfig config)
        {
            _config = config;
        }

        public float CurrentLimitForExp => _config.GetExperienceFor(_level.Value);

        public void OnInit(Entity entity)
        {
            _experience = entity.Experience;
            _level = entity.Level;

            _experienceChangedDisposable = _experience.Subscribe(OnExperienceChanged);
        }

        private void OnExperienceChanged(float arg1, float newExp)
        {
            while (newExp >= CurrentLimitForExp && _level.Value < _config.MaxLevel)
            {
                newExp -= CurrentLimitForExp;
                _level.Value++;
            }

            _experience.Value = newExp;
        }

        public void OnDispose()
        {
            _experienceChangedDisposable.Dispose();
        }
    }
}