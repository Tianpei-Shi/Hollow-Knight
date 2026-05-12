using UnityEngine;

namespace Game.UI.HUD.Config
{
    /// <summary>
    /// HUD 动画参数配置资产，集中维护 Animator 参数名，避免硬编码字符串散落在各个脚本里。
    /// ScriptableObject 适合商业项目做“可调配置”，UI 动画参数改名时不需要改代码。
    /// </summary>
    [CreateAssetMenu(fileName = "HudAnimatorConfig", menuName = "Hollow Knight/UI/HUD Animator Config")]
    public class HudAnimatorConfig : ScriptableObject
    {
        /// <summary>
        /// 血量 HUD 受击触发器参数名，对应 Health.controller 里的 Trigger。
        /// </summary>
        [Header("Health Animator Parameters")]
        [Tooltip("血量 HUD 受击触发器参数名。")]
        [SerializeField] private string healthHurtTrigger = "Hurt";

        /// <summary>
        /// 血量 HUD 隐藏触发器参数名。
        /// </summary>
        [Tooltip("血量 HUD 隐藏触发器参数名。")]
        [SerializeField] private string healthHideTrigger = "Hide";

        /// <summary>
        /// 血量 HUD 重新显示触发器参数名。
        /// 注意你当前动画资源里参数名是 ResPawn（不是 Respawn）。
        /// </summary>
        [Tooltip("血量 HUD 重新显示触发器参数名。")]
        [SerializeField] private string healthRespawnTrigger = "ResPawn";

        /// <summary>
        /// Geo HUD 增加时触发器参数名。
        /// 你当前 Geo.controller 还没有参数，后续可在 Animator 里添加同名 Trigger。
        /// </summary>
        [Header("Geo Animator Parameters")]
        [Tooltip("Geo HUD 增加时触发器参数名。")]
        [SerializeField] private string geoGainTrigger = "Gain";

        /// <summary>
        /// Geo HUD 消耗时触发器参数名。
        /// </summary>
        [Tooltip("Geo HUD 消耗时触发器参数名。")]
        [SerializeField] private string geoSpendTrigger = "Spend";

        /// <summary>
        /// 血量 HUD 受击 Trigger 参数名。
        /// </summary>
        public string HealthHurtTrigger => healthHurtTrigger;

        /// <summary>
        /// 血量 HUD 隐藏 Trigger 参数名。
        /// </summary>
        public string HealthHideTrigger => healthHideTrigger;

        /// <summary>
        /// 血量 HUD 复显 Trigger 参数名。
        /// </summary>
        public string HealthRespawnTrigger => healthRespawnTrigger;

        /// <summary>
        /// Geo HUD 获得 Trigger 参数名。
        /// </summary>
        public string GeoGainTrigger => geoGainTrigger;

        /// <summary>
        /// Geo HUD 消耗 Trigger 参数名。
        /// </summary>
        public string GeoSpendTrigger => geoSpendTrigger;
    }
}
