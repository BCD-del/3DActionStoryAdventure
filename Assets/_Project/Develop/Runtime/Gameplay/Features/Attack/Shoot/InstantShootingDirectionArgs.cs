using System.Collections.Generic;
using System.Linq;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.Attack.Shoot
{
    public class InstantShootingDirectionArgs
    {
        private List<InstantShotDirectionArgs> _args;

        public InstantShootingDirectionArgs(params InstantShotDirectionArgs[] args)
        {
            _args = new List<InstantShotDirectionArgs>(args);
        }

        public IReadOnlyList<InstantShotDirectionArgs> Args => _args;

        public void Add(InstantShotDirectionArgs shotInDeirectionArgs)
        {
            var arg = _args.FirstOrDefault(ar => ar.Angel == shotInDeirectionArgs.Angel);

            if (arg != null)
            {
                arg.ProjectileCounts += shotInDeirectionArgs.ProjectileCounts;
                return;
            }

            _args.Add(shotInDeirectionArgs);
        }

        public void Remove(InstantShotDirectionArgs shotInDeirectionArgs)
        {
            var arg = _args.FirstOrDefault(ar => ar.Angel == shotInDeirectionArgs.Angel);

            if (arg != null)
            {
                arg.ProjectileCounts -= shotInDeirectionArgs.ProjectileCounts;

                if (arg.ProjectileCounts <= 0)
                    _args.Remove(shotInDeirectionArgs);
            }
        }
    }
}
