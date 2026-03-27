using System;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.Attack.Shoot
{
    public class InstantShotDirectionArgs
    {
        private int _angel;
        private int _projectileCounts;

        public InstantShotDirectionArgs(int angel, int projectileCounts)
        {
            _angel = angel;
            _projectileCounts = projectileCounts;
        }

        public int Angel => _angel;
        public int ProjectileCounts
        {
            get => _projectileCounts;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value));

                _projectileCounts = value;
            }
        }
    }
}
