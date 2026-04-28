using Game.Player.Animation;
using Game.Player.Input;
using Game.Player.Movement;
using System;
using System.Collections.Generic;

namespace Game.Player.FSM
{
    /// <summary>
    /// 玩家状态编号，用来让状态机通过稳定 ID 查找具体状态对象。
    /// 商业项目中新增 Dash、Attack、Hurt 等动作时，优先新增枚举值和状态类，而不是让状态类互相保存具体引用。
    /// </summary>
    public enum PlayerStateId
    {
        /// <summary>
        /// 落地状态，角色站在地面上，可以普通移动和起跳。
        /// </summary>
        Grounded,

        /// <summary>
        /// 空中状态，角色离开地面，可以空中移动、短跳和落地。
        /// </summary>
        Airborne
    }

    /// <summary>
    /// 玩家状态机，同一时间只允许一个 PlayerState 生效。
    /// 通过 PlayerStateId 注册和切换状态，避免状态类之间形成复杂的互相引用。
    /// </summary>
    public class PlayerStateMachine
    {
        /// <summary>
        /// 已注册的状态表。
        /// Dictionary 是 C# 泛型集合，这里用状态 ID 快速找到对应状态对象。
        /// </summary>
        private readonly Dictionary<PlayerStateId, PlayerState> states = new Dictionary<PlayerStateId, PlayerState>();

        /// <summary>
        /// 当前正在运行的状态对象。
        /// </summary>
        private PlayerState current;

        /// <summary>
        /// 当前状态 ID。可空类型表示状态机可能还没有进入任何状态。
        /// </summary>
        private PlayerStateId? currentId;

        /// <summary>
        /// 注册一个状态到状态机。
        /// 后续状态切换只需要传 PlayerStateId，不需要状态之间互相保存引用。
        /// </summary>
        /// <param name="id">状态编号。</param>
        /// <param name="state">状态对象实例。</param>
        public void RegisterState(PlayerStateId id, PlayerState state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));

            states[id] = state;
        }

        /// <summary>
        /// 切换到指定 ID 对应的状态。
        /// </summary>
        /// <param name="nextId">准备进入的新状态 ID。</param>
        public void ChangeState(PlayerStateId nextId)
        {
            if (currentId.HasValue && currentId.Value == nextId)
                return;

            if (!states.TryGetValue(nextId, out PlayerState next))
                throw new InvalidOperationException($"Player state '{nextId}' has not been registered.");

            // 切状态时保证旧状态先清理，新状态再初始化。
            current?.Exit();
            currentId = nextId;
            current = next;
            current?.Enter();
        }

        /// <summary>
        /// 每帧更新当前状态，通常处理输入、状态切换和一次性逻辑。
        /// </summary>
        /// <param name="deltaTime">当前帧耗时。</param>
        public void Tick(float deltaTime)
        {
            current?.Tick(deltaTime);
        }

        /// <summary>
        /// 固定物理帧更新当前状态，通常处理 Rigidbody2D 速度和 AddForce。
        /// </summary>
        /// <param name="fixedDeltaTime">固定物理帧耗时。</param>
        public void FixedTick(float fixedDeltaTime)
        {
            current?.FixedTick(fixedDeltaTime);
        }
    }

    /// <summary>
    /// 玩家状态上下文，集中存放状态需要访问的组件，避免每个状态自己 GetComponent。
    /// 上下文只保存依赖，不保存状态切换规则。
    /// </summary>
    public sealed class PlayerStateContext
    {
        /// <summary>
        /// 创建玩家状态上下文。
        /// </summary>
        /// <param name="input">玩家输入读取器。</param>
        /// <param name="motor">玩家运动执行器。</param>
        /// <param name="ground">玩家地面检测器。</param>
        /// <param name="anim">玩家动画桥接器。</param>
        public PlayerStateContext(
            PlayerInputReader input,
            PlayerMotor2D motor,
            PlayerGroundDetector2D ground,
            PlayerAnimatorBridge anim)
        {
            Input = input;
            Motor = motor;
            Ground = ground;
            Anim = anim;
        }

        /// <summary>
        /// 玩家输入读取器，提供移动、跳跃等输入状态。
        /// </summary>
        public PlayerInputReader Input { get; }

        /// <summary>
        /// 玩家运动执行器，负责 Rigidbody2D 速度和跳跃力。
        /// </summary>
        public PlayerMotor2D Motor { get; }

        /// <summary>
        /// 玩家地面检测器，负责判断角色是否落地。
        /// </summary>
        public PlayerGroundDetector2D Ground { get; }

        /// <summary>
        /// 玩家动画桥接器，负责写入 Animator 参数。
        /// </summary>
        public PlayerAnimatorBridge Anim { get; }
    }

    /// <summary>
    /// 所有玩家状态的抽象基类。
    /// 抽象类不能直接实例化，子类只重写自己关心的生命周期。
    /// </summary>
    public abstract class PlayerState
    {
        /// <summary>
        /// 创建玩家状态。
        /// </summary>
        /// <param name="stateMachine">所属状态机，用于在状态内部发起切换。</param>
        /// <param name="context">玩家状态上下文。</param>
        protected PlayerState(PlayerStateMachine stateMachine, PlayerStateContext context)
        {
            StateMachine = stateMachine;
            Context = context;
        }

        /// <summary>
        /// 所属玩家状态机。
        /// </summary>
        protected PlayerStateMachine StateMachine { get; }

        /// <summary>
        /// 玩家状态共享上下文。
        /// </summary>
        protected PlayerStateContext Context { get; }

        /// <summary>
        /// 进入状态时调用。
        /// </summary>
        public virtual void Enter()
        {
        }

        /// <summary>
        /// 离开状态时调用。
        /// </summary>
        public virtual void Exit()
        {
        }

        /// <summary>
        /// 每帧调用，适合处理输入和状态切换。
        /// </summary>
        /// <param name="deltaTime">当前帧耗时。</param>
        public virtual void Tick(float deltaTime)
        {
        }

        /// <summary>
        /// 固定物理帧调用，适合处理 Rigidbody2D 运动。
        /// </summary>
        /// <param name="fixedDeltaTime">固定物理帧耗时。</param>
        public virtual void FixedTick(float fixedDeltaTime)
        {
        }
    }
}
