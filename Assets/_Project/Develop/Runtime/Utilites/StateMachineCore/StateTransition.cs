using Assets._Project.Develop.Runtime.Utilites.Conditions;

namespace Assets._Project.Develop.Runtime.Utilites.StateMachineCore
{
    public class StateTransition<TState> where TState : class, IState
    {
        public StateTransition(StateNode<TState> toState, ICondition condition)
        {
            ToState = toState;
            Condition = condition;
        }

        public StateNode<TState> ToState { get; }
        public ICondition Condition { get; }
    }
}
