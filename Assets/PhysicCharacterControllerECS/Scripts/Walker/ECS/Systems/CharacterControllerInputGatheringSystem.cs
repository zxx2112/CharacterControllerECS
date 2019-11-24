using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;
using UnityEngine;

namespace PhysicBaseCharacterControllerECS
{
    /// <summary>
    /// Device Input->CharacterControllerInput
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(CharacterControllerOneToManyInputSystem))]
    public class CharacterControllerInputGatheringSystem : ComponentSystem
    {
        EntityQuery WalkerQuery;
        EntityQuery CharacterControllerInputQuery;

        protected override void OnCreate() {
            CharacterControllerInputQuery = GetEntityQuery(typeof(CharacterControllerInput));
            WalkerQuery = GetEntityQuery(typeof(WalkerTag));
        }

        protected override void OnUpdate() {
            if (WalkerQuery.CalculateEntityCount() == 0) return;

            if (CharacterControllerInputQuery.CalculateEntityCount() == 0)
                EntityManager.CreateEntity(typeof(CharacterControllerInput));

            var movementX = Input.GetAxis("Horizontal");
            var movementY = Input.GetAxis("Vertical");
            var lookX = Input.GetAxis("Mouse X");
            var lookY = -Input.GetAxis("Mouse Y");
            var jump = Input.GetKeyDown(KeyCode.Space) ? 1 : 0;

            CharacterControllerInputQuery.SetSingleton(new CharacterControllerInput {
                Movement = new float2(movementX, movementY),
                Looking = new float2(lookX, lookY),
                Jumped = jump
            });

        }
    }
}

