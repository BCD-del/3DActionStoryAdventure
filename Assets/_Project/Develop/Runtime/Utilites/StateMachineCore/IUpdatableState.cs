namespace Assets._Project.Develop.Runtime.Utilites.StateMachineCore
{
    public interface IUpdatableState : IState
    {
        void Update(float deltaTime);
    }
}