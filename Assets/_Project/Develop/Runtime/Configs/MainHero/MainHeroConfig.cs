using UnityEngine;

namespace Assets._Project.Develop.Runtime.Configs.MainHero
{
    [CreateAssetMenu(menuName = "Configs/Gameplay/Entities/New Main Hero Config", fileName = "MainHeroConfig")]
    public class MainHeroConfig : ScriptableObject
    {
        [field: SerializeField] public string PrefabPath { get; private set; }
        [field: SerializeField] public float MaxHealth { get; private set; } = 100f;
        [field: SerializeField] public float MoveSpeed { get; private set; } = 10f;
        [field: SerializeField] public float SprintMoveSpeedMultiplier { get; private set; } = 5;
    }
}
