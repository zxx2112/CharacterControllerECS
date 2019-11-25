using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using UnityEngine;

public struct SendEntity : IComponentData
{

}

public class EntitySender : MonoBehaviour, IConvertGameObjectToEntity
{

    public GameObject[] entityReceivers;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
        dstManager.AddComponentData(entity, new SendEntity());


        foreach (var receiverObj in entityReceivers.Where(n => n != null)) {
            var receiver = receiverObj.GetComponent<IEntityReceiver>();
            Debug.Log($"发送{name} Entity 到 {receiverObj.name}");
            receiver?.ReceiveEntity(entity);
        }
    }
}
