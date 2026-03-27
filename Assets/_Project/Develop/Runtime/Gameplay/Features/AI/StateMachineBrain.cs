using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.AI
{
    public class StateMachineBrain : IBrain
    {
        private AIStateMachine _stateMachine;

        private bool _isEnabled;

        public StateMachineBrain(AIStateMachine stateMachine)
        {
            _stateMachine = stateMachine;
        }

        public void Disable()
        {
            _stateMachine.Exit();
            _isEnabled = false;
        }

        public void Dispose()
        {
            _stateMachine.Dispose();
            _isEnabled = false;
        }

        public void Enable()
        {
            _stateMachine.Enter();
            _isEnabled = true;
        }

        public void Update(float deltaTime)
        {
            if (_isEnabled == false)
                return;

            _stateMachine.Update(deltaTime);
        }
    }
}
