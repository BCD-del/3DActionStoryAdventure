using System;

namespace Assets._Project.Develop.Runtime.Utilites.Reactive
{
    public interface IReadOnlyEvent
    {
        IDisposable Subscribe(Action action);
    }

    public interface IReadOnlyEvent<T>
    {
        IDisposable Subscribe(Action<T> action);
    }
}