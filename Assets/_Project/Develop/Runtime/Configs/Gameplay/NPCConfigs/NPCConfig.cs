using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Assets._Project.Develop.NPCConfigsGameplay.NPCConfigs
{
    [CreateAssetMenu(menuName = "Configs/Gameplay/Npc/New NPC Config", fileName = "NPCConfig")]
    public class NPCConfig : ScriptableObject
    {
        [field:SerializeField] public string PrefabPath { get; private set; }
    }
}
