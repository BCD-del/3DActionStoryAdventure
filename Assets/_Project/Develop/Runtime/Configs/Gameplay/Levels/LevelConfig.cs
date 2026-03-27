using Assets._Project.Develop.Runtime.Configs.Gameplay.Stages;
using System.Collections.Generic;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Configs.Gameplay.Levels
{
    [CreateAssetMenu(menuName = "Configs/Gameplay/Levels/New Level Config", fileName = "LevelConfig", order = 54)]
    public class LevelConfig : ScriptableObject
    {
        [SerializeField] private List<StageConfig> _stageConfigs;

        [field: SerializeField] public string LevelName { get; private set; }
        [field: SerializeField] public int LevelNumber { get; private set; }
        [field: SerializeField] public Sprite LevelIcon { get; private set; }
        [field: SerializeField] public Vector3 ContactTriggerSpawnPointPosition { get; private set; }

        public IReadOnlyList<StageConfig> StageConfigs => _stageConfigs;

    }
}
