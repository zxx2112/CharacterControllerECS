using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Unity.Transforms;
using Unity.Mathematics;

public class EntityTracker : MonoBehaviour,IEntityReceiver
{
    private EntityManager em;
    private Entity entityCache;

    public void ReceiveEntity(Entity entity) {
        entityCache = entity;
    }

    void Start()
    {
        em = World.Active.EntityManager;
    }

    void Update()
    {
        if (entityCache == Entity.Null) return;

        var localToWorld = em.GetComponentData<LocalToWorld>(entityCache);

        transform.position = localToWorld.Position;
        transform.rotation = quaternion.LookRotation(localToWorld.Forward, math.up());
    }
}
