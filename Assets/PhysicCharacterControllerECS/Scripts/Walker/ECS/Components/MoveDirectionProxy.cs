using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace PhysicBaseCharacterControllerECS
{

    public struct MoveDirection : IComponentData
    {
        public float3 Value;
    }

    [DisallowMultipleComponent]
    [RequiresEntityConversion]
    public class MoveDirectionProxy : MonoBehaviour, IConvertGameObjectToEntity
    {


        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            dstManager.AddComponentData(entity, new MoveDirection());
        }
    }
}



