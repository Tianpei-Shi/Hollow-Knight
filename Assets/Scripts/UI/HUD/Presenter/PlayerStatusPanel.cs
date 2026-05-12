using Game.UI.HUD.Model;
using Game.UI.HUD.View;
using UnityEngine;

namespace Game.UI.HUD.Presenter
{
    /// <summary>
    /// HUD 展示协调器（Presenter），负责订阅数据事件并驱动各个 HUD 视图刷新。
    /// 这是商业项目常见的“中介层”，用于把数据层与视图层解耦。
    /// </summary>
    public class PlayerStatusPanel : MonoBehaviour
    {
        /// <summary>
        /// HUD 数据源，提供血量/Geo 数据和变化事件。
        /// </summary>
        [Header("Dependencies")]
        [Tooltip("HUD 数据源，负责维护血量和 Geo。")]
        [SerializeField] private PlayerHudDataSource dataSource;

        /// <summary>
        /// 血量 HUD 视图。
        /// </summary>
        [Tooltip("血量 HUD 视图。")]
        [SerializeField] private HealthPanel healthView;

        /// <summary>
        /// Geo HUD 视图。
        /// </summary>
        [Tooltip("Geo HUD 视图。")]
        [SerializeField] private GeoPanel geoView;

        /// <summary>
        /// Unity 生命周期方法：对象加载时调用。
        /// 优先自动补齐同对象上的依赖，减少初期搭建摩擦。
        /// </summary>
        private void Awake()
        {
            dataSource = dataSource != null ? dataSource : GetComponent<PlayerHudDataSource>();
            healthView = healthView != null ? healthView : GetComponentInChildren<HealthPanel>(true);
            geoView = geoView != null ? geoView : GetComponentInChildren<GeoPanel>(true);
        }

        /// <summary>
        /// Unity 生命周期方法：组件启用时调用。
        /// 在这里订阅事件，并立刻推送一次全量数据给 HUD，避免初始显示为空。
        /// </summary>
        private void OnEnable()
        {
            if (dataSource == null)
                return;

            dataSource.HealthChanged += HandleHealthChanged;
            dataSource.GeoChanged += HandleGeoChanged;
            dataSource.PublishAll();
        }

        /// <summary>
        /// Unity 生命周期方法：组件禁用时调用。
        /// 在这里取消订阅，避免对象销毁后仍收到事件回调导致异常。
        /// </summary>
        private void OnDisable()
        {
            if (dataSource == null)
                return;

            dataSource.HealthChanged -= HandleHealthChanged;
            dataSource.GeoChanged -= HandleGeoChanged;
        }

        /// <summary>
        /// 处理血量变化事件，并驱动血量视图刷新。
        /// </summary>
        /// <param name="currentHealth">当前血量。</param>
        /// <param name="maxHealth">最大血量。</param>
        private void HandleHealthChanged(int currentHealth, int maxHealth)
        {
            if (healthView == null)
                return;

            healthView.RenderHealth(currentHealth, maxHealth);
        }

        /// <summary>
        /// 处理 Geo 变化事件，并驱动 Geo 视图刷新。
        /// </summary>
        /// <param name="currentGeo">当前 Geo。</param>
        /// <param name="delta">本次变化量。</param>
        private void HandleGeoChanged(int currentGeo, int delta)
        {
            if (geoView == null)
                return;

            geoView.RenderGeo(currentGeo, delta);
        }
    }
}
