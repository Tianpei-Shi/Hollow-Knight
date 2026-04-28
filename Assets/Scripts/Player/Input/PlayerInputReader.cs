using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace Game.Player.Input
{
    /// <summary>
    /// 玩家输入读取器，负责把 Unity New Input System 的 Action 转成角色状态机能读取的数据。
    /// 输入层只采集输入，不直接执行移动、跳跃或动画逻辑。
    /// </summary>
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerInputReader : MonoBehaviour
    {
        // 后续新增攻击、冲刺时，优先在这里增加对应的输入属性。

        /// <summary>
        /// 横向移动 Action 的名字，必须和 Input Actions 资源中的 Action 名一致。
        /// FormerlySerializedAs 是 Unity 序列化兼容 Attribute，用来保留旧字段名上的 Inspector 数据。
        /// </summary>
        [Header("Input Action Names")]
        [FormerlySerializedAs("moveActionName")]
        [SerializeField] private string moveName = "Move";

        /// <summary>
        /// 跳跃 Action 的名字，必须和 Input Actions 资源中的 Action 名一致。
        /// </summary>
        [FormerlySerializedAs("jumpActionName")]
        [SerializeField] private string jumpName = "Jump";

        /// <summary>
        /// PlayerInput 是 New Input System 的玩家输入组件，持有 .inputactions 资源。
        /// </summary>
        private PlayerInput input;

        /// <summary>
        /// 横向移动输入 Action。
        /// </summary>
        private InputAction moveAction;

        /// <summary>
        /// 跳跃输入 Action。
        /// </summary>
        private InputAction jumpAction;

        /// <summary>
        /// 当前横向输入值，通常为 -1、0、1。
        /// </summary>
        public float MoveX { get; private set; }

        /// <summary>
        /// 跳跃键当前是否正在按住。
        /// </summary>
        public bool JumpHeld { get; private set; }

        /// <summary>
        /// 当前帧是否刚按下跳跃键。
        /// </summary>
        public bool WasJumpPressed { get; private set; }

        /// <summary>
        /// 当前帧是否刚松开跳跃键。
        /// </summary>
        public bool WasJumpReleased { get; private set; }

        /// <summary>
        /// Unity 生命周期方法：对象加载时调用。
        /// 这里缓存 PlayerInput 并绑定 Action 引用。
        /// </summary>
        private void Awake()
        {
            input = GetComponent<PlayerInput>();
            BindActions();
        }

        /// <summary>
        /// Unity 生命周期方法：组件启用时调用。
        /// 这里订阅输入事件并启用对应 Action。
        /// </summary>
        private void OnEnable()
        {
            if (input == null)
                input = GetComponent<PlayerInput>();

            // PlayerInput 持有 .inputactions 资源，这里按名字找到具体 Action。
            BindActions();
            Subscribe();
            moveAction?.Enable();
            jumpAction?.Enable();
        }

        /// <summary>
        /// Unity 生命周期方法：组件禁用时调用。
        /// 这里取消订阅事件，并清空输入状态，避免旧输入残留。
        /// </summary>
        private void OnDisable()
        {
            Unsubscribe();
            MoveX = 0f;
            JumpHeld = false;
            ClearFrameActions();
        }

        /// <summary>
        /// 清空当前帧的一次性输入事件。
        /// </summary>
        public void ClearFrameActions()
        {
            // “刚按下/刚松开”是一次性事件，只允许 FSM 在当前帧消费一次。
            WasJumpPressed = false;
            WasJumpReleased = false;
        }

        /// <summary>
        /// 从 PlayerInput 的 actions 中按名字查找具体 InputAction。
        /// </summary>
        private void BindActions()
        {
            if (input == null || input.actions == null)
                return;

            moveAction = input.actions.FindAction(moveName, false);
            jumpAction = input.actions.FindAction(jumpName, false);
        }

        /// <summary>
        /// 订阅输入事件。
        /// </summary>
        private void Subscribe()
        {
            // 直接订阅 InputAction 事件，不依赖 PlayerInput 的 Send Messages 方法命名规则。
            if (moveAction != null)
            {
                moveAction.performed += OnMove;
                moveAction.canceled += OnMove;
            }

            if (jumpAction != null)
            {
                jumpAction.started += OnJump;
                jumpAction.performed += OnJump;
                jumpAction.canceled += OnJump;
            }
        }

        /// <summary>
        /// 取消订阅输入事件，避免组件禁用后仍然收到回调。
        /// </summary>
        private void Unsubscribe()
        {
            if (moveAction != null)
            {
                moveAction.performed -= OnMove;
                moveAction.canceled -= OnMove;
            }

            if (jumpAction != null)
            {
                jumpAction.started -= OnJump;
                jumpAction.performed -= OnJump;
                jumpAction.canceled -= OnJump;
            }
        }

        /// <summary>
        /// 处理移动输入回调。
        /// </summary>
        /// <param name="context">输入系统传入的 Action 回调上下文。</param>
        private void OnMove(InputAction.CallbackContext context)
        {
            // Move 是 Axis：A/Left 应该读到 -1，D/Right 应该读到 1。
            MoveX = context.canceled ? 0f : context.ReadValue<float>();
        }

        /// <summary>
        /// 处理跳跃输入回调。
        /// </summary>
        /// <param name="context">输入系统传入的 Action 回调上下文。</param>
        private void OnJump(InputAction.CallbackContext context)
        {
            // canceled 表示按钮松开，用它结束长按跳并产生一帧释放事件。
            if (context.canceled)
            {
                if (JumpHeld)
                    WasJumpReleased = true;

                JumpHeld = false;
                return;
            }

            // started/performed 都可能到达；只有从未按住变成按住时才算“刚按下”。
            if (!JumpHeld)
                WasJumpPressed = true;

            JumpHeld = true;
        }
    }
}
