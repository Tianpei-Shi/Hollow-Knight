using Game.Player.Config;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Player.Movement
{
    /// <summary>
    /// 玩家 2D 地面检测器，负责判断角色当前是否站在地面上。
    /// 使用接触法线判断，可以避免角色侧面贴墙时被误判为落地。
    /// </summary>
    public class PlayerGroundDetector2D : MonoBehaviour
    {
        // 地面检测只关心“是否踩在地面”，不负责跳跃、不负责动画。

        /// <summary>
        /// 玩家配置数据，提供地面 LayerMask 和法线阈值。
        /// </summary>
        [Header("Config")]
        [Tooltip("通常由 PlayerController 自动注入；单独测试这个组件时也可以手动拖入。")]
        [SerializeField] private PlayerConfig config;

        /// <summary>
        /// 当前接触到的地面对象 ID 集合。
        /// HashSet 是 C# 集合类型，用来保存不重复的数据。
        /// </summary>
        private readonly HashSet<int> grounds = new HashSet<int>();

        /// <summary>
        /// 当前是否站在地面上。
        /// </summary>
        public bool IsGrounded { get; private set; }

        /// <summary>
        /// 落地状态变化事件。
        /// Action 是 C# 委托类型，这里表示可以通知外部“是否落地发生了变化”。
        /// </summary>
        public event Action<bool> GroundedChanged;

        /// <summary>
        /// 设置地面检测配置。
        /// </summary>
        /// <param name="nextConfig">新的玩家配置数据。</param>
        public void SetConfig(PlayerConfig nextConfig)
        {
            config = nextConfig;
        }

        /// <summary>
        /// Unity 2D 物理回调：开始碰撞时调用。
        /// </summary>
        /// <param name="collision">本次碰撞信息。</param>
        private void OnCollisionEnter2D(Collision2D collision)
        {
            UpdateCollision(collision);
        }

        /// <summary>
        /// Unity 2D 物理回调：持续碰撞时调用。
        /// </summary>
        /// <param name="collision">本次碰撞信息。</param>
        private void OnCollisionStay2D(Collision2D collision)
        {
            UpdateCollision(collision);
        }

        /// <summary>
        /// Unity 2D 物理回调：离开碰撞时调用。
        /// </summary>
        /// <param name="collision">离开的碰撞对象信息。</param>
        private void OnCollisionExit2D(Collision2D collision)
        {
            grounds.Remove(collision.gameObject.GetInstanceID());
            RefreshGrounded();
        }

        /// <summary>
        /// 根据碰撞对象 Layer 和接触法线刷新地面对象集合。
        /// </summary>
        /// <param name="collision">本次碰撞信息。</param>
        private void UpdateCollision(Collision2D collision)
        {
            int id = collision.gameObject.GetInstanceID();

            // 同时满足 Layer 和法线方向，才算真正踩在地面上。
            if (IsGroundLayer(collision.gameObject.layer) && HasGroundNormal(collision))
                grounds.Add(id);
            else
                grounds.Remove(id);

            RefreshGrounded();
        }

        /// <summary>
        /// 判断指定 Layer 是否属于地面 LayerMask。
        /// </summary>
        /// <param name="layer">待检测的 Unity Layer 编号。</param>
        /// <returns>如果该 Layer 被配置为地面，则返回 true。</returns>
        private bool IsGroundLayer(int layer)
        {
            return (GroundMask.value & (1 << layer)) != 0;
        }

        /// <summary>
        /// 判断本次碰撞中是否存在足够接近地面的法线。
        /// </summary>
        /// <param name="collision">本次碰撞信息。</param>
        /// <returns>如果存在符合阈值的接触法线，则返回 true。</returns>
        private bool HasGroundNormal(Collision2D collision)
        {
            for (int i = 0; i < collision.contactCount; i++)
            {
                // normal.y 接近 1 表示碰撞面朝上；墙面法线通常主要在 X 方向。
                if (collision.GetContact(i).normal.y >= MinGroundNormalY)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 根据当前地面对象集合刷新 IsGrounded，并在状态变化时触发事件。
        /// </summary>
        private void RefreshGrounded()
        {
            bool next = grounds.Count > 0;
            if (IsGrounded == next)
                return;

            // 只有状态真的变化时才触发事件，减少不必要的重复通知。
            IsGrounded = next;
            GroundedChanged?.Invoke(IsGrounded);
        }

        private LayerMask GroundMask => config != null ? config.GroundMask : ~0;
        private float MinGroundNormalY => config != null ? config.MinGroundNormalY : 0.7f;
    }
}
