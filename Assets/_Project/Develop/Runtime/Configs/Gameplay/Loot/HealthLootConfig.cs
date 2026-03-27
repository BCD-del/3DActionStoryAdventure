using UnityEngine;

namespace Assets._Project.Develop.Runtime.Configs.Gameplay.Loot
{
    [CreateAssetMenu(menuName = "Configs/Gameplay/Loot/HealthLootConfig", fileName = "HealthLootConfig")]
    public class HealthLootConfig : LootConfig
    {
        [field: SerializeField] public float Health { get; private set; }
    }
}