using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Configs.Gameplay.Camera
{
    [CreateAssetMenu(menuName = "Configs/Gameplay/CameraFeature/New Camera Config", fileName = "CameraConfig")]
    public class CameraConfig : ScriptableObject
    {
        [field: SerializeField] public string PrefabPath { get; private set; }

        [field: SerializeField] public Entity TargetedEntity { get; private set; }
    }
}
