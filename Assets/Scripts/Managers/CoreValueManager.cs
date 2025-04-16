/****************************************************************************
 * Author: 周欣悦
 * Date: 2025-04-16
 * Description: 贡献值管理器，负责管理游戏的核心数值和增长
 ****************************************************************************/

namespace BoomJam2025
{
    using UnityEngine;
    using System.Numerics;

    /// <summary>
    /// 贡献值管理器类，负责管理游戏的核心数值和增长
    /// </summary>
    public class CoreValueManager
    {
        /// <summary>
        /// 单例实例
        /// </summary>
        private static CoreValueManager instance;
        public static CoreValueManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new CoreValueManager();
                }
                return instance;
            }
        }

        /// <summary>
        /// 当前贡献值
        /// </summary>
        public double valueContribution = 0;

        /// <summary>
        /// 点击送出礼物
        /// </summary>
        /// <returns>本次点击获得的贡献值</returns>
        public double ClickGiftValue()
        {
            // 计算基础点击价值
            double baseValue = MemberBenefitManager.Instance.GetBaseClickValue();

            // 计算粉丝等级提升百分比
            float fanBoostPercentage = FanLevelManager.Instance.GetClickBoostPercentage();

            // 计算最终价值
            double finalValue = baseValue * (1 + fanBoostPercentage);

            // 判断是否暴击
            if (Random.value < MemberBenefitManager.Instance.GetCriticalRate())
            {
                finalValue *= MemberBenefitManager.Instance.GetCriticalMultiplier();
            }

            valueContribution += finalValue;
            return finalValue;
        }

        /// <summary>
        /// 消耗贡献值
        /// </summary>
        /// <param name="amount">消耗量</param>
        /// <returns>是否消耗成功</returns>
        public bool ConsumeContribution(double amount)
        {
            if (valueContribution >= amount)
            {
                valueContribution -= amount;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 重置贡献值
        /// </summary>
        public void Reset()
        {
            valueContribution = 0;
        }

        /// <summary>
        /// 格式化贡献值显示
        /// </summary>
        /// <param name="value">贡献值</param>
        /// <returns>格式化后的字符串</returns>
        public string FormatValue(double value)
        {
            if (value < 1000)
            {
                // 小于1000时显示小数点后5位
                return value.ToString("F5");
            }
            else if (value < 1000000)
            {
                // 小于100万时显示整数
                return value.ToString("F0");
            }
            else
            {
                // 大于等于100万时使用科学计数法
                return value.ToString("E2");
            }
        }
    }
}