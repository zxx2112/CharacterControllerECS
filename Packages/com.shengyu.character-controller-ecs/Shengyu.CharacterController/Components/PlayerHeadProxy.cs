using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct PlayerHead:IComponentData
{
    public float CurrentRotationAngleX;
}


[DisallowMultipleComponent]
[RequiresEntityConversion]
public class PlayerHeadProxy : MonoBehaviour, IConvertGameObjectToEntity
{


    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {

        dstManager.AddComponentData(entity, new PlayerHead());
    }
}
