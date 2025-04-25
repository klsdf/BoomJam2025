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
        public decimal valueContribution = 0;

        /// <summary>
        /// 上一秒的贡献值
        /// </summary>
        private decimal lastSecondValue = 0;

        /// <summary>
        /// 每秒贡献值
        /// </summary>
        public decimal valuePerSecond { get; private set; } = 0;

        /// <summary>
        /// 历史最大每秒贡献值
        /// </summary>
        public decimal maxValuePerSecond { get; private set; } = 0;

        /// <summary>
        /// 计时器
        /// </summary>
        private float timer = 0f;

        /// <summary>
        /// 更新每秒贡献值
        /// </summary>
        public void Update()
        {
            timer += Time.deltaTime;
            if (timer >= 1f)
            {
                // 计算每秒贡献值
                valuePerSecond = valueContribution - lastSecondValue;
                // 更新最大每秒贡献值
                if (valuePerSecond > maxValuePerSecond)
                {
                    maxValuePerSecond = valuePerSecond;
                }
                lastSecondValue = valueContribution;
                timer = 0f;
            }
        }

        /// <summary>
        /// 点击送出礼物，并且增加贡献值
        /// </summary>
        /// <returns>本次点击获得的贡献值</returns>
        public decimal ClickGiftValue()
        {
            decimal singleClickValue = GetClickValue();
            // 判断是否暴击
            if (Random.value < (float)MemberBenefitManager.Instance.GetCriticalRate())
            {
                singleClickValue *= (decimal)MemberBenefitManager.Instance.GetCriticalMultiplier();
            }
            valueContribution += singleClickValue;
            
            // 检查是否达到1 trillion
            if (valueContribution >= 1000000000000m) // 1 trillion = 1,000,000,000,000
            {
                RestartManager.Instance.OnAdvanceRestartButtonClicked();
            }
            
            return singleClickValue;
        }

        /// <summary>
        /// 获取单次点击价值，不考虑暴击
        /// </summary>
        /// <returns>单次点击价值</returns>
        public decimal GetClickValue()
        {
            // 计算基础点击价值
            decimal baseValue = (decimal)MemberBenefitManager.Instance.GetBaseClickValue();

            // 计算粉丝等级提升百分比
            decimal fanBoostPercentage = (decimal)MemberBenefitManager.Instance.GetPercentagePer();

            int levelFan = FanLevelManager.Instance.levelFan;
            // 计算最终价值
            decimal singleClickValue = baseValue * (decimal)System.Math.Pow((double)(1 + fanBoostPercentage), levelFan - 1);

            return singleClickValue;
        }

        /// <summary>
        /// 消耗贡献值
        /// </summary>
        /// <param name="amount">消耗量</param>
        /// <returns>是否消耗成功</returns>
        public bool ConsumeContribution(decimal amount)
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
            lastSecondValue = 0;
            valuePerSecond = 0;
            maxValuePerSecond = 0;
            timer = 0f;
        }

        /// <summary>
        /// 格式化贡献值显示
        /// </summary>
        /// <param name="value">贡献值</param>
        /// <returns>格式化后的字符串</returns>
        public string FormatValue(decimal value)
        {
            if (value < 1000000)
            {
                // 小于100万时，显示到小数点后2位
                return value.ToString("N2");
            }
            else if (value < 1000000000)
            {
                // 100万到10亿之间，使用K/M为单位
                if (value < 1000000)
                    return (value / 1000).ToString("N2") + "K";
                else
                    return (value / 1000000).ToString("N2") + "M";
            }
            else
            {
                // 大于10亿时，使用科学计数法
                return value.ToString("E2");
            }
        }
        
        public string FormatValueShort(decimal value)
        {
            if (value < 1000)
            {
                // 小于1000时，显示到小数点后2位
                return value.ToString("N2");
            }
            else if (value < 1000000000)
            {
                // 100万到10亿之间，使用K/M为单位
                if (value < 1000000)
                    return (value / 1000).ToString("N2") + "K";
                else
                    return (value / 1000000).ToString("N2") + "M";
            }
            else
            {
                // 大于10亿时，使用科学计数法
                return value.ToString("E2");
            }
        }
        /// <summary>
        /// 获取指定等级下的暴击价值
        /// </summary>
        /// <param name="level">等级</param>
        /// <returns>暴击价值</returns>
        public decimal GetCritValueAtLevel(int level)
        {
            // 计算基础点击价值
            decimal baseValue = (decimal)MemberBenefitManager.Instance.GetBaseClickValue();

            // 计算粉丝等级提升百分比
            decimal fanBoostPercentage = (decimal)MemberBenefitManager.Instance.GetPercentagePer();

            // 计算最终价值
            decimal singleClickValue = baseValue * (decimal)System.Math.Pow((double)(1 + fanBoostPercentage), level - 1);

            singleClickValue *= (decimal)MemberBenefitManager.Instance.GetCriticalMultiplier();

            return singleClickValue;
        }

        /// <summary>
        /// 格式化贡献值显示（大于100万时只显示整数部分）
        /// </summary>
        /// <param name="value">贡献值</param>
        /// <returns>格式化后的字符串</returns>
        public string FormatValueInteger(decimal value)
        {
            if (value < 1000000)
            {
                // 小于100万时，显示到小数点后2位
                return value.ToString("N2");
            }
            else
            {
                // 大于100万时，只显示整数部分
                return ((long)value).ToString();
            }
        }
    }
}