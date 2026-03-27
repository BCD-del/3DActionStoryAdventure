using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Gameplay.Features.TeamsFeature;
using Assets._Project.Develop.Runtime.Utilites;
using Assets._Project.Develop.Runtime.Utilites.Reactive;
using System;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.Sensors
{
    public class AnotherTeamTouchDetectorSystem : IInitializableSystem, IUpdatableSystem
    {
        private Buffer<Entity> _contacts;
        private ReactiveVariable<bool> _isTouchAnotherTeam;
        private ReactiveVariable<Teams> _sourceTeam;

        public void OnInit(Entity entity)
        {
            _contacts = entity.ContactEntitiesBuffer;
            _isTouchAnotherTeam = entity.IsTouchAnotherTeam;
            _sourceTeam = entity.Team;
        }

        public void OnUpdate(float deltaTime)
        {
            for (int i = 0; i < _contacts.Count; i++)
            {
                Entity contact = _contacts.Items[i];

                if (contact.TryGetTeam(out ReactiveVariable<Teams> anotherTeam))
                {
                    if (_sourceTeam.Value != anotherTeam.Value)
                    {
                        _isTouchAnotherTeam.Value = true;
                        return;
                    }
                }
            }

            _isTouchAnotherTeam.Value = false;
        }
    }
}
