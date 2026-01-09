using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Configs
{
    [CreateAssetMenu(fileName = "PlayerConfig", menuName = "Configs/Characters/Player", order = 54)]
    public class PlayerConfig : ScriptableObject
    {
        [field: SerializeField] public float MovementSpeed { get; private set; } = 5;
        [field: SerializeField] public float RotationSpeed { get; private set; } = 90;
        [field: SerializeField] public float JumpPower { get; private set; } = 10;
        [field: SerializeField] public float LegsRange { get; private set; } = 0.25f;
        [field: SerializeField] public LayerMask JumpableMask { get; private set; }
        [field: SerializeField] public float Gravity { get; private set; } = 9.8f;
    }
}
