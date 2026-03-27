using System;

namespace Assets._Project.Develop.Runtime.Utilites.Conditions
{
    public class FuncCondition : ICondition
    {
        private Func<bool> _condition;

        public FuncCondition(Func<bool> condition)
        {
            _condition = condition;
        }

        public bool Evaluate() => _condition.Invoke();
    }
}
