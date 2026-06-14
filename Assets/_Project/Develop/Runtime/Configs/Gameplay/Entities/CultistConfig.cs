using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Configs.Gameplay.Entities
{
    [CreateAssetMenu(fileName = "CultistConfig", menuName = "Configs/Gameplay/Entities/New Cultist Config")]
    public class CultistConfig : ScriptableObject
    {
        [field: SerializeField] public string PrefabPath { get; private set; }

        [field: SerializeField] public float MoveSpeed { get; private set; } = 5f;

        [field: SerializeField] public float MaxHealth { get; private set; } = 20f;

    }
}
