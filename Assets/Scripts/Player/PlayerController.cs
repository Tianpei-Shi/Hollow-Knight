using Game.Player.Animation;
using Game.Player.Config;
using Game.Player.FSM;
using Game.Player.FSM.States;
using Game.Player.Input;
using Game.Player.Movement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace Game.Player
{
    /// <summary>
    /// 玩家角色控制入口，负责收集 Player 相关组件、注入配置并驱动状态机。
    /// RequireComponent 是 Unity Attribute，用来保证角色对象上必须存在输入、运动、地面检测和动画桥接组件。
    /// </summary>
    [RequireComponent(typeof(PlayerInput))]
    [RequireComponent(typeof(PlayerInputReader))]
    [RequireComponent(typeof(PlayerMotor2D))]
    [RequireComponent(typeof(PlayerGroundDetector2D))]
    [RequireComponent(typeof(PlayerAnimatorBridge))]
    public class PlayerController : MonoBehaviour
    {
        /// <summary>
        /// 玩家配置资产。移动、跳跃、地面检测、动画参数都从这里读取。
        /// </summary>
        [Header("Config")]
        [Tooltip("角色配置资产。移动、跳跃、地面检测、动画参数都从这里读取。")]
        [SerializeField] private PlayerConfig config = null;

        /// <summary>
        /// 玩家输入读取器，负责把 Input System 的 Action 转换成角色输入数据。
        /// </summary>
        [Header("Parts")]
        [SerializeField] private PlayerInputReader input;

        /// <summary>
        /// 玩家运动执行器，负责 Rigidbody2D 速度、跳跃和朝向。
        /// </summary>
        [SerializeField] private PlayerMotor2D motor;

        /// <summary>
        /// 玩家地面检测器，负责判断角色是否踩在地面上。
        /// </summary>
        [FormerlySerializedAs("groundDetector")]
        [SerializeField] private PlayerGroundDetector2D ground;

        /// <summary>
        /// 玩家动画桥接器，负责把逻辑状态写入 Animator 参数。
        /// </summary>
        [FormerlySerializedAs("animatorBridge")]
        [SerializeField] private PlayerAnimatorBridge anim;

        /// <summary>
        /// 玩家状态机实例。FSM 是 Finite State Machine 的常见缩写，商业项目中经常这样命名。
        /// </summary>
        private PlayerStateMachine fsm;

        /// <summary>
        /// Unity 生命周期方法：对象加载时调用。
        /// 这里收集组件、注入配置、创建状态机，并进入初始状态。
        /// </summary>
        private void Awake()
        {
            CacheParts();
            ApplyConfig();
            CreateStateMachine();
            EnterInitialState();
        }

        /// <summary>
        /// Unity 编辑器回调：Inspector 修改引用或配置时调用。
        /// 用它自动补齐组件引用并同步配置，方便边调参数边测试。
        /// </summary>
        private void OnValidate()
        {
            CacheParts();
            ApplyConfig();
        }

        /// <summary>
        /// Unity 生命周期方法：每帧调用一次。
        /// 状态切换和一次性输入放在 Update，保证按键按下/松开不会被 FixedUpdate 漏掉。
        /// </summary>
        private void Update()
        {
            fsm.Tick(Time.deltaTime);

            // WasJumpPressed / WasJumpReleased 只保留一帧，当前帧消费完就清空。
            input.ClearFrameActions();
        }

        /// <summary>
        /// Unity 生命周期方法：按固定物理步长调用。
        /// 速度和力相关逻辑放在 FixedUpdate，和 Unity 2D 物理系统保持一致。
        /// </summary>
        private void FixedUpdate()
        {
            fsm.FixedTick(Time.fixedDeltaTime);
        }

        /// <summary>
        /// 缓存 Player 需要的同对象组件引用。
        /// RequireComponent 负责保证组件存在，这里负责拿到引用。
        /// </summary>
        private void CacheParts()
        {
            input = input != null ? input : GetComponent<PlayerInputReader>();
            motor = motor != null ? motor : GetComponent<PlayerMotor2D>();
            ground = ground != null ? ground : GetComponent<PlayerGroundDetector2D>();
            anim = anim != null ? anim : GetComponent<PlayerAnimatorBridge>();
        }

        /// <summary>
        /// 创建并注册玩家状态机中的所有状态。
        /// 后续新增 Dash、Attack、Hurt 时，只需要在这里注册新状态，并在对应状态中通过 PlayerStateId 切换。
        /// </summary>
        private void CreateStateMachine()
        {
            PlayerStateContext context = new PlayerStateContext(input, motor, ground, anim);
            fsm = new PlayerStateMachine();

            fsm.RegisterState(PlayerStateId.Grounded, new PlayerGroundedState(fsm, context));
            fsm.RegisterState(PlayerStateId.Airborne, new PlayerAirborneState(fsm, context));
        }

        /// <summary>
        /// 根据当前地面检测结果进入初始状态。
        /// 场景启动时角色可能已经在地面，也可能出生在空中，所以这里不能固定写死某一个状态。
        /// </summary>
        private void EnterInitialState()
        {
            PlayerStateId initialId = ground.IsGrounded
                ? PlayerStateId.Grounded
                : PlayerStateId.Airborne;

            fsm.ChangeState(initialId);
        }

        /// <summary>
        /// 把 PlayerConfig 注入给需要配置的子组件。
        /// Controller 是配置统一入口，其它组件只负责使用配置，不自己查找资源。
        /// </summary>
        private void ApplyConfig()
        {
            if (config == null)
                return;

            motor?.SetConfig(config);
            ground?.SetConfig(config);
            anim?.SetConfig(config);
        }
    }
}
