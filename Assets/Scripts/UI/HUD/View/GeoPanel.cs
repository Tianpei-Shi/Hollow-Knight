using Game.UI.HUD.Config;
using TMPro;
using UnityEngine;

namespace Game.UI.HUD.View
{
    /// <summary>
    /// Geo HUD 视图层，负责金币（Geo）文本刷新和动画触发。
    /// 该类只处理 UI 表现，不负责货币计算规则。
    /// </summary>
    public class GeoPanel : MonoBehaviour
    {
        /// <summary>
        /// Geo 数值文本。
        /// </summary>
        [Header("UI References")]
        [Tooltip("Geo 数值文本（TextMeshProUGUI）。")]
        [SerializeField] private TextMeshProUGUI geoText;

        /// <summary>
        /// Geo HUD 的 Animator。用于播放加钱/花钱反馈。
        /// </summary>
        [Tooltip("Geo HUD 的 Animator，用于播放 Gain/Spend 等动画。")]
        [SerializeField] private Animator geoAnimator;

        /// <summary>
        /// HUD 动画参数配置资产。
        /// </summary>
        [Header("Config")]
        [Tooltip("HUD 动画参数配置资产。")]
        [SerializeField] private HudAnimatorConfig animatorConfig;

        /// <summary>
        /// Unity 生命周期方法：对象加载时调用。
        /// 自动补齐 Animator 引用，减少漏拖风险。
        /// </summary>
        private void Awake()
        {
            if (geoAnimator == null)
            {
                // GetComponent<T>() 是 Unity 的泛型组件查找方法。
                // 这里用于自动获取同一个 GameObject 上的 Animator，减少 Inspector 漏拖引用的问题。
                geoAnimator = GetComponent<Animator>();
            }
        }

        /// <summary>
        /// 刷新 Geo 显示并根据变化量触发动画。
        /// </summary>
        /// <param name="currentGeo">当前 Geo 总量。</param>
        /// <param name="delta">本次变化量，正数为获得，负数为消耗，0 为初始化同步。</param>
        public void RenderGeo(int currentGeo, int delta)
        {
            UpdateGeoText(currentGeo);
            PlayGeoDeltaAnimation(delta);
        }

        /// <summary>
        /// 更新 Geo 文本。
        /// </summary>
        /// <param name="currentGeo">当前 Geo 总量。</param>
        private void UpdateGeoText(int currentGeo)
        {
            if (geoText == null)
                return;

            geoText.text = currentGeo.ToString();
        }

        /// <summary>
        /// 根据变化量触发 Geo 动画。
        /// </summary>
        /// <param name="delta">本次变化量。</param>
        private void PlayGeoDeltaAnimation(int delta)
        {
            if (delta > 0)
            {
                TriggerIfAvailable(geoAnimator, GetGeoGainTrigger());
                return;
            }

            if (delta < 0)
                TriggerIfAvailable(geoAnimator, GetGeoSpendTrigger());
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

            animator.ResetTrigger(triggerName);
            animator.SetTrigger(triggerName);
        }

        /// <summary>
        /// 获取 Geo 增加动画的 Trigger 参数名。
        /// 如果配置资产存在，就读取配置；如果没有配置资产，就使用默认值。
        /// </summary>
        /// <returns>Animator 中用于播放 Geo 增加动画的 Trigger 参数名。</returns>
        private string GetGeoGainTrigger()
        {
            if (animatorConfig != null)
            {
                return animatorConfig.GeoGainTrigger;
            }

            return "Gain";
        }

        /// <summary>
        /// 获取 Geo 消耗动画的 Trigger 参数名。
        /// </summary>
        /// <returns>Animator 中用于播放 Geo 消耗动画的 Trigger 参数名。</returns>
        private string GetGeoSpendTrigger()
        {
            if (animatorConfig != null)
            {
                return animatorConfig.GeoSpendTrigger;
            }

            return "Spend";
        }
    }
}
