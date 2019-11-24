using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using Unity.Physics.Extensions;
using UnityEngine;
using Unity.Physics.Systems;

namespace PhysicBaseCharacterControllerECS
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(WalkerControllerSystem))]
    public class WalkerPhysicMoveSystem : ComponentSystem
    {
        EntityQuery WalkerQuery;
        EntityQuery HeadQuery;
        BuildPhysicsWorld m_BuildPhysicsWorldSystem;
        protected override void OnCreate() {
            WalkerQuery = GetEntityQuery(typeof(WalkerTag),typeof(Translation) ,typeof(Rotation),typeof(CharacterControllerInternalData),typeof(PhysicsVelocity));
            HeadQuery = GetEntityQuery(typeof(PlayerHead),typeof(Rotation));
            m_BuildPhysicsWorldSystem = World.GetOrCreateSystem<BuildPhysicsWorld>();
        }

        protected override void OnUpdate() {
            Entities.With(WalkerQuery).ForEach((Entity entity,ref Translation position ,ref Rotation rotation,ref CharacterControllerInternalData ccInternalData, ref PhysicsVelocity physicsVelocity) =>
            {
                //写入移动的速度
                physicsVelocity.Linear = ccInternalData.Velocity + ccInternalData.CurrentGroundAdjustmentVelocity;
                //写入新的旋转
                rotation.Value = quaternion.AxisAngle(math.up(), math.radians(ccInternalData.CurrentRotationAngleY));

                //头部旋转也在这里完成吧
                Entities.With(HeadQuery).ForEach((Entity headEntity, ref PlayerHead head,ref Rotation headRotation) => {
                    headRotation.Value = quaternion.Euler(new float3(math.radians(head.CurrentRotationAngleX), 0, 0));
                });
            });

        }
    }
}

