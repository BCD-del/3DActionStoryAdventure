using System;
using System.Collections.Generic;

namespace Assets._Project.Develop.Runtime.Utilites.DataManagment.KeyStorage
{
    public class MapDataKeysStorage : IDataKeysStorage
    {
        private readonly Dictionary<Type, string> Keys = new Dictionary<Type, string>()
        {
            {typeof(PlayerData), nameof(PlayerData) },
        };

        public string GetKeyFor<Tdata>() where Tdata : ISaveData
        {
            return Keys[typeof(Tdata)];
        }
    }
}
