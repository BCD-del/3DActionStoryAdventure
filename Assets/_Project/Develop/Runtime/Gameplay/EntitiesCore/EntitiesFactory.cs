using Assets._Project.Develop.Infrastructure.DI;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Entities;
using Assets._Project.Develop.Runtime.Configs.MainHero;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Mono;
using Assets._Project.Develop.Runtime.Gameplay.Features.ApplyDamage;
using Assets._Project.Develop.Runtime.Gameplay.Features.Attack;
using Assets._Project.Develop.Runtime.Gameplay.Features.Attack.MeleeAttack;
using Assets._Project.Develop.Runtime.Gameplay.Features.ContactTakeDamage;
using Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.LifeCycle;
using Assets._Project.Develop.Runtime.Gameplay.Features.LootFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.MovementFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.Sensors;
using Assets._Project.Develop.Runtime.Gameplay.Features.SpawnFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.TeamsFeature;
using Assets._Project.Develop.Runtime.Utilites;
using Assets._Project.Develop.Runtime.Utilites.Conditions;
using Assets._Project.Develop.Runtime.Utilites.Reactive;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.EntitiesCore
{
    public class EntitiesFactory
    {
        private readonly DIContainer _container;
        private readonly EntitiesLifeContext _entitiesLifeContext;

        private readonly MonoEntitiesFactory _monoEntitiesFactory;

        private readonly CollidersRegistryService _collidersRegistryService;

        public EntitiesFactory(DIContainer container)
        {
            _container = container;
            _entitiesLifeContext = container.Resolve<EntitiesLifeContext>();
            _monoEntitiesFactory = container.Resolve<MonoEntitiesFactory>();
            _collidersRegistryService = container.Resolve<CollidersRegistryService>();
        }

        public Entity CreateMainHero(Vector3 position, MainHeroConfig config)
        {
            Entity entity = CreateEmpty();

            _monoEntitiesFactory.Create(entity, position, config.PrefabPath);

            entity
               .AddMaxHealth(new ReactiveVariable<float>(config.MaxHealth))
               .AddCurrentHealth(new ReactiveVariable<float>(config.MaxHealth))

               .AddMoveSpeed(new ReactiveVariable<float>(config.MoveSpeed))
               .AddSprintMoveSpeedMultiplier(new ReactiveVariable<float>(config.SprintMoveSpeedMultiplier))
               .AddMoveDirection()
               .AddRotationDirection()
               .AddRotationSpeed()
               .AddIsMoving()
               
             

               .AddStartAttackRequest()
               .AddStartAttackEvent()
               .AddAttackProcessCurrentTime()
               .AddAttackProcessModifiedTime(new ReactiveVariable<float>(config.AttackProcessTime))
               .AddInAttackProcess()
               .AddAttackDelayModifiedTime(new ReactiveVariable<float>(config.AttackDelay))
               .AddAttackDelayEndEvent()
               .AddAttackCooldownModifiedTime(new ReactiveVariable<float>(config.AttackCooldown))
               .AddAttackCooldownCurrentTime()
               .AddInAttackCooldown()
               .AddEndAttackEvent()
               ;

            ICompositeCondition canStartAttack = new CompositeCondition();
            ICompositeCondition canMove = new CompositeCondition()
                .Add(new FuncCondition(() => entity.IsMoving.Value == false))
                .Add(new FuncCondition(() => entity.AttackCooldownCurrentTime.Value <= 0))
                ;

            entity
                .AddCanStartAttack(canStartAttack)
                .AddCanMove(canMove)
                ;

            entity
                // .AddSystem(new PlayerInputSystem(_container.Resolve<IInputService>()))

                .AddSystem(new CharacterControllerLocomotionSystem())

                .AddSystem(new StartAttackSystem())
                .AddSystem(new AttackProcessTimerSystem())
                .AddSystem(new AttackDelayEndTriggerSystem())
                .AddSystem(new AttackCooldownTimerSystem())
                .AddSystem(new EndAttackSystem())

                .AddSystem(new DealAttackSystem())
                ;

            return entity;
        }


        public void CreateCultist(Vector3 position, CultistConfig config)
        {
            Entity entity = CreateEmpty();

            _monoEntitiesFactory.Create(entity, position, config.PrefabPath);

            entity
                .AddTakeDamageRequest()
                .AddTakeDamageEvent()
                ;
        }

        /* public Entity CreateHero(Vector3 position, HeroConfig config, Dictionary<StatTypes, float> baseStats)
         {
             Entity entity = CreateEmpty();

             Dictionary<StatTypes, float> modifiedStats = new(baseStats);

             _monoEntitiesFactory.Create(entity, position, config.PrefabPath);

             entity
                 .AddAttacksPerSecond()
                 .AddInstantShootingDirection(new InstantShootingDirectionArgs(new InstantShotDirectionArgs(0, 1)))

                 .AddStatsEffects()
                 .AddBaseStats(baseStats)
                 .AddModifiedStats(modifiedStats)

                 .AddMoveDirection()
                 .AddMoveSpeed(new ReactiveVariable<float>(baseStats[StatTypes.MoveSpeed]))
                 .AddIsMoving()
                 .AddRotationSpeed(new ReactiveVariable<float>(config.RotationSpeed))
                 .AddRotationDirection()
                 .AddMaxHealth(new ReactiveVariable<float>(baseStats[StatTypes.MaxHealth]))
                 .AddCurrentHealth(new ReactiveVariable<float>(baseStats[StatTypes.MaxHealth]))
                 .AddIsDead()
                 .AddInDeathProcess()
                 .AddDeathProcessInitialTime(new ReactiveVariable<float>(config.DeathProcessTime))
                 .AddDeathProcessCurrentTime()
                 .AddTakeDamageRequest()
                 .AddTakeDamageEvent()

                 .AddAttackProcessInitialTime(new ReactiveVariable<float>(config.AttackProcessTime))
                 .AddAttackProcessModifiedTime(new ReactiveVariable<float>(config.AttackProcessTime))

                 .AddAttackProcessCurrentTime()
                 .AddInAttackProcess()
                 .AddStartAttackRequest()
                 .AddStartAttackEvent()
                 .AddEndAttackEvent()

                 .AddAttackDelayTime(new ReactiveVariable<float>(config.AttackDelayTime))
                 .AddAttackDelayModifiedTime(new ReactiveVariable<float>(config.AttackDelayTime))

                 .AddAttackDelayEndEvent()
                 .AddInstantAttackDamage(new ReactiveVariable<float>(baseStats[StatTypes.Damage]))
                 .AddAttackCanceledEvent()

                 .AddAttackCooldownInitialTime(new ReactiveVariable<float>(config.AttackCooldown))
                 .AddAttackCooldownModifiedTime(new ReactiveVariable<float>(config.AttackCooldown))

                 .AddAttackCooldownCurrentTime()
                 .AddInAttackCooldown()

                 .AddSpawnInitialTime(new ReactiveVariable<float>(config.SpawnProcessTime))
                 .AddSpawnCurrentTime()
                 .AddInSpawnProcess()
                 ;

             ICompositeCondition canMove = new CompositeCondition()
                 .Add(new FuncCondition(() => entity.IsDead.Value == false))
                 .Add(new FuncCondition(() => entity.InSpawnProcess.Value == false));

             ICompositeCondition canRotate = new CompositeCondition()
                 .Add(new FuncCondition(() => entity.IsDead.Value == false))
                 .Add(new FuncCondition(() => entity.InAttackProcess.Value == false))
                 .Add(new FuncCondition(() => entity.InSpawnProcess.Value == false));

             ICompositeCondition mustDie = new CompositeCondition()
                 .Add(new FuncCondition(() => entity.CurrentHealth.Value <= 0));

             ICompositeCondition mustSelfRelease = new CompositeCondition()
                 .Add(new FuncCondition(() => entity.IsDead.Value == true))
                 .Add(new FuncCondition(() => entity.InDeathProcess.Value == false));

             ICompositeCondition canApplyDamage = new CompositeCondition()
                 .Add(new FuncCondition(() => entity.IsDead.Value == false))
                 .Add(new FuncCondition(() => entity.InSpawnProcess.Value == false));

             ICompositeCondition canStartAttack = new CompositeCondition()
                 .Add(new FuncCondition(() => entity.IsDead.Value == false))
                 .Add(new FuncCondition(() => entity.InAttackProcess.Value == false))
                 .Add(new FuncCondition(() => entity.IsMoving.Value == false))
                 .Add(new FuncCondition(() => entity.InAttackCooldown.Value == false))
                 .Add(new FuncCondition(() => entity.InSpawnProcess.Value == false));

             ICompositeCondition mustCancelAttack = new CompositeCondition(LogicOperations.Or)
                 .Add(new FuncCondition(() => entity.IsDead.Value == true))
                 .Add(new FuncCondition(() => entity.IsMoving.Value == true));

             entity
                 .AddCanMove(canMove)
                 .AddCanRotate(canRotate)
                 .AddMustDie(mustDie)
                 .AddMustSelfRelease(mustSelfRelease)
                 .AddCanApplyDamage(canApplyDamage)
                 .AddCanStartAttack(canStartAttack)
                 .AddMustCancelAttack(mustCancelAttack);

             entity
                 .AddSystem(new StatEffectsApplierSystem())
                 .AddSystem(new MoveSpeedStatSynchronizerSystem())
                 .AddSystem(new MaxHealthStatSynchronizerSystem())
                 .AddSystem(new DamageStatSynchronizerSystem())

                 .AddSystem(new AttackTimeByAttackSpeedStatSyncronizerSystem())
                 .AddSystem(new AttackPerSecondStatSynchronizerSystem())

                 .AddSystem(new SpawnProcessTimerSystem())
                 .AddSystem(new RigidbodyMovementSystem())
                 .AddSystem(new RigidbodyRotationSystem())
                 .AddSystem(new AttackCancelSystem())
                 .AddSystem(new StartAttackSystem())
                 .AddSystem(new AttackProcessTimerSystem())
                 .AddSystem(new AttackDelayEndTriggerSystem())
                 .AddSystem(new DirectionsInstantShootSystem(this))
                 .AddSystem(new EndAttackSystem())
                 .AddSystem(new AttackCooldownTimerSystem())
                 .AddSystem(new ApplyDamageSystem())
                 .AddSystem(new DeathSystem())
                 .AddSystem(new DisableCollidersOnDeathSystem())
                 .AddSystem(new DeathProcessTimerSystem())
                 .AddSystem(new SelfReleaseSystem(_entitiesLifeContext))
                 ;

             return entity;
         }*/

        public Entity CreateGhost(Vector3 position, GhostConfig config)
        {
            Entity entity = CreateEmpty();

            _monoEntitiesFactory.Create(entity, position, config.PrefabPath);

            entity
                .AddMoveDirection()
                .AddMoveSpeed(new ReactiveVariable<float>(config.MoveSpeed))
                .AddIsMoving()
                .AddRotationSpeed(new ReactiveVariable<float>(config.RotationSpeed))
                .AddRotationDirection()
                .AddMaxHealth(new ReactiveVariable<float>(config.MaxHealth))
                .AddCurrentHealth(new ReactiveVariable<float>(config.MaxHealth))
                .AddIsDead()
                .AddInDeathProcess()
                .AddDeathProcessInitialTime(new ReactiveVariable<float>(config.DeathProcessTime))
                .AddDeathProcessCurrentTime()
                .AddTakeDamageRequest()
                .AddTakeDamageEvent()

                .AddContactsDetectingMask(LayersAPI.LayerMaskCharacters)
                .AddContactCollidersBuffer(new Buffer<Collider>(64))
                .AddContactEntitiesBuffer(new Buffer<Entity>(64))
                .AddBodyContactDamage(new ReactiveVariable<float>(config.BodyContactDamage))

                .AddSpawnInitialTime(new ReactiveVariable<float>(config.SpawnProcessTime))
                .AddSpawnCurrentTime()
                .AddInSpawnProcess()
                ;

            ICompositeCondition canMove = new CompositeCondition()
                .Add(new FuncCondition(() => entity.IsDead.Value == false))
                .Add(new FuncCondition(() => entity.InSpawnProcess.Value == false));

            ICompositeCondition canRotate = new CompositeCondition()
                .Add(new FuncCondition(() => entity.IsDead.Value == false))
                .Add(new FuncCondition(() => entity.InSpawnProcess.Value == false));

            ICompositeCondition mustDie = new CompositeCondition()
                .Add(new FuncCondition(() => entity.CurrentHealth.Value <= 0));

            ICompositeCondition mustSelfRelease = new CompositeCondition()
                .Add(new FuncCondition(() => entity.IsDead.Value == true))
                .Add(new FuncCondition(() => entity.InDeathProcess.Value == false));

            ICompositeCondition canApplyDamage = new CompositeCondition()
                .Add(new FuncCondition(() => entity.IsDead.Value == false))
                .Add(new FuncCondition(() => entity.InSpawnProcess.Value == false));

            entity
                .AddCanMove(canMove)
                .AddCanRotate(canRotate)
                .AddMustDie(mustDie)
                .AddMustSelfRelease(mustSelfRelease)
                .AddCanApplyDamage(canApplyDamage);

            entity
                .AddSystem(new RigidbodyMovementSystem())
                .AddSystem(new RigidbodyRotationSystem())
                .AddSystem(new ApplyDamageSystem())
                .AddSystem(new DeathSystem())
                .AddSystem(new DeathProcessTimerSystem())
                .AddSystem(new SelfReleaseSystem(_entitiesLifeContext))

                .AddSystem(new DisableCollidersOnDeathSystem())
                .AddSystem(new BodyContactDetectingSystem())
                .AddSystem(new BodyContactsEntitiesFilterSystem(_collidersRegistryService))
                .AddSystem(new DealDamageOnContactSystem())

                .AddSystem(new SpawnProcessTimerSystem())
                ;

            return entity;
        }

        public Entity CreateFireballProjectile(Vector3 position, Vector3 direction, float damage, Entity owner)
        {
            Entity entity = CreateEmpty();

            _monoEntitiesFactory.Create(entity, position, "Entities/FireballProjectile");

            entity
                .AddIsProjectile()
                .AddOwner(new ReactiveVariable<Entity>(owner))
                .AddMoveDirection(new ReactiveVariable<Vector3>(direction))
                .AddMoveSpeed(new ReactiveVariable<float>(25))
                .AddIsMoving()
                .AddRotationDirection(new ReactiveVariable<Vector3>(direction))
                .AddRotationSpeed(new ReactiveVariable<float>(9999))
                .AddIsDead()
                .AddContactsDetectingMask(LayersAPI.LayerMaskCharacters | LayersAPI.LayerMaskEnviroment)
                .AddContactCollidersBuffer(new Buffer<Collider>(64))
                .AddContactEntitiesBuffer(new Buffer<Entity>(64))
                .AddBodyContactDamage(new ReactiveVariable<float>(damage))
                .AddDeathMask(LayersAPI.LayerMaskEnviroment)
                .AddIsTouchDeathMask()
                .AddIsTouchAnotherTeam()
                .AddTeam(new ReactiveVariable<Teams>(owner.Team.Value));

            ICompositeCondition canMove = new CompositeCondition()
                .Add(new FuncCondition(() => entity.IsDead.Value == false));

            ICompositeCondition canRotate = new CompositeCondition()
                .Add(new FuncCondition(() => entity.IsDead.Value == false));

            ICompositeCondition mustDie = new CompositeCondition()
                .Add(new FuncCondition(() => entity.IsTouchDeathMask.Value), 0)
                .Add(new FuncCondition(() => entity.IsTouchAnotherTeam.Value), 10, LogicOperations.Or);

            ICompositeCondition mustSelfRelease = new CompositeCondition()
                .Add(new FuncCondition(() => entity.IsDead.Value));

            entity
                .AddCanMove(canMove)
                .AddCanRotate(canRotate)
                .AddMustDie(mustDie)
                .AddMustSelfRelease(mustSelfRelease);

            entity
                .AddSystem(new RigidbodyMovementSystem())
                .AddSystem(new RigidbodyRotationSystem())
                .AddSystem(new BodyContactDetectingSystem())
                .AddSystem(new BodyContactsEntitiesFilterSystem(_collidersRegistryService))
                .AddSystem(new DealDamageOnContactSystem())
                .AddSystem(new DeathMaskTouchDetectorSystem())
                .AddSystem(new AnotherTeamTouchDetectorSystem())
                .AddSystem(new DeathSystem())
                .AddSystem(new DisableCollidersOnDeathSystem())
                .AddSystem(new SelfReleaseSystem(_entitiesLifeContext));

            _entitiesLifeContext.Add(entity);

            return entity;
        }

        public Entity CreateContactTrigger(Vector3 position)
        {
            Entity entity = CreateEmpty();

            _monoEntitiesFactory.Create(entity, position, "Entities/ContactTrigger");

            entity
                .AddContactsDetectingMask(LayersAPI.LayerMaskCharacters)
                .AddContactCollidersBuffer(new Buffer<Collider>(64))
                .AddContactEntitiesBuffer(new Buffer<Entity>(64));

            entity
                  .AddSystem(new BodyContactDetectingSystem())
                  .AddSystem(new BodyContactsEntitiesFilterSystem(_collidersRegistryService));

            _entitiesLifeContext.Add(entity);

            return entity;
        }


        public Entity CreatePullable(string prefabPath, Vector3 position)
        {
            Entity entity = CreateEmpty();

            _monoEntitiesFactory.Create(entity, position, prefabPath);

            entity
                .AddIsPullable()
                .AddIsPullingProcess()
                .AddInSpawnProcess(new ReactiveVariable<bool>(true))
                .AddCurrentTarget(new ReactiveVariable<Entity>(null))
                .AddMoveDirection()
                .AddMoveSpeed(new ReactiveVariable<float>(12))
                .AddIsMoving()
                .AddIsCollected();

            ICompositeCondition moveCondition = new CompositeCondition()
                .Add(new FuncCondition(() => entity.IsPullingProcess.Value))
                .Add(new FuncCondition(() => entity.InSpawnProcess.Value == false));

            ICompositeCondition mustSelfRelease = new CompositeCondition()
                .Add(new FuncCondition(() => entity.IsCollected.Value));

            entity
                .AddCanMove(moveCondition)
                .AddMustSelfRelease(mustSelfRelease);

            entity
                .AddSystem(new GenerateMoveDirectionToTargetSystem())
                .AddSystem(new RigidbodyMovementSystem())
                .AddSystem(new CollectedOnNearToTargetSystem())
                .AddSystem(new SelfReleaseSystem(_entitiesLifeContext));

            return entity;
        }

        private Entity CreateEmpty() => new Entity();
    }
}
