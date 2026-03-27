using UnityEngine;

namespace Assets._Project.Develop.Runtime.Configs.Gameplay.Loot
{
    [CreateAssetMenu(menuName = "Configs/Gameplay/Loot/ExperienceLootConfig", fileName = "ExperienceLootConfig")]
    public class ExperienceLootConfig : LootConfig
    {
        [field: SerializeField] public int Experience { get; private set; }
    }
}