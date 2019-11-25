using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public struct CharacterControllerInternalData : IComponentData
{
    public float3 Momentum;//当前线速度
    public float3 Velocity;//(移动速度+惯性速度)
    public float3 MovementVelocity;//相对移动速度
    public float3 CurrentGroundAdjustmentVelocity;//调整贴合地面的速度
    public ControllerState CurrentControllerState;//当前角色的状态
    public float RayHeight;//射线发出点的高度
    public float CastLength;//射线检测长度
    public float JumpSpeed;//跳跃速度

    public float CurrentRotationAngleY;//当前Y轴旋转
    public bool isJumping;//是否正在跳跃
    public float3 UnsupportedVelocity;//离地速度
    public bool JumpRequest;
    //Debug
    public bool IsGrounded;
    public float3 RayFrom;//射线发出点
    public float3 HitPoint;//射线击中点

    public bool Equals(CharacterControllerInternalData other) {
        throw new NotImplementedException();
    }
}


public enum ControllerState
{
    Grounded,
    Sliding,
    Falling,
    Rising,
    Jumping
}

namespace PhysicBaseCharacterControllerECS
{
    [DisallowMultipleComponent]
    [RequiresEntityConversion]
    public class CharacterControllerInternalDataProxy : MonoBehaviour, IConvertGameObjectToEntity
    {
        public float rayHeight = 0.25f;
        public float castLength = 1;
        public float jumpSpeed = 5;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            dstManager.AddComponentData(entity, new CharacterControllerInternalData() {
                RayHeight = rayHeight,
                CastLength = castLength,
                JumpSpeed = jumpSpeed
            }); ;
        }
    }
}


