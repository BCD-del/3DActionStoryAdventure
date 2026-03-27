using System;
using System.Collections;
using System.Collections.Generic;

namespace Assets._Project.Develop.Runtime.Utilites.DataManagment.DataProviders
{
    public abstract class DataProvider<TData> where TData : ISaveData
    {
        private readonly ISaveLoadService _saveLoadService;

        private readonly List<IDataWriter<TData>> _writers = new();
        private readonly List<IDataReader<TData>> _readers = new();
         
        private TData _data;

        protected DataProvider(ISaveLoadService saveLoadService)
        {
            _saveLoadService = saveLoadService;
        }

        public void RegisterWriter(IDataWriter<TData> writer)
        {
            if (_writers.Contains(writer))
                throw new ArgumentException(nameof(writer) + " already registred");

            _writers.Add(writer);
        }

        public void RegisterReader(IDataReader<TData> reader)
        {
            if (_readers.Contains(reader))
                throw new ArgumentException(nameof(reader) + " already registred");

            _readers.Add(reader);
        }

        public IEnumerator LoadAsync()
        {
            yield return _saveLoadService.Load<TData>(loadedData => _data = loadedData);
            SendDataToReaders();
        }

        public IEnumerator SaveAsync()
        {
            UpdateDataForWriters();

            yield return _saveLoadService.Save(_data);
        }

        public IEnumerator ExistsAsync(Action<bool> onExistsResult)
        {
            yield return _saveLoadService.Exists<TData>(result => onExistsResult?.Invoke(result));
        }

        public void Reset()
        {
            _data = GetOriginData();
            SendDataToReaders();
        }

        protected abstract TData GetOriginData();

        private void SendDataToReaders()
        {
            foreach (IDataReader<TData> reader in _readers)
            {
                reader.ReadFrom(_data);
            }
        }

        private void UpdateDataForWriters()
        {
            foreach (IDataWriter<TData> writer in _writers)
            {
                writer.WriteTo(_data);
            }
        }
    }
}
