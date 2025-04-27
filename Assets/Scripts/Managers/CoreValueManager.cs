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
    public class CoreValueManager : MonoBehaviour
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
                    GameObject go = new GameObject("CoreValueManager");
                    instance = go.AddComponent<CoreValueManager>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }

        /// <summary>
        /// 当前贡献值
        /// </summary>
        public decimal valueContribution = 0;

        /// <summary>
        /// 普通礼物贡献值
        /// </summary>
        public decimal normalGiftContribution = 0;

        /// <summary>
        /// 上一秒的贡献值
        /// </summary>
        private decimal lastSecondValue = 0;

        /// <summary>
        /// 上一秒的普通礼物贡献值
        /// </summary>
        private decimal lastSecondNormalValue = 0;

        /// <summary>
        /// 每秒贡献值
        /// </summary>
        public decimal valuePerSecond { get; private set; } = 0;

        /// <summary>
        /// 每秒普通礼物贡献值
        /// </summary>
        public decimal normalValuePerSecond { get; private set; } = 0;

        /// <summary>
        /// 历史最大每秒贡献值
        /// </summary>
        public decimal maxValuePerSecond { get; private set; } = 0;

        /// <summary>
        /// 历史最大每秒普通礼物贡献值
        /// </summary>
        public decimal maxNormalValuePerSecond { get; private set; } = 0;

        /// <summary>
        /// 计时器
        /// </summary>
        private float timer = 0f;

        // 定义状态切换的阈值
        private const decimal CASUAL_SINGING_THRESHOLD = 100m;    // 随性唱歌阈值
        private const decimal FOCUSED_SINGING_THRESHOLD = 10000m;  // 投入唱歌阈值
        private const decimal PASSIONATE_SINGING_THRESHOLD = 100000m; // 激情唱歌阈值

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// 更新每秒贡献值
        /// </summary>
        private void Update()
        {
            timer += Time.deltaTime;
            if (timer >= 1f)
            {
                // 计算每秒贡献值
                valuePerSecond = valueContribution - lastSecondValue;
                // 计算每秒普通礼物贡献值
                normalValuePerSecond = normalGiftContribution - lastSecondNormalValue;
                
                // 更新最大每秒贡献值
                if (valuePerSecond > maxValuePerSecond)
                {
                    maxValuePerSecond = valuePerSecond;
                    // Debug.Log($"最大每秒贡献值更新: {maxValuePerSecond}");
                    
                }
                
                // 更新最大每秒普通礼物贡献值
                if (normalValuePerSecond > maxNormalValuePerSecond)
                {
                    maxNormalValuePerSecond = normalValuePerSecond;
                    Debug.Log($"最大每秒普通礼物贡献值更新: {maxNormalValuePerSecond}");
                    UpdateStreamerState();
                }
                
                lastSecondValue = valueContribution;
                lastSecondNormalValue = normalGiftContribution;
                timer = 0f;
            }
        }

        /// <summary>
        /// 根据最大每秒普通礼物贡献值更新主播状态
        /// </summary>
        private void UpdateStreamerState()
        {
            if (maxNormalValuePerSecond >= PASSIONATE_SINGING_THRESHOLD)
            {
                Debug.Log($"达到激情唱歌阈值: {PASSIONATE_SINGING_THRESHOLD}");
                StreamerStateManager.Instance.SetState(StreamerState.PassionateSinging);
            }
            else if (maxNormalValuePerSecond >= FOCUSED_SINGING_THRESHOLD)
            {
                Debug.Log($"达到投入唱歌阈值: {FOCUSED_SINGING_THRESHOLD}");
                StreamerStateManager.Instance.SetState(StreamerState.FocusedSinging);
            }
            else if (maxNormalValuePerSecond >= CASUAL_SINGING_THRESHOLD)
            {
                Debug.Log($"达到随性唱歌阈值: {CASUAL_SINGING_THRESHOLD}");
                StreamerStateManager.Instance.SetState(StreamerState.CasualSinging);
            }
            else
            {
                Debug.Log("未达到任何唱歌阈值，保持闲聊状态");
                StreamerStateManager.Instance.SetState(StreamerState.Chatting);
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
            normalGiftContribution += singleClickValue;
            
            // 检查是否达到1 trillion
            // if (valueContribution >= 1000000000000m) // 1 trillion = 1,000,000,000,000
            // {
            //     RestartManager.Instance.OnAdvanceRestartButtonClicked();
            // }
            
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
            normalGiftContribution = 0;
            lastSecondValue = 0;
            lastSecondNormalValue = 0;
            valuePerSecond = 0;
            normalValuePerSecond = 0;
            maxValuePerSecond = 0;
            maxNormalValuePerSecond = 0;
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