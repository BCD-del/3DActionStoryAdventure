using System;
using System.Collections.Generic;

namespace Assets._Project.Develop.Runtime.Utilites.Reactive
{
    public class ReactiveEvent<T> : IReadOnlyEvent<T>
    {
        private readonly List<Subscriber<T>> _subscribers = new();
        private readonly List<Subscriber<T>> _toAdd = new();
        private readonly List<Subscriber<T>> _toRemove = new();

        public IDisposable Subscribe(Action<T> action)
        {
            Subscriber<T> subscriber = new Subscriber<T>(action, Remove);
            _toAdd.Add(subscriber);

            return subscriber;
        }

        private void Remove(Subscriber<T> subscriber) => _toRemove.Add(subscriber);

        public void Invoke(T arg)
        {
            if (_toAdd.Count > 0)
            {
                _subscribers.AddRange(_toAdd);
                _toAdd.Clear();
            }

            if (_toRemove.Count > 0)
            {
                foreach (Subscriber<T> subscriber in _toRemove)
                    _subscribers.Remove(subscriber);

                _toRemove.Clear();
            }

            foreach (Subscriber<T> subscriber in _subscribers)
                subscriber.Invoke(arg);
        }
    }

    public class ReactiveEvent : IReadOnlyEvent
    {
        private readonly List<Subscriber> _subscribers = new();
        private readonly List<Subscriber> _toAdd = new();
        private readonly List<Subscriber> _toRemove = new();

        public IDisposable Subscribe(Action action)
        {
            Subscriber subscriber = new Subscriber(action, Remove);
            _toAdd.Add(subscriber);

            return subscriber;
        }

        private void Remove(Subscriber subscriber) => _toRemove.Add(subscriber);

        public void Invoke()
        {
            if (_toAdd.Count > 0)
            {
                _subscribers.AddRange(_toAdd);
                _toAdd.Clear();
            }

            if (_toRemove.Count > 0)
            {
                foreach (Subscriber subscriber in _toRemove)
                    _subscribers.Remove(subscriber);

                _toRemove.Clear();
            }

            foreach (Subscriber subscriber in _subscribers)
                subscriber.Invoke();
        }

        internal IDisposable Invoke(object onEndAttack)
        {
            throw new NotImplementedException();
        }
    }
}
