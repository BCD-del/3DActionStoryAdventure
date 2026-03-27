using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Utilites.Reactive;
using Assets._Project.Develop.Runtime.Utilites.StateMachineCore;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.AI.States
{

    public class RandomMovementState : State, IUpdatableState
    {
        private ReactiveVariable<Vector3> _movementDirection;
        private ReactiveVariable<Vector3> _rotationDirection;

        private float _cooldownBetweenDirectionGeneration;

        private float _timer;

        public RandomMovementState(
            Entity entity,
            float cooldownBetweenDirectionGeneration)
        {
            _movementDirection = entity.MoveDirection;
            _rotationDirection = entity.RotationDirection;

            _cooldownBetweenDirectionGeneration = cooldownBetweenDirectionGeneration;
        }

        public override void Enter()
        {
            base.Enter();

            Vector3 randomDirection = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;

            _movementDirection.Value = randomDirection;
            _rotationDirection.Value = randomDirection;

            _timer = 0;
        }

        public override void Exit()
        {
            base.Exit();

            _movementDirection.Value = Vector3.zero;
        }

        public void Update(float deltaTime)
        {
            _timer += deltaTime;

            if (_timer >= _cooldownBetweenDirectionGeneration)
            {
                GenerateNewDirection();
                _timer = 0;
            }
        }

        private void GenerateNewDirection()
        {
            Vector3 inverseDirection = -_movementDirection.Value.normalized;
            Quaternion randomTurn = Quaternion.Euler(0, Random.Range(-30, 30), 0);
            Vector3 newDirection = randomTurn * inverseDirection;

            _movementDirection.Value = newDirection;
            _rotationDirection.Value = newDirection;
        }
    }
}
