using Assets._Project.Develop.Runtime.Utilites.Reactive;

namespace Assets._Project.Develop.Runtime.Utilites.StateMachineCore
{
    public abstract class State : IState
    {
        private ReactiveEvent _entered = new();
        private ReactiveEvent _exited = new();

        public IReadOnlyEvent Entered => _entered;
        public IReadOnlyEvent Exited => _exited;

        public virtual void Enter() => _entered.Invoke();
        public virtual void Exit() => _exited.Invoke();   
    }
}
