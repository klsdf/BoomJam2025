using UnityEngine;

namespace BoomJam2025
{
    /// <summary>
    /// 礼物接口
    /// </summary>
    public interface IGiftItem
    {
        /// <summary>
        /// 初始化礼物
        /// </summary>
        /// <param name="screenHeight">屏幕高度</param>
        void Initialize(float screenHeight);

        /// <summary>
        /// 设置礼物图标
        /// </summary>
        /// <param name="sprite">礼物图标</param>
        void SetGiftIcon(Sprite sprite);

        /// <summary>
        /// 设置贡献值显示
        /// </summary>
        /// <param name="value">贡献值</param>
        void SetObtainedContributionValue(string value);

        /// <summary>
        /// 获取礼物游戏对象
        /// </summary>
        GameObject gameObject { get; }
    }
} 