using System;
using UnityEngine;

namespace Game.UI.HUD.Model
{
    /// <summary>
    /// HUD 数据源，负责维护玩家 HUD 相关的运行时数据（血量、最大血量、Geo）。
    /// 商业项目中通常把数据层与表现层分离，避免 UI 脚本直接持有游戏规则。
    /// </summary>
    public class PlayerHudDataSource : MonoBehaviour
    {
        /// <summary>
        /// 玩家最大血量。通过 Inspector 方便调试起始数值。
        /// </summary>
        [Header("Initial Data")]
        [Tooltip("玩家初始最大血量。")]
        [SerializeField] private int maxHealth = 5;

        /// <summary>
        /// 玩家初始当前血量。Awake 会自动夹在 0~maxHealth 之间。
        /// </summary>
        [Tooltip("玩家初始当前血量。")]
        [SerializeField] private int currentHealth = 5;

        /// <summary>
        /// 玩家初始 Geo（类似金币）。
        /// </summary>
        [Tooltip("玩家初始 Geo（类似金币）。")]
        [SerializeField] private int geo = 0;

        /// <summary>
        /// 血量变化事件。
        /// 第一个参数是当前血量，第二个参数是最大血量。
        /// C# event 是“只允许外部订阅/退订，不允许外部直接触发”的安全事件语法。
        /// </summary>
        public event Action<int, int> HealthChanged;

        /// <summary>
        /// Geo 变化事件。
        /// 第一个参数是当前 Geo，第二个参数是这次变化量（可正可负）。
        /// </summary>
        public event Action<int, int> GeoChanged;

        /// <summary>
        /// 当前血量只读属性。
        /// </summary>
        public int CurrentHealth => currentHealth;

        /// <summary>
        /// 最大血量只读属性。
        /// </summary>
        public int MaxHealth => maxHealth;

        /// <summary>
        /// 当前 Geo 只读属性。
        /// </summary>
        public int Geo => geo;

        /// <summary>
        /// Unity 生命周期方法：对象加载时调用。
        /// 这里做数据防御性校验，避免 Inspector 误填导致运行时越界。
        /// </summary>
        private void Awake()
        {
            maxHealth = Mathf.Max(1, maxHealth);
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
            geo = Mathf.Max(0, geo);
        }

        /// <summary>
        /// 向所有监听者广播一次完整当前状态。
        /// 场景初始化或 UI 重新启用时，Presenter 可以主动调用它完成同步。
        /// </summary>
        public void PublishAll()
        {
            HealthChanged?.Invoke(currentHealth, maxHealth);
            GeoChanged?.Invoke(geo, 0);
        }

        /// <summary>
        /// 设置最大血量。
        /// </summary>
        /// <param name="newMaxHealth">新的最大血量，最小为 1。</param>
        /// <param name="keepHealthRatio">是否按比例保留当前血量。</param>
        public void SetMaxHealth(int newMaxHealth, bool keepHealthRatio)
        {
            int safeNewMax = Mathf.Max(1, newMaxHealth);
            if (safeNewMax == maxHealth)
                return;

            int oldMax = maxHealth;
            maxHealth = safeNewMax;

            if (keepHealthRatio)
            {
                // 按比例迁移当前血量，避免升降上限时体感突兀。
                float ratio = oldMax > 0 ? (float)currentHealth / oldMax : 1f;
                currentHealth = Mathf.Clamp(Mathf.RoundToInt(ratio * maxHealth), 0, maxHealth);
            }
            else
            {
                // 不保留比例时，只做边界收敛。
                currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
            }

            HealthChanged?.Invoke(currentHealth, maxHealth);
        }

        /// <summary>
        /// 直接设置当前血量。
        /// </summary>
        /// <param name="newHealth">目标血量，会自动夹到 0~maxHealth。</param>
        public void SetHealth(int newHealth)
        {
            int clamped = Mathf.Clamp(newHealth, 0, maxHealth);
            if (clamped == currentHealth)
                return;

            currentHealth = clamped;
            HealthChanged?.Invoke(currentHealth, maxHealth);
        }

        /// <summary>
        /// 扣血。
        /// </summary>
        /// <param name="amount">扣血量，非正数会被忽略。</param>
        public void Damage(int amount)
        {
            if (amount <= 0)
                return;

            SetHealth(currentHealth - amount);
        }

        /// <summary>
        /// 回血。
        /// </summary>
        /// <param name="amount">回血量，非正数会被忽略。</param>
        public void Heal(int amount)
        {
            if (amount <= 0)
                return;

            SetHealth(currentHealth + amount);
        }

        /// <summary>
        /// 增加 Geo。
        /// </summary>
        /// <param name="amount">增加量，非正数会被忽略。</param>
        public void AddGeo(int amount)
        {
            if (amount <= 0)
                return;

            geo += amount;
            GeoChanged?.Invoke(geo, amount);
        }

        /// <summary>
        /// 消耗 Geo。
        /// </summary>
        /// <param name="amount">消耗量，非正数会被忽略。</param>
        /// <returns>是否成功扣除。</returns>
        public bool SpendGeo(int amount)
        {
            if (amount <= 0)
                return false;

            if (geo < amount)
                return false;

            geo -= amount;
            GeoChanged?.Invoke(geo, -amount);
            return true;
        }
    }
}
