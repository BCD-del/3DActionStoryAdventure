using UnityEngine;

namespace Assets._Project.Develop.Runtime.Configs.Gameplay.Moveables
{
    [CreateAssetMenu(menuName = "Configs/Gameplay/Moveables/New Moveables Config", fileName = "NPCConfig")]
    public class MoveablesConfig : ScriptableObject
    {
        [field: SerializeField] public string PrefabPath { get; private set; }
    }
}
