using System;
using System.Collections.Generic;

namespace Assets._Project.Develop.Runtime.Utilites.Reactive
{
    public class ReactiveVariable<T> : IReadOnlyVariable<T>
    {
        private readonly List<Subscriber<T, T>> _subscribers = new();
        private readonly List<Subscriber<T, T>> _toAdd = new();
        private readonly List<Subscriber<T, T>> _toRemove = new();

        private T _value;
        private IEqualityComparer<T> _comparer;

        public ReactiveVariable() : this(default)
        { 
        }

        public ReactiveVariable(T value) : this(value, EqualityComparer<T>.Default)
        {
        }

        public ReactiveVariable(T value, IEqualityComparer<T> comparer)
        {
            _value = value;
            _comparer = comparer;
        }

        public T Value
        {
            get => _value;
            set
            {
                T oldValue = _value;
                _value = value;

                if (_comparer.Equals(oldValue, value) == false)
                    Invoke(oldValue, value);
            }
        }

        public IDisposable Subscribe(Action<T, T> action)
        {
            Subscriber<T, T> subscriber = new Subscriber<T, T>(action, Remove);
            _toAdd.Add(subscriber);

            return subscriber;
        }

        private void Remove(Subscriber<T, T> subscriber) => _toRemove.Add(subscriber);

        private void Invoke(T oldValue, T newValue)
        {
            if (_toAdd.Count > 0)
            {
                _subscribers.AddRange(_toAdd);
                _toAdd.Clear();
            }

            if (_toRemove.Count > 0)
            {
                foreach (Subscriber<T, T> subscriber in _toRemove)
                    _subscribers.Remove(subscriber);

                _toRemove.Clear();
            }

            foreach (Subscriber<T, T> subscriber in _subscribers)
                subscriber.Invoke(oldValue, newValue);
        }
    }
}
