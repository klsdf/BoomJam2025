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
        /// 点击送出礼物，并且增加贡献值
        /// </summary>
        /// <returns>本次点击获得的贡献值</returns>
        public double ClickGiftValue()
        {
            double singleClickValue = GetClickValue();
            valueContribution += singleClickValue;
            return singleClickValue;
        }

        /// <summary>
        /// 获取单次点击价值
        /// </summary>
        /// <returns>单次点击价值</returns>
        public double GetClickValue()
        {
            // 计算基础点击价值
            double baseValue = MemberBenefitManager.Instance.GetBaseClickValue();

            // 计算粉丝等级提升百分比
            float fanBoostPercentage = FanLevelManager.Instance.GetClickBoostPercentage();

            // 计算最终价值
            double singleClickValue = baseValue * (1 + fanBoostPercentage);

            // 判断是否暴击
            if (Random.value < MemberBenefitManager.Instance.GetCriticalRate())
            {
                singleClickValue *= MemberBenefitManager.Instance.GetCriticalMultiplier();
            }

            return singleClickValue;
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
            // 将数字转换为字符串，保留所有小数位
            string valueStr = value.ToString();
            
            // 计算纯数字的位数（不包括小数点）
            int digitCount = valueStr.Replace(".", "").Length;
            
            // 如果纯数字位数不超过8位，直接显示
            if (digitCount <= 8)
            {
                return valueStr;
            }
            
            // 如果超过千万（8位），使用科学计数法
            if (value >= 10000000)
            {
                return value.ToString("E2");
            }
            
            // 其他情况（超过8位但小于千万），显示前8位数字
            string digitsOnly = valueStr.Replace(".", "");
            string firstEightDigits = digitsOnly.Substring(0, 8);
            
            // 如果有小数点，需要重新插入小数点
            if (valueStr.Contains("."))
            {
                int decimalIndex = valueStr.IndexOf(".");
                // 确保小数点位置正确
                if (decimalIndex < 8)
                {
                    return firstEightDigits.Insert(decimalIndex, ".");
                }
            }
            
            return firstEightDigits;
        }
    }
}