using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;

namespace PhysicBaseCharacterControllerECS
{
    public class WalkerDebugBehaviour : MonoBehaviour,IEntityReceiver
    {
        EntityManager _em;
         Entity _walkerEntity; 

        void Start() {
            _em = World.Active.EntityManager;
        }

        void Update() {
            if (_em == null) return;
            if (!_em.Exists(_walkerEntity)) return;

            var translation = _em.GetComponentData<Translation>(_walkerEntity);
            var internalData = _em.GetComponentData<CharacterControllerInternalData>(_walkerEntity);

            Debug.DrawLine(internalData.RayFrom, internalData.HitPoint,internalData.IsGrounded? Color.red : Color.blue);
        }

        public void ReceiveEntity(Entity entity) {
            _walkerEntity = entity;
        }
    }
}


