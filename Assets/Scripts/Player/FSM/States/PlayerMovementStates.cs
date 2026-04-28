using Game.Player.FSM;

namespace Game.Player.FSM.States
{
    /// <summary>
    /// 玩家落地状态，负责落地移动、起跳和离地切换。
    /// 后续新增 Dash、Attack 时，应优先在这里判断“落地状态是否允许进入新动作”。
    /// </summary>
    public sealed class PlayerGroundedState : PlayerState
    {
        /// <summary>
        /// 创建玩家落地状态。
        /// </summary>
        /// <param name="stateMachine">所属玩家状态机。</param>
        /// <param name="context">玩家状态上下文。</param>
        public PlayerGroundedState(PlayerStateMachine stateMachine, PlayerStateContext context)
            : base(stateMachine, context)
        {
        }

        /// <summary>
        /// 进入落地状态时调用。
        /// 落地后清理续跳状态，并同步 Animator 的落地参数。
        /// </summary>
        public override void Enter()
        {
            // 每次回到地面，都清掉可能残留的续跳和 Jump Trigger。
            Context.Motor.StopJumpHold();
            Context.Anim.SetGrounded(true);
            Context.Anim.ResetJump();
        }

        /// <summary>
        /// 每帧更新落地状态，处理离地和起跳。
        /// </summary>
        /// <param name="deltaTime">当前帧耗时。</param>
        public override void Tick(float deltaTime)
        {
            if (ShouldFall())
            {
                StateMachine.ChangeState(PlayerStateId.Airborne);
                return;
            }

            if (CanStartJump())
                StartJump();
        }

        /// <summary>
        /// 固定物理帧更新落地状态，处理横向移动和移动动画参数。
        /// </summary>
        /// <param name="fixedDeltaTime">固定物理帧耗时。</param>
        public override void FixedTick(float fixedDeltaTime)
        {
            // 落地状态下仍然可以左右移动。
            Context.Motor.MoveHorizontal(Context.Input.MoveX);
            Context.Anim.SetMovement(Context.Input.MoveX);
        }

        /// <summary>
        /// 判断角色是否应该从落地状态进入空中状态。
        /// 拆成方法是为了以后加入平台、击退、下落穿透等条件时不把 Tick 写乱。
        /// </summary>
        /// <returns>如果已经不在地面上，则返回 true。</returns>
        private bool ShouldFall()
        {
            return !Context.Ground.IsGrounded;
        }

        /// <summary>
        /// 判断当前是否可以开始普通跳跃。
        /// 后续加入攻击硬直、体力、跳跃缓冲时，可以集中扩展这个方法。
        /// </summary>
        /// <returns>如果当前帧按下跳跃键，则返回 true。</returns>
        private bool CanStartJump()
        {
            return Context.Input.WasJumpPressed;
        }

        /// <summary>
        /// 执行起跳逻辑，并切换到空中状态。
        /// 具体物理起跳交给 PlayerMotor2D，动画触发交给 PlayerAnimatorBridge。
        /// </summary>
        private void StartJump()
        {
            Context.Motor.StartJump();
            Context.Anim.SetGrounded(false);
            Context.Anim.PlayJump();
            StateMachine.ChangeState(PlayerStateId.Airborne);
        }
    }

    /// <summary>
    /// 玩家空中状态，负责空中移动、短跳、长按跳和落地切换。
    /// 后续新增二段跳、空中冲刺时，应优先从这个状态扩展进入条件。
    /// </summary>
    public sealed class PlayerAirborneState : PlayerState
    {
        /// <summary>
        /// 创建玩家空中状态。
        /// </summary>
        /// <param name="stateMachine">所属玩家状态机。</param>
        /// <param name="context">玩家状态上下文。</param>
        public PlayerAirborneState(PlayerStateMachine stateMachine, PlayerStateContext context)
            : base(stateMachine, context)
        {
        }

        /// <summary>
        /// 进入空中状态时调用。
        /// </summary>
        public override void Enter()
        {
            Context.Anim.SetGrounded(false);
        }

        /// <summary>
        /// 每帧更新空中状态，处理短跳和落地切换。
        /// </summary>
        /// <param name="deltaTime">当前帧耗时。</param>
        public override void Tick(float deltaTime)
        {
            if (ShouldCutJump())
                Context.Motor.ReleaseJump();

            if (ShouldLand())
                StateMachine.ChangeState(PlayerStateId.Grounded);
        }

        /// <summary>
        /// 固定物理帧更新空中状态，处理空中横向移动和长按跳。
        /// </summary>
        /// <param name="fixedDeltaTime">固定物理帧耗时。</param>
        public override void FixedTick(float fixedDeltaTime)
        {
            // 空中也允许横向控制，保留平台动作游戏常见的空中微调。
            Context.Motor.MoveHorizontal(Context.Input.MoveX);
            Context.Anim.SetMovement(Context.Input.MoveX);

            if (ShouldHoldJump())
                Context.Motor.ContinueJumpHold(fixedDeltaTime);
        }

        /// <summary>
        /// 判断是否应该执行短跳削减。
        /// </summary>
        /// <returns>如果当前帧松开跳跃键，则返回 true。</returns>
        private bool ShouldCutJump()
        {
            return Context.Input.WasJumpReleased;
        }

        /// <summary>
        /// 判断是否应该从空中状态回到落地状态。
        /// 竖直速度接近或小于 0 时才允许落地，避免起跳瞬间碰撞仍接触地面导致误切回落地。
        /// </summary>
        /// <returns>如果检测到地面且角色不再向上运动，则返回 true。</returns>
        private bool ShouldLand()
        {
            return Context.Ground.IsGrounded && Context.Motor.VerticalVelocity <= 0.05f;
        }

        /// <summary>
        /// 判断是否继续执行长按跳跃加力。
        /// </summary>
        /// <returns>如果跳跃键仍被按住，则返回 true。</returns>
        private bool ShouldHoldJump()
        {
            return Context.Input.JumpHeld;
        }
    }
}
