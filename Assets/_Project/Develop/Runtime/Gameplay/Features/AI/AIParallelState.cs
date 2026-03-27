using Assets._Project.Develop.Runtime.Utilites.StateMachineCore;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.AI
{
    public class AIParallelState : ParallelState<IUpdatableState>, IUpdatableState
    {
        public AIParallelState(params IUpdatableState[] states) : base(states)
        {
            
        }

        public void Update(float deltaTime)
        {
            foreach (IUpdatableState state in States)
                state.Update(deltaTime);
        }
    }
}
