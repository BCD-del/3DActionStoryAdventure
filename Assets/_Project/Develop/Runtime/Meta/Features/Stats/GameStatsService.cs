using Assets._Project.Develop.Runtime.Utilites.DataManagment;
using Assets._Project.Develop.Runtime.Utilites.Reactive;
using Assets._Project.Develop.Runtime.Utilites.DataManagment.DataProviders;

namespace Assets._Project.Develop.Runtime.Meta.Features.Stats
{

    public class GameStatsService : IDataReader<PlayerData>, IDataWriter<PlayerData>
    {
        private readonly PlayerDataProvider _playerDataProvider;

        public ReactiveVariable<int> Wins { get; private set; } = new();
        public ReactiveVariable<int> Losses { get; private set; } = new();

        public GameStatsService(PlayerDataProvider playerDataProvider)
        {
            _playerDataProvider = playerDataProvider;

            _playerDataProvider.RegisterWriter(this);
            _playerDataProvider.RegisterReader(this);
        }

        public void RegisterVictory()
        {
            Wins.Value++;
        }

        public void RegisterDefeat()
        {
            Losses.Value++;
        }

        public void ReadFrom(PlayerData data)
        {
            Wins.Value = data.Wins;
            Losses.Value = data.Losses;
        }

        public void WriteTo(PlayerData data)
        {
            data.Wins = Wins.Value;
            data.Losses = Losses.Value;
        }

        public void ResetStats()
        {
            Wins.Value = 0;
            Losses.Value = 0;
        }
    }
}