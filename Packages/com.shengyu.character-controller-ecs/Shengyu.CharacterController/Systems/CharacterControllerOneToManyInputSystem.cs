using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

namespace PhysicBaseCharacterControllerECS
{
    /// <summary>
    /// CharacterControllerInput (One) -> CharacterControllerInput (Many)
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public class CharacterControllerOneToManyInputSystem : ComponentSystem
    {
        EntityQuery CharacterControllerInputQuery;
        protected override void OnCreate() {
            CharacterControllerInputQuery = GetEntityQuery(ComponentType.ReadOnly<CharacterControllerInput>());
        }

        protected override void OnUpdate() {
            var input = CharacterControllerInputQuery.GetSingleton<CharacterControllerInput>();
            Entities.ForEach((ref CharacterControllerInternalData ccData) => {
            });
        }
    }
}

