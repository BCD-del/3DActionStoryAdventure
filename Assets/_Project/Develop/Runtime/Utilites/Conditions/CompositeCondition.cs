using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets._Project.Develop.Runtime.Utilites.Conditions
{
    public class CompositeCondition : ICompositeCondition
    {
        private List<(ICondition, Func<bool, bool, bool>, int)> _conditions = new();
        private Func<bool, bool, bool> _standardLogicOperation;

        public CompositeCondition(Func<bool, bool, bool> standardLogicOperation)
        {
            _standardLogicOperation = standardLogicOperation;
        }

        public CompositeCondition() : this(LogicOperations.And)
        {

        }

        public ICompositeCondition Add(ICondition condition, int order = 0, Func<bool, bool, bool> logicOperation = null)
        {
            _conditions.Add((condition, logicOperation, order));
            _conditions = _conditions.OrderBy(cond => cond.Item3).ToList();
            return this;
        }

        public bool Evaluate()
        {
            if (_conditions.Count == 0)
                return false;

            bool result = _conditions[0].Item1.Evaluate();

            for (int i = 1; i < _conditions.Count; i++)
            {
                var currentCondition = _conditions[i];

                if (currentCondition.Item2 != null)
                    result = currentCondition.Item2.Invoke(result, currentCondition.Item1.Evaluate());
                else
                    result = _standardLogicOperation.Invoke(result, currentCondition.Item1.Evaluate());
            }

            return result;
        }

        public ICompositeCondition Remove(ICondition condition)
        {
            (ICondition, Func<bool, bool, bool>, int) conditionPair = _conditions.First(condPair => condPair.Item1 == condition);
            _conditions.Remove(conditionPair);
            return this;
        }
    }
}