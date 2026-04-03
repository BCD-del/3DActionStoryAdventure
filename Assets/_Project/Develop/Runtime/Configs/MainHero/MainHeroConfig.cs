using UnityEngine;

namespace Assets._Project.Develop.Runtime.Configs.MainHero
{
    [CreateAssetMenu(menuName = "Configs/Main Hero/New Main Hero Config", fileName = "MainHeroConfig")]
    public class MainHeroConfig : ScriptableObject
    {
        [field: SerializeField] public string Name { get; private set; }
        [field: SerializeField] public string PrefabPath { get; private set; } 
    }
}
