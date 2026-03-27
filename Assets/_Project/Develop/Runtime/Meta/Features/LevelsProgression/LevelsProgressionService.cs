using Assets._Project.Develop.Runtime.Utilites.DataManagment;
using Assets._Project.Develop.Runtime.Utilites.DataManagment.DataProviders;
using System.Collections.Generic;

namespace Assets._Project.Develop.Runtime.Meta.Features.LevelsProgression
{
    public class LevelsProgressionService : IDataReader<PlayerData>, IDataWriter<PlayerData>
    {
        private const int FirstLevel = 1;

        private readonly List<int> _completedLevels = new();

        public LevelsProgressionService(PlayerDataProvider playerDataProvider)
        {
            playerDataProvider.RegisterReader(this);
            playerDataProvider.RegisterWriter(this);
        }

        public void AddLevelToCompleted(int levelNumber)
        {
            if (IsLevelCompleted(levelNumber))
                return;

            _completedLevels.Add(levelNumber);
        }

        public void ReadFrom(PlayerData data)
        {
            _completedLevels.Clear();
            _completedLevels.AddRange(data.CompletedLevels);
        }

        public void WriteTo(PlayerData data)
        {
            data.CompletedLevels.Clear();
            data.CompletedLevels.AddRange(_completedLevels);
        }

        public bool IsLevelCompleted(int levelNumber) => _completedLevels.Contains(levelNumber);

        public bool CanPlay(int levelNumber) => levelNumber == FirstLevel || IsPreviousLevelCompleted(levelNumber);

        private bool IsPreviousLevelCompleted(int levelNumber) => IsLevelCompleted(levelNumber - 1);
    }
}
