using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

namespace PhysicBaseCharacterControllerECS
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class WalkerInitSystem : ComponentSystem
    {
        EntityQuery WalkerQuery_MissingCharacterControllerInternalData;
        protected override void OnCreate() {
            WalkerQuery_MissingCharacterControllerInternalData = GetEntityQuery(typeof(WalkerTag), ComponentType.Exclude<CharacterControllerInternalData>());
        }

        protected override void OnUpdate() {
            Entities.With(WalkerQuery_MissingCharacterControllerInternalData).ForEach((Entity entity) => {
                EntityManager.AddComponentData(entity, new CharacterControllerInternalData { 
                    CurrentRotationAngleY = 0
                });
            });
        }
    }
}

