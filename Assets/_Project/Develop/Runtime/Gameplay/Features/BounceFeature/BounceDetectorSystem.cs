using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Utilites.Reactive;
using Assets._Project.Develop.Runtime.Utilites;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.BounceFeature
{
    public class BounceDetectorSystem : IInitializableSystem, IUpdatableSystem
    {
        private LayerMask _layerToBounceReaction;
        private Buffer<Collider> _contacts;
        private Transform _transform;
        private ReactiveEvent<RaycastHit> _bounceEvent;

        private Vector3 _previousPosition;
        private Collider _previousObject;

        public void OnInit(Entity entity)
        {
            _transform = entity.Transform;
            _layerToBounceReaction = entity.LayerToBounceReaction;
            _contacts = entity.ContactCollidersBuffer;
            _bounceEvent = entity.BounceEvent;

            _previousPosition = _transform.position;
        }

        public void OnUpdate(float deltaTime)
        {
            if (_contacts.Count > 0)
            {
                List<Collider> bounceContacts = new();

                for (int i = 0; i < _contacts.Count; i++)
                    if (MatchWithBounceLayer(_contacts.Items[i]))
                        bounceContacts.Add(_contacts.Items[i]);

                if (bounceContacts.Any())
                {
                    if (Physics.Raycast(_previousPosition, _transform.forward, out RaycastHit hit, 1000, _layerToBounceReaction.value))
                    {
                        if (hit.collider != _previousObject && bounceContacts.Contains(hit.collider))
                        {
                            _previousObject = hit.collider;
                            _bounceEvent.Invoke(hit);
                            _previousPosition = _transform.position;
                        }
                    }
                }
                else
                {
                    _previousPosition = _transform.position;
                }
            }
        }

        private bool MatchWithBounceLayer(Collider collider)
        {
            return ((1 << collider.gameObject.layer) & _layerToBounceReaction) != 0;
        }
    }
}

