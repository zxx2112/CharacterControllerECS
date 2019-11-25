using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace PhysicBaseCharacterControllerECS
{

    public struct MoveVelocity : IComponentData
    {
        public float3 Value;
    }

    [DisallowMultipleComponent]
    [RequiresEntityConversion]
    public class MoveVelocityProxy : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            dstManager.AddComponentData(entity, new MoveVelocity());
        }
    }
}



