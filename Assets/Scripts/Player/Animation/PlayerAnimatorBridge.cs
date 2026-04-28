using Game.Player.Config;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Player.Animation
{
    /// <summary>
    /// 玩家动画桥接器，负责把角色逻辑状态转换为 Animator 参数。
    /// FSM 不直接写 Animator 参数名，避免以后改动画控制器时到处找字符串。
    /// </summary>
    public class PlayerAnimatorBridge : MonoBehaviour
    {
        /// <summary>
        /// 玩家配置数据，提供 Animator 参数名和移动死区。
        /// </summary>
        [Header("Config")]
        [Tooltip("通常由 PlayerController 自动注入；单独测试这个组件时也可以手动拖入。")]
        [SerializeField] private PlayerConfig config;

        /// <summary>
        /// Unity Animator 组件，负责播放动画状态机。
        /// </summary>
        [Header("Animator")]
        [FormerlySerializedAs("animator")]
        [SerializeField] private Animator anim;

        /// <summary>
        /// 设置动画配置。
        /// </summary>
        /// <param name="nextConfig">新的玩家配置数据。</param>
        public void SetConfig(PlayerConfig nextConfig)
        {
            config = nextConfig;
        }

        /// <summary>
        /// Unity 生命周期方法：对象加载时调用。
        /// 这里自动获取同一对象上的 Animator。
        /// </summary>
        private void Awake()
        {
            anim = anim != null ? anim : GetComponent<Animator>();
        }

        /// <summary>
        /// 根据横向输入设置移动动画参数。
        /// </summary>
        /// <param name="moveX">横向输入值。</param>
        public void SetMovement(float moveX)
        {
            if (anim == null)
                return;

            // Animator 里使用 int：-1 左移，0 静止，1 右移。
            int movement = 0;
            if (moveX > MoveDeadZone)
                movement = 1;
            else if (moveX < -MoveDeadZone)
                movement = -1;

            anim.SetInteger(MovementParameter, movement);
        }

        /// <summary>
        /// 设置角色是否落地的动画参数。
        /// </summary>
        /// <param name="isGrounded">是否处于地面。</param>
        public void SetGrounded(bool isGrounded)
        {
            if (anim == null)
                return;

            anim.SetBool(IsGroundedParameter, isGrounded);
        }

        /// <summary>
        /// 播放跳跃动画 Trigger。
        /// Trigger 是 Animator 中的一次性触发参数，适合起跳、攻击这类瞬间动作。
        /// </summary>
        public void PlayJump()
        {
            if (anim == null)
                return;

            // 先 Reset 再 Set，避免上一帧残留的 Trigger 干扰这次起跳。
            anim.ResetTrigger(JumpTrigger);
            anim.SetTrigger(JumpTrigger);
        }

        /// <summary>
        /// 重置跳跃 Trigger，避免 Trigger 残留影响下一次动画切换。
        /// </summary>
        public void ResetJump()
        {
            if (anim == null)
                return;

            anim.ResetTrigger(JumpTrigger);
        }

        private float MoveDeadZone => config != null ? config.MoveDeadZone : 0.01f;
        private string MovementParameter => config != null ? config.MovementParameter : "Movement";
        private string IsGroundedParameter => config != null ? config.IsGroundedParameter : "IsOnGround";
        private string JumpTrigger => config != null ? config.JumpTrigger : "Jump";
    }
}
