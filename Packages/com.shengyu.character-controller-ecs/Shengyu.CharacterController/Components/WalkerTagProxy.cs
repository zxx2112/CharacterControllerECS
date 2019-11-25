using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
namespace PhysicBaseCharacterControllerECS
{
    public struct WalkerTag:IComponentData
    {
        public float nothing;
    }

    [DisallowMultipleComponent]
    [RequiresEntityConversion]
    public class WalkerTagProxy : MonoBehaviour, IConvertGameObjectToEntity
    {

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            dstManager.AddComponent<WalkerTag>(entity);
        }

    }
}



