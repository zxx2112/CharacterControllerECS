using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace PhysicBaseCharacterControllerECS
{
    public struct CameraControlsTag : IComponentData
    {

    }

    [DisallowMultipleComponent]
    [RequiresEntityConversion]
    public class CameraControlsTagProxy : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            dstManager.AddComponent<CameraControlsTag>(entity);
        }
    }



}


