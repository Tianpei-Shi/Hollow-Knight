using UnityEngine;

namespace Game.Player.Config
{
    /// <summary>
    /// 玩家角色配置数据，保存移动、跳跃、地面检测和动画参数名。
    /// ScriptableObject 是 Unity 的资源型数据对象，适合存放可在 Inspector 中调节的设计参数。
    /// </summary>
    [CreateAssetMenu(
        fileName = "PlayerConfig",
        menuName = "Hollow Knight/Player/Player Config")]
    public class PlayerConfig : ScriptableObject
    {
        // 这个 ScriptableObject 只保存“角色设计参数”，不保存运行时状态。
        // 例如当前是否在地面、当前输入值、当前状态机状态，都应该留在运行时脚本里。

        /// <summary>
        /// 角色横向移动速度。
        /// </summary>
        [Header("Move")]
        [Tooltip("角色横向移动速度。")]
        [SerializeField] private float moveSpeed = 10f;

        /// <summary>
        /// 横向输入死区，小于该值的输入会被当作 0。
        /// </summary>
        [Tooltip("小于这个值的横向输入会被当作 0，避免手柄漂移导致角色轻微移动。")]
        [SerializeField] private float moveDeadZone = 0.01f;

        /// <summary>
        /// 角色美术素材默认是否面朝右。
        /// </summary>
        [Tooltip("勾选表示美术素材默认面朝右；不勾选表示默认面朝左。")]
        [SerializeField] private bool spriteFacesRightByDefault = false;

        /// <summary>
        /// 起跳瞬间设置的向上速度。
        /// </summary>
        [Header("Jump")]
        [Tooltip("按下跳跃瞬间设置的向上速度。")]
        [SerializeField] private float jumpForce = 5f;

        /// <summary>
        /// 按住跳跃时持续施加的向上力，用于长按跳更高。
        /// </summary>
        [Tooltip("按住跳跃时持续施加的向上力，用于长按跳更高。")]
        [SerializeField] private float jumpHoldForce = 10f;

        /// <summary>
        /// 允许长按续跳的最长时间。
        /// </summary>
        [Tooltip("允许长按续跳的最长时间。")]
        [SerializeField] private float maxJumpHoldTime = 0.2f;

        /// <summary>
        /// 松开跳跃时削减上升速度的倍率。
        /// Range 是 Unity Attribute，会把 Inspector 字段显示成滑条。
        /// </summary>
        [Tooltip("松开跳跃时削减上升速度的倍率，值越小短跳越明显。")]
        [SerializeField, Range(0f, 1f)] private float jumpCutMultiplier = 0.5f;

        /// <summary>
        /// 哪些 Layer 会被当作地面。
        /// </summary>
        [Header("Ground Check")]
        [Tooltip("哪些 Layer 会被当作地面。")]
        [SerializeField] private LayerMask groundMask = ~0;

        /// <summary>
        /// 碰撞法线 Y 分量达到这个值才算踩在地面上。
        /// </summary>
        [Tooltip("碰撞法线 Y 分量达到这个值才算踩在地面上，避免墙面被误判为地面。")]
        [SerializeField, Range(0f, 1f)] private float minGroundNormalY = 0.7f;

        /// <summary>
        /// Animator 中控制左右移动的 int 参数名。
        /// </summary>
        [Header("Animator Parameters")]
        [Tooltip("Animator 中控制左右移动的 int 参数名。")]
        [SerializeField] private string movementParameter = "Movement";

        /// <summary>
        /// Animator 中表示是否落地的 bool 参数名。
        /// </summary>
        [Tooltip("Animator 中表示是否落地的 bool 参数名。")]
        [SerializeField] private string isGroundedParameter = "IsOnGround";

        /// <summary>
        /// Animator 中播放起跳动画的 trigger 参数名。
        /// </summary>
        [Tooltip("Animator 中播放起跳动画的 trigger 参数名。")]
        [SerializeField] private string jumpTrigger = "Jump";

        public float MoveSpeed => moveSpeed;
        public float MoveDeadZone => moveDeadZone;
        public bool SpriteFacesRightByDefault => spriteFacesRightByDefault;
        public float JumpForce => jumpForce;
        public float JumpHoldForce => jumpHoldForce;
        public float MaxJumpHoldTime => maxJumpHoldTime;
        public float JumpCutMultiplier => jumpCutMultiplier;
        public LayerMask GroundMask => groundMask;
        public float MinGroundNormalY => minGroundNormalY;
        public string MovementParameter => movementParameter;
        public string IsGroundedParameter => isGroundedParameter;
        public string JumpTrigger => jumpTrigger;
    }
}
