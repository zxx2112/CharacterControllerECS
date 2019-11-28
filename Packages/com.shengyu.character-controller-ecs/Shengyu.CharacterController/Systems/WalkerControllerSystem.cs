using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
//using UnityEngine;

namespace PhysicBaseCharacterControllerECS
{
    /// <summary>
    /// Input->Velocity
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public class WalkerControllerSystem : ComponentSystem
    {
        EntityQuery WalkerQuery;
        EntityQuery HeadQuery;
        BuildPhysicsWorld m_BuildPhysicsWorldSystem;

        float velocityMultipal = 5;
        float rotateSpeed = 100;
        float AdjustmentMultipal = 1f;

        private float _mouseXInput;
        private float _mouseYInput;
        private float _horizontalInput;
        private float _verticalInput;
        private bool _jumpKeyIsPressed;
        private float deltaTime;

        protected override void OnCreate() {
            WalkerQuery = GetEntityQuery(typeof(WalkerTag),typeof(CharacterControllerInternalData),typeof(Translation));
            HeadQuery = GetEntityQuery(typeof(PlayerHead));
            m_BuildPhysicsWorldSystem = World.GetOrCreateSystem<BuildPhysicsWorld>();
        }

        protected override void OnUpdate() {
            //收集输入
            GatherInput();

            Entities.With(WalkerQuery).ForEach((Entity entity, ref CharacterControllerInternalData ccInternalData,ref Translation translation) => {

                //检查是否在地面
                var isGrounded = CheckForGround(
                    m_BuildPhysicsWorldSystem.PhysicsWorld,
                    ccInternalData.CurrentControllerState,
                    translation.Value,
                    ccInternalData.RayHeight,
                    ccInternalData.CastLength,
                    ref ccInternalData.CurrentGroundAdjustmentVelocity,
                    ref ccInternalData.HitPoint,
                    ref ccInternalData.RayFrom
                    );

                //写入Debug信息
                ccInternalData.IsGrounded = isGrounded;
                //计算并写入新的状态和数据
                {
                    //状态切换
                    HandleState(ref ccInternalData.CurrentControllerState, ref ccInternalData.Momentum, isGrounded);

                    //处理移动惯性
                    ccInternalData.Momentum = HandleMomentum(ccInternalData.Momentum, ccInternalData.CurrentControllerState);

                    //处理跳跃
                    HandleJumping(ccInternalData.JumpSpeed,ref ccInternalData.CurrentControllerState, ref ccInternalData.Momentum);

                    //计算移动速度
                    var _movementVelocity = CalculateMovementVelocity(ccInternalData.CurrentRotationAngleY);
                    var _velocity = _movementVelocity + ccInternalData.Momentum;

                    //速度缓存下来
                    ccInternalData.Velocity = _velocity;
                    ccInternalData.MovementVelocity = _movementVelocity;

                    //计算新的旋转角度
                    ccInternalData.CurrentRotationAngleY = CalculateRotationAngleY(ccInternalData.CurrentRotationAngleY);
                }

            });

            //头部旋转写入数据
            Entities.With(HeadQuery).ForEach((Entity entity, ref PlayerHead head) => {
                var ylAngle = _mouseYInput * rotateSpeed * deltaTime;
                var newAngle = math.clamp(head.CurrentRotationAngleX + ylAngle, -45, 45);
                head.CurrentRotationAngleX = newAngle;
            });

        }

        private bool CheckForGround(PhysicsWorld physicsWorld,ControllerState _currentControllerState ,float3 playerPosition,float _rayHeight,float _castLength,ref float3 _currentGroundAdjustmentVelocity,ref float3 _hitPoint,ref float3 _rayFrom) {

            float3 newGroundAdjustmentVelocity;
            bool haveHit;

            //使用射线查询,之后改成使用ColliderDistance查询
            _rayFrom = playerPosition + new float3(0,1,0) * _rayHeight;

            var rayTo = playerPosition + new float3(0,-1,0) * _castLength;
            var stepHeight = 0.25f;

            if (_currentControllerState == ControllerState.Grounded)
                rayTo += new float3(0,-1,0) * stepHeight;

            //发送射线
            if (CastLine(physicsWorld, _rayFrom, rayTo, out var _hit)) {
                _hitPoint = _hit.Position;
                haveHit = true;
            }
            else {
                
                _hitPoint = rayTo;
                haveHit = false;
            }

            //贴合地面,处理上下台阶
            if(haveHit && _currentControllerState == ControllerState.Grounded) {
                var up = math.up();
                var dif = _hit.Position - playerPosition;
                //简单投影处理
                var distanceToGo = dif.y;
                newGroundAdjustmentVelocity = up * (distanceToGo / UnityEngine.Time.fixedDeltaTime) * AdjustmentMultipal;
            }
            else
                newGroundAdjustmentVelocity = float3.zero;


            _currentGroundAdjustmentVelocity = newGroundAdjustmentVelocity;
            return haveHit;
        }
        
        private bool CastLine(PhysicsWorld physicsWorld, float3 _origin,float3 _end,out RaycastHit _hit) {
            float3 RayFrom = _origin;
            float3 RayTo = _end;
            RaycastInput input = new RaycastInput() {
                Start = RayFrom,
                End = RayTo,
                Filter = new CollisionFilter() {
                    BelongsTo = ~0u, //地面层
                    CollidesWith = 1 << 0, //所有的层
                    GroupIndex = 0
                }
            };

            bool haveHit = physicsWorld.CollisionWorld.CastRay(input, out  _hit);
            if (haveHit) {
                if (_hit.RigidBodyIndex == -1) {
                    //但是不清出这代表什么含义
                    UnityEngine.Debug.LogError($"hit.RigidBodyIndex:{-1}");
                } else {
                    Entity e = physicsWorld.Bodies[_hit.RigidBodyIndex].Entity;
                }
            }

            return haveHit;
        }
        /// <summary>
        /// 处理状态装换
        /// </summary>
        private void HandleState(ref ControllerState _currentControllerState,ref float3 _movementum,bool _isGrounded) {
            var up = math.up();

            var _isRising = IsRisingOrFalling(_movementum) && GetDotProduct(_movementum,up) > 0f;
            var _isSliding = false;

            var newState = _currentControllerState;
            var newMovementum = _movementum;
            switch (_currentControllerState) {
                case ControllerState.Grounded:
                    if(_isRising) {
                        newState = ControllerState.Rising;//地面->上升
                        //当离开地面,重置移动惯性为只有竖直方向
                        OnGroundConcactLost(ref _movementum);
                        break;
                    }
                    if(!_isGrounded) {
                        newState = ControllerState.Falling;//地面到->下落
                        //当离开地面,重置移动惯性为只有竖直方向
                        OnGroundConcactLost(ref _movementum);
                        break;
                    }
                    if(_isSliding) {
                        newState = ControllerState.Sliding;//地面->滑动
                        break;
                    }
                    break;
                case ControllerState.Sliding:
                    break;
                case ControllerState.Falling:
                    if (_isRising) {
                        newState = ControllerState.Rising;
                        break;
                    }
                    if(_isGrounded) {
                        newState = ControllerState.Grounded;
                        //重新回到地面
                        OnGroundContactRegained(_movementum);
                    }
                    if(_isSliding) {
                        newState = ControllerState.Sliding;
                        //重新回到地面
                        OnGroundContactRegained(_movementum);
                    }

                    break;
                case ControllerState.Rising:
                    if(!_isRising) {
                        if(_isGrounded && ! _isSliding) {
                            newState = ControllerState.Grounded;
                            OnGroundContactRegained(_movementum);
                            break;
                        }
                        if(_isSliding) {
                            newState = ControllerState.Sliding;
                            OnGroundContactRegained(_movementum);
                            break;
                        }
                        if(_isGrounded) {
                            newState = ControllerState.Falling;
                            break;
                        }
                    }
                    break;
                case ControllerState.Jumping:
                    //这儿直接转成Rising状态,所以Jumping状态只持续一帧
                    newState = ControllerState.Rising;
                    break;
                default:
                    break;
            }

            _currentControllerState = newState;
            _movementum = newMovementum;
        }
        /// <summary>
        /// Momentum 移动惯性 
        /// </summary>
        /// <param name="momentum">旧的移动惯性</param>
        /// <returns>新的移动惯性</returns>
        private float3 HandleMomentum(float3 _momentum, ControllerState _currentControllerState) {
            float3 up = math.up();//暂时默认上方向为世界上方向
            float3 gravity = 9.81f;
            float jumpSpeed = 10f;

            float3 _verticalMomentum = float3.zero;//垂直移动趋势
            float3 _horizontalMomentum = float3.zero;//水平移动趋势

            //分离水平和垂直运动趋势
            if (!_momentum.Equals(float3.zero)) {
                _verticalMomentum = ExtractDotVector(_momentum, up);//在上方向的投影
                _horizontalMomentum = _momentum - _verticalMomentum;
            }
            //应用重力
            _verticalMomentum -= up * gravity * UnityEngine.Time.deltaTime;

            //如果在地面上,清除向下的运动趋势
            if (_currentControllerState == ControllerState.Grounded)
                _verticalMomentum = float3.zero;
            //如果在地面上的话要在水平趋势上增加摩檫力


            _momentum = _horizontalMomentum + _verticalMomentum;

            //应用跳跃
            if(_currentControllerState == ControllerState.Jumping) {
                _momentum = RemoveDotVector(_momentum, up);
                _momentum += up * jumpSpeed;
            }
            return _momentum;

        }

        private void GatherInput() {
            _mouseXInput = UnityEngine.Input.GetAxis("Mouse X");
            _mouseYInput = -UnityEngine.Input.GetAxis("Mouse Y");
            _horizontalInput = UnityEngine.Input.GetAxis("Horizontal");
            _verticalInput = UnityEngine.Input.GetAxis("Vertical");
            _jumpKeyIsPressed = UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Space);
            deltaTime = UnityEngine.Time.deltaTime;
        }

        private void HandleJumping(float jumpSpeed,ref ControllerState _currentControllerState,ref float3 _momntum) {
            //只有处在Grounded状态才允许跳跃
            if(_currentControllerState == ControllerState.Grounded) {
                if(_jumpKeyIsPressed) {
                    //重置为只有竖直移动惯性
                    OnGroundConcactLost(ref _momntum);
                    //起跳
                    OnJumpStart(jumpSpeed, ref _momntum);

                    _currentControllerState = ControllerState.Jumping;
                }
            }
        }

        /// <summary>
        /// 起跳,增加竖直方向速度以及开始计时
        /// </summary>
        private void OnJumpStart(float jumpSpeed,ref float3 _momentum) {
            var up = math.up();
            _momentum += up * jumpSpeed;
        }

        /// <summary>
        /// 计算移动速度
        /// </summary>
        private float3 CalculateMovementVelocity(float _currentRotationAngleY) {
            var _velocity = CalculateMovementDirection(_currentRotationAngleY);

            return _velocity * velocityMultipal;
        }

        //通过当前旋转方向计算移动方向
        private float3 CalculateMovementDirection(float currentRotationAngleY) {
            var up = math.up();
            var _velocity = float3.zero;

            var forwardRotation = quaternion.AxisAngle(up, math.radians(currentRotationAngleY));
            var forward = math.forward(forwardRotation) * _verticalInput;
            var right = math.rotate(forwardRotation, new float3(1, 0, 0)) * _horizontalInput;

            _velocity += forward;
            _velocity += right;

            if (math.length(_velocity) > 1)
                _velocity = math.normalize(_velocity);

            return _velocity;
        }

        private float CalculateRotationAngleY(float _rotationAngleY) {
            return _rotationAngleY + _mouseXInput * rotateSpeed * UnityEngine.Time.deltaTime;
        }

        /// <summary>
        /// 判断是否在上升或者下落
        /// </summary>
        /// <param name="_momentum"></param>
        /// <returns></returns>
        private bool IsRisingOrFalling(float3 _momentum) {
            var up = math.up();
            //计算竖直分量
            var _verticalMomentum = ExtractDotVector(_momentum, up);

            var _limit = 0.001f;

            return math.length(_verticalMomentum) > _limit;
        }

        /// <summary>
        /// 当离开地面的时候,只保留竖直方向速度
        /// </summary>
        /// <param name="_momentum"></param>
        private void OnGroundConcactLost(ref float3 _momentum) {
            var up = math.up();
            _momentum = ExtractDotVector(_momentum, up);
        }

        /// <summary>
        /// 重新回到地面
        /// </summary>
        /// <param name=""></param>
        private void OnGroundContactRegained(float3 _momentum) {

        }


        private static float3 ExtractDotVector(float3 _vector,float3 _direction) {
            if (math.length(_direction) != 1)
                _direction = math.normalize(_direction);

            float _amount = math.dot(_vector, _direction);

            return _direction * _amount;
        }

        private static float3 RemoveDotVector(float3 _vector,float3 _direction) {
            if (math.length(_direction) != 1)
                _direction = math.normalize(_direction);

            float _amount = math.dot(_vector, _direction);

            _vector -= _direction * _amount;

            return _vector;
        }
        private static float GetDotProduct(float3 _vector,float3 _direction) {
            if (math.length(_direction) != 1)
                _direction = math.normalize(_direction);

            var lenth = math.dot(_vector, _direction);

            return lenth;
        }
    }
}

