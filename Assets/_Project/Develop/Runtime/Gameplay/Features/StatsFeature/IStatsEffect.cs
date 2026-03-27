using System.Collections.Generic;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.StatsFeature
{
    public interface IStatsEffect
    {
        void ApplyTo(Dictionary<StatTypes, float> stats);
    }
}
