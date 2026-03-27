using System;

namespace Assets._Project.Develop.Runtime.Utilites.Reactive
{
    public interface IReadOnlyVariable<T>
    {
        T Value { get; }

        IDisposable Subscribe(Action<T, T> action);
    }
}