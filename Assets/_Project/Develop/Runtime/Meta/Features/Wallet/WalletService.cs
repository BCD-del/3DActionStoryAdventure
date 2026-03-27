using Assets._Project.Develop.Runtime.Utilites.DataManagment;
using Assets._Project.Develop.Runtime.Utilites.DataManagment.DataProviders;
using Assets._Project.Develop.Runtime.Utilites.Reactive;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Meta.Features.Wallet
{
    public class WalletService : IDataReader<PlayerData>, IDataWriter<PlayerData>
    {
        private readonly Dictionary<CurrencyTypes, ReactiveVariable<int>> _currencies;

        public WalletService(Dictionary<CurrencyTypes, 
            ReactiveVariable<int>> currencies,
            PlayerDataProvider playerDataProvider)
        {
            _currencies = new Dictionary<CurrencyTypes, ReactiveVariable<int>>(currencies);

            playerDataProvider.RegisterWriter(this);
            playerDataProvider.RegisterReader(this);
        }


        public List<CurrencyTypes> AvailableCurrencies => _currencies.Keys.ToList();

        public IReadOnlyVariable<int> GetCurrency(CurrencyTypes type) => _currencies[type];

        public bool IsEnough(CurrencyTypes type, int amount)
        {
            if (amount < 0)
                throw new ArgumentOutOfRangeException("amount can't be less than zero");

            return _currencies[type].Value >= amount;
        }

        public void Add(CurrencyTypes type, int amount)
        {
            if (amount < 0)
                throw new ArgumentOutOfRangeException("amount can't be less than zero");

            _currencies[type].Value += amount;
        }

        public void Spend(CurrencyTypes type, int amount)
        {
            if (IsEnough(type, amount) == false)
                throw new InvalidOperationException($"not enough {type} currency");

            if (amount < 0)
                throw new ArgumentOutOfRangeException("amount can't be less than zero");

            _currencies[type].Value -= amount;
            Debug.Log($"Spend {amount} of {type}, left: {_currencies[type].Value}");
        }

        public void ReadFrom(PlayerData data)
        {
            foreach (KeyValuePair<CurrencyTypes, int> currency in data.WalletData)
            {
                if (_currencies.ContainsKey(currency.Key))
                    _currencies[currency.Key].Value = currency.Value;
                else
                    _currencies.Add(currency.Key, new ReactiveVariable<int>(currency.Value));
            }
        }

        public void WriteTo(PlayerData data)
        {
            foreach (KeyValuePair<CurrencyTypes, ReactiveVariable<int>> currency in _currencies)
            {
                if (data.WalletData.ContainsKey(currency.Key))
                    data.WalletData[currency.Key] = currency.Value.Value;
                else
                    data.WalletData.Add(currency.Key, currency.Value.Value);
            }
        }
    }
}
