using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace PhysicBaseCharacterControllerECS
{
    public struct CameraRootTag : IComponentData
    {

    }

    [DisallowMultipleComponent]
    [RequiresEntityConversion]
    public class CameraRootTagProxy : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            dstManager.AddComponent<CameraRootTag>(entity);
        }
    }
}


