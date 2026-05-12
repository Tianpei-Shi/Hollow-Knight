using Game.UI.HUD.Config;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.HUD.View
{
    /// <summary>
    /// 血量 HUD 视图层，负责血量文本/图像刷新和血量相关动画触发。
    /// 这个类只做“表现”，不保存战斗规则，不判断是否应该扣血。
    /// </summary>
    public class HealthPanel : MonoBehaviour
    {
        /// <summary>
        /// 血量图标（例如面具）容器。常见做法是启用前 N 个图标表示当前血量。
        /// </summary>
        [Header("UI References")]
        [Tooltip("血量图标数组，索引小于当前血量的图标会显示。")]
        [SerializeField] private Image[] healthIcons;

        /// <summary>
        /// 可选的血量文本（例如 3/5）。如果没有该需求，可以不赋值。
        /// TextMeshProUGUI 来自 TMPro 命名空间，是 Unity 主流的高质量文本组件。
        /// </summary>
        [Tooltip("可选：显示当前血量/最大血量的文本。")]
        [SerializeField] private TextMeshProUGUI healthText;

        /// <summary>
        /// 血量 HUD 的 Animator。
        /// Animator 用于基于状态机和参数播放 UI 动画。
        /// </summary>
        [Tooltip("血量 HUD 的 Animator，用于播放受击/隐藏/复显动画。")]
        [SerializeField] private Animator healthAnimator;

        /// <summary>
        /// HUD 动画参数配置资产。
        /// </summary>
        [Header("Config")]
        [Tooltip("HUD 动画参数配置资产。")]
        [SerializeField] private HudAnimatorConfig animatorConfig;

        /// <summary>
        /// 上一次记录的当前血量，用于比较是掉血还是回血。
        /// </summary>
        private int lastHealth = -1;

        /// <summary>
        /// Unity 生命周期方法：对象加载时调用。
        /// 这里做引用自动补齐，减少 Inspector 漏拖导致的空引用。
        /// </summary>
        private void Awake()
        {
            if (healthAnimator == null)
            {
                // GetComponent<T>() 是 Unity 提供的泛型方法，用来获取同一个 GameObject 上的组件。
                // 这里使用它，是为了在 Inspector 忘记拖拽 Animator 时自动补齐引用。
                healthAnimator = GetComponent<Animator>();
            }
        }

        /// <summary>
        /// 刷新血量显示。
        /// </summary>
        /// <param name="currentHealth">当前血量。</param>
        /// <param name="maxHealth">最大血量。</param>
        public void RenderHealth(int currentHealth, int maxHealth)
        {
            int safeMax = Mathf.Max(1, maxHealth);
            int safeCurrent = Mathf.Clamp(currentHealth, 0, safeMax);

            UpdateHealthIcons(safeCurrent, safeMax);
            UpdateHealthText(safeCurrent, safeMax);
            PlayHealthDeltaAnimation(safeCurrent);

            lastHealth = safeCurrent;
        }

        /// <summary>
        /// 播放 HUD 隐藏动画。
        /// 例如过场或无 UI 镜头时可调用。
        /// </summary>
        public void PlayHide()
        {
            TriggerIfAvailable(healthAnimator, GetHealthHideTrigger());
        }

        /// <summary>
        /// 播放 HUD 复显动画。
        /// </summary>
        public void PlayRespawn()
        {
            TriggerIfAvailable(healthAnimator, GetHealthRespawnTrigger());
        }

        /// <summary>
        /// 根据当前血量数量刷新图标显隐。
        /// </summary>
        /// <param name="currentHealth">当前血量。</param>
        /// <param name="maxHealth">最大血量。</param>
        private void UpdateHealthIcons(int currentHealth, int maxHealth)
        {
            if (healthIcons == null || healthIcons.Length == 0)
                return;

            // 商业项目常用做法：图标数组长度和最大血量允许不一致，运行时按最小边界显示。
            int iconCount = healthIcons.Length;
            int visibleCount = Mathf.Min(currentHealth, iconCount);
            int activeWindow = Mathf.Min(maxHealth, iconCount);

            for (int i = 0; i < iconCount; i++)
            {
                if (healthIcons[i] == null)
                    continue;

                bool shouldBeVisible = i < visibleCount && i < activeWindow;
                healthIcons[i].enabled = shouldBeVisible;
            }
        }

        /// <summary>
        /// 更新血量文本显示。
        /// </summary>
        /// <param name="currentHealth">当前血量。</param>
        /// <param name="maxHealth">最大血量。</param>
        private void UpdateHealthText(int currentHealth, int maxHealth)
        {
            if (healthText == null)
                return;

            healthText.text = $"{currentHealth}/{maxHealth}";
        }

        /// <summary>
        /// 根据血量变化方向播放动画。
        /// 当前实现：只在掉血时触发 Hurt。
        /// </summary>
        /// <param name="currentHealth">当前血量。</param>
        private void PlayHealthDeltaAnimation(int currentHealth)
        {
            if (lastHealth < 0)
                return;

            // 掉血时触发受击动画，回血动画可后续按项目风格新增参数。
            if (currentHealth < lastHealth)
                TriggerIfAvailable(healthAnimator, GetHealthHurtTrigger());
        }

        /// <summary>
        /// 安全触发 Animator Trigger 参数。
        /// </summary>
        /// <param name="animator">目标 Animator。</param>
        /// <param name="triggerName">Trigger 参数名。</param>
        private static void TriggerIfAvailable(Animator animator, string triggerName)
        {
            if (animator == null || string.IsNullOrWhiteSpace(triggerName))
                return;

            // 先 Reset 再 Set，可减少高频触发时 Trigger 残留导致的不稳定切换。
            animator.ResetTrigger(triggerName);
            animator.SetTrigger(triggerName);
        }

        /// <summary>
        /// 获取血量受击动画的 Trigger 参数名。
        /// 如果配置资产存在，就优先读取配置；如果没有配置资产，就使用代码里的默认值。
        /// </summary>
        /// <returns>Animator 中用于播放受击动画的 Trigger 参数名。</returns>
        private string GetHealthHurtTrigger()
        {
            if (animatorConfig != null)
            {
                return animatorConfig.HealthHurtTrigger;
            }

            return "Hurt";
        }

        /// <summary>
        /// 获取血量 HUD 隐藏动画的 Trigger 参数名。
        /// </summary>
        /// <returns>Animator 中用于播放隐藏动画的 Trigger 参数名。</returns>
        private string GetHealthHideTrigger()
        {
            if (animatorConfig != null)
            {
                return animatorConfig.HealthHideTrigger;
            }

            return "Hide";
        }

        /// <summary>
        /// 获取血量 HUD 重新显示动画的 Trigger 参数名。
        /// </summary>
        /// <returns>Animator 中用于播放复显动画的 Trigger 参数名。</returns>
        private string GetHealthRespawnTrigger()
        {
            if (animatorConfig != null)
            {
                return animatorConfig.HealthRespawnTrigger;
            }

            return "ResPawn";
        }
    }
}
