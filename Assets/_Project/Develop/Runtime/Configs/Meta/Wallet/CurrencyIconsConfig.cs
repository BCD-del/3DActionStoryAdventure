using Assets._Project.Develop.Runtime.Meta.Features.Wallet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Configs.Meta.Wallet
{
    [CreateAssetMenu(menuName = "Configs/Wallet/New Currency Icons Config", fileName = "CurrencyIconsConfig", order = 54)]
    public class CurrencyIconsConfig : ScriptableObject
    {
        [SerializeField] private List<CurrencyConfig> _configs;

        public string GetSpriteNameFor(CurrencyTypes currencyType)
            => _configs.First(config => config.Type == currencyType).SpriteName;

        public int GetSpriteIndexFor(CurrencyTypes currencyType)
            => _configs.First(config => config.Type == currencyType).SpriteIndex;

        [Serializable]
        private class CurrencyConfig
        {
            [field: SerializeField] public CurrencyTypes Type { get; private set; }
            [field: SerializeField] public Sprite Sprite { get; private set; }
            [field: SerializeField] public int SpriteIndex { get; private set; }
            [field: SerializeField] public string SpriteName { get; private set; }
        }
    }
}