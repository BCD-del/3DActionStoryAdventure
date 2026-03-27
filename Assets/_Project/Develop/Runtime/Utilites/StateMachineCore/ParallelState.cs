using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets._Project.Develop.Runtime.Utilites.StateMachineCore
{
    public abstract class ParallelState<TState> : State where TState : class, IState
    {
        private List<TState> _states;

        public ParallelState(params TState[] states)
        {
            _states = new List<TState>(states);
        }

        protected IReadOnlyList<TState> States => _states;

        public override void Enter()
        {
            base.Enter();

            foreach (TState state in _states)
                state.Enter();
        }

        public override void Exit()
        {
            base.Exit();

            foreach (TState state in _states)
                state.Exit();
        }
    }
}
