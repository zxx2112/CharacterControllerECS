//using Unity.Burst;
//using Unity.Collections;
//using Unity.Entities;
//using Unity.Jobs;
//using Unity.Mathematics;
//using Unity.Transforms;
//using Unity.Physics;
//using Unity.Physics.Systems;
//using Unity.Physics.Extensions;

//namespace PhysicBaseCharacterControllerECS
//{
//    //旋转约束系统

//    [DisableAutoCreation]
//    [UpdateBefore(typeof(StepPhysicsWorld))]
//    public class RotationConstraintSystem : ComponentSystem
//    {
//        EntityQuery PhysicsBodyQuery;

//        protected override void OnCreate() {
//            PhysicsBodyQuery = GetEntityQuery(typeof(PhysicsVelocity),typeof(Rotation));
//        }

//        protected override void OnUpdate() {
//            Entities.With(PhysicsBodyQuery).ForEach((Entity entity, ref PhysicsVelocity physicsVelocity,ref Rotation rotation) => {
//                rotation.Value = quaternion.identity;
//            });
//        }
//    }
//}

