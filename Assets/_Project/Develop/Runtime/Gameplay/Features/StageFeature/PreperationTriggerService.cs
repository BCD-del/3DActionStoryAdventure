using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.MainHero;
using Assets._Project.Develop.Runtime.Utilites;
using Assets._Project.Develop.Runtime.Utilites.Reactive;
using System;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.StageFeature
{
    public class PreperationTriggerService
    {
        private ReactiveVariable<bool> _hasMainHeroContact = new();

        private EntitiesFactory _entitiesFactory;
        private EntitiesLifeContext _entitiesLifeContext;

        private Entity _nextStageTrigger;
        private Buffer<Entity> _nextStageTriggerContacts;

        public PreperationTriggerService(
            EntitiesFactory entitiesFactory, 
            EntitiesLifeContext entitiesLifeContext)
        {
            _entitiesFactory = entitiesFactory;
            _entitiesLifeContext = entitiesLifeContext;
        }

        public IReadOnlyVariable<bool> HasMainHeroContact => _hasMainHeroContact;

        public void Create(Vector3 at)
        {
            if (_nextStageTrigger != null)
                throw new InvalidOperationException("Trigger is already created");

            _nextStageTrigger = _entitiesFactory.CreateContactTrigger(at);
            _nextStageTriggerContacts = _nextStageTrigger.ContactEntitiesBuffer;
        }

        public void Update(float deltaTime)
        {
            if (_nextStageTrigger == null)
                return;

            for (int i = 0; i < _nextStageTriggerContacts.Count; i++)
            {
                Entity contact = _nextStageTriggerContacts.Items[i];

                if (contact.HasComponent<IsMainHero>())
                {
                    _hasMainHeroContact.Value = true;
                    return;
                }
            }

            _hasMainHeroContact.Value = false;
        }

        public void Cleanup()
        {
            _entitiesLifeContext.Release(_nextStageTrigger);
            _hasMainHeroContact.Value = false;
            _nextStageTrigger = null;
            _nextStageTriggerContacts = null;
        }
    }
}
