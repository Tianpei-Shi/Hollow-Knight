using Game.Player.Config;
using UnityEngine;

namespace Game.Player.Movement
{
    /// <summary>
    /// 玩家 2D 运动执行器，负责修改 Rigidbody2D 的速度、跳跃力和角色朝向。
    /// 运动层不读取输入、不判断状态，只提供运动能力给状态机调用。
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerMotor2D : MonoBehaviour
    {
        /// <summary>
        /// 玩家配置数据，通常由 PlayerController 自动注入。
        /// </summary>
        [Header("Config")]
        [Tooltip("通常由 PlayerController 自动注入；单独测试这个组件时也可以手动拖入。")]
        [SerializeField] private PlayerConfig config;

        /// <summary>
        /// 当前对象上的 Rigidbody2D 组件，用于控制 2D 物理运动。
        /// </summary>
        private Rigidbody2D body;

        /// <summary>
        /// 角色初始缩放，用于翻转朝向时保留原始大小。
        /// </summary>
        private Vector3 baseScale;

        /// <summary>
        /// 当前还剩多少长按续跳时间。
        /// </summary>
        private float holdTimer;

        /// <summary>
        /// 当前是否允许继续长按续跳。
        /// </summary>
        private bool canHold;

        /// <summary>
        /// 当前竖直速度，状态机用它判断是否可以从空中回到落地状态。
        /// </summary>
        public float VerticalVelocity => body.linearVelocity.y;

        /// <summary>
        /// 设置运动配置。
        /// </summary>
        /// <param name="nextConfig">新的玩家配置数据。</param>
        public void SetConfig(PlayerConfig nextConfig)
        {
            config = nextConfig;
        }

        /// <summary>
        /// Unity 生命周期方法：对象加载时调用。
        /// 这里缓存 Rigidbody2D 和初始缩放。
        /// </summary>
        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            baseScale = transform.localScale;
        }

        /// <summary>
        /// 根据横向输入设置角色水平速度。
        /// </summary>
        /// <param name="moveX">横向输入值，通常为 -1、0、1。</param>
        public void MoveHorizontal(float moveX)
        {
            // 只控制 X 速度，Y 速度留给重力和跳跃。
            body.linearVelocity = new Vector2(moveX * MoveSpeed, body.linearVelocity.y);
            UpdateFacing(moveX);
        }

        /// <summary>
        /// 开始跳跃，直接设置向上速度并开启长按续跳计时。
        /// </summary>
        public void StartJump()
        {
            // 起跳时直接设置向上速度，让按下跳跃的反馈更干脆。
            canHold = true;
            holdTimer = MaxJumpHoldTime;
            body.linearVelocity = new Vector2(body.linearVelocity.x, JumpForce);
        }

        /// <summary>
        /// 继续长按跳跃，在限定时间内持续施加向上力。
        /// </summary>
        /// <param name="deltaTime">本次物理帧的时间间隔。</param>
        public void ContinueJumpHold(float deltaTime)
        {
            if (!canHold)
                return;

            // 长按时间耗尽后，即使继续按住跳跃键，也不再额外加高度。
            if (holdTimer <= 0f)
            {
                canHold = false;
                return;
            }

            // 在有限时间内持续给向上力，实现长按跳更高。
            body.AddForce(Vector2.up * JumpHoldForce, ForceMode2D.Force);
            holdTimer -= deltaTime;
        }

        /// <summary>
        /// 释放跳跃键。如果角色仍在上升，则削减上升速度，形成短跳效果。
        /// </summary>
        public void ReleaseJump()
        {
            canHold = false;

            // 松开时如果还在上升，就削减上升速度，形成短按跳更矮。
            if (body.linearVelocity.y > 0f)
                body.linearVelocity = new Vector2(body.linearVelocity.x, body.linearVelocity.y * JumpCutMultiplier);
        }

        /// <summary>
        /// 停止长按跳跃逻辑，通常在落地时调用，避免续跳状态残留。
        /// </summary>
        public void StopJumpHold()
        {
            canHold = false;
            holdTimer = 0f;
        }

        /// <summary>
        /// 根据移动方向更新角色朝向。
        /// </summary>
        /// <param name="moveX">横向输入值。</param>
        private void UpdateFacing(float moveX)
        {
            if (Mathf.Abs(moveX) <= MoveDeadZone)
                return;

            // 这个素材默认朝左，所以配置为 false 时，向右移动需要反转 X 缩放。
            float xSign = moveX > 0f ? 1f : -1f;
            if (!SpriteFacesRightByDefault)
                xSign *= -1f;

            transform.localScale = new Vector3(Mathf.Abs(baseScale.x) * xSign, baseScale.y, baseScale.z);
        }

        // 没有拖配置时使用默认值，保证组件单独挂上也能运行，方便调试。
        private float MoveSpeed => config != null ? config.MoveSpeed : 10f;
        private float MoveDeadZone => config != null ? config.MoveDeadZone : 0.01f;
        private bool SpriteFacesRightByDefault => config != null && config.SpriteFacesRightByDefault;
        private float JumpForce => config != null ? config.JumpForce : 5f;
        private float JumpHoldForce => config != null ? config.JumpHoldForce : 10f;
        private float MaxJumpHoldTime => config != null ? config.MaxJumpHoldTime : 0.2f;
        private float JumpCutMultiplier => config != null ? config.JumpCutMultiplier : 0.5f;
    }
}
