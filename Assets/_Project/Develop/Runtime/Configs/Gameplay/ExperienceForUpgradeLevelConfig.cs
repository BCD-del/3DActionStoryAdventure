using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Configs.Gameplay
{
    [CreateAssetMenu(menuName = "Configs/Gameplay/ExperienceForUpgradeLevelConfig", fileName = "ExperienceForUpgradeLevelConfig")]
    public class ExperienceForUpgradeLevelConfig : ScriptableObject
    {
        [SerializeField] private List<float> _experienceForLevel;

        public int MaxLevel => _experienceForLevel.Count;
        public float GetExperienceFor(int level) => _experienceForLevel[level - 1];
    }
}