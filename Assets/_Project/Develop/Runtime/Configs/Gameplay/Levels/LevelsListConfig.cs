using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Configs.Gameplay.Levels
{
    [CreateAssetMenu(menuName = "Configs/Gameplay/Levels/New Levels List Config", fileName = "LevelsListConfig", order = 54)]
    public class LevelsListConfig : ScriptableObject
    {
        [SerializeField] private List<LevelConfig> _levels;

        public IReadOnlyList<LevelConfig> Levels => _levels;

        public LevelConfig GetBy(int levelNumber)
        {
            int levelIndex = levelNumber - 1;

            return _levels[levelIndex];
        }
    }
}
