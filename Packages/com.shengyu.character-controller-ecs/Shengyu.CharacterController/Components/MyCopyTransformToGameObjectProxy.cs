using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Transforms;

namespace PhysicBaseCharacterControllerECS
{
    [DisallowMultipleComponent]
    [RequiresEntityConversion]
    public class MyCopyTransformToGameObjectProxy : MonoBehaviour, IConvertGameObjectToEntity
    {

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            dstManager.AddComponent<CopyTransformToGameObject>(entity);
        }
    }
}


