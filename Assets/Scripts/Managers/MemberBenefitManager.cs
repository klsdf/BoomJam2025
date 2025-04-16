/****************************************************************************
 * Author: 周欣悦
 * Date: 2025-04-16
 * Description: 会员权益类，管理会员的各项权益加点
 ****************************************************************************/

namespace BoomJam2025
{
    using System.Numerics;

    /// <summary>
    /// 会员权益类，管理会员的各项权益加点
    /// </summary>
    [System.Serializable]
    public class MemberBenefitManager
    {
        /// <summary>
        /// 单例实例
        /// </summary>
        private static MemberBenefitManager instance;
        public static MemberBenefitManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new MemberBenefitManager();
                }
                return instance;
            }
        }

        /// <summary>
        /// 私有构造函数，防止外部实例化
        /// </summary>
        private MemberBenefitManager()
        {
            // 初始化默认值
            levelBaseClickValue = 1;
            levelPercentagePerLevel = 1;
            levelReductionFactor = 1;
            levelCriticalRate = 1;
            levelCriticalMultiplier = 1;
        }

        /// <summary>
        /// 基础点击价值加点等级
        /// </summary>
        public int levelBaseClickValue = 1;

        /// <summary>
        /// 每级提升百分比加点等级
        /// </summary>
        public int levelPercentagePerLevel = 1;

        /// <summary>
        /// 减免系数加点等级
        /// </summary>
        public int levelReductionFactor = 1;

        /// <summary>
        /// 暴击率加点等级
        /// </summary>
        public int levelCriticalRate = 1;

        /// <summary>
        /// 暴击倍率加点等级
        /// </summary>
        public int levelCriticalMultiplier = 1;

        /// <summary>
        /// 获取基础点击价值
        /// </summary>
        /// <returns>基础点击价值</returns>
        public double GetBaseClickValue()
        {
            return levelBaseClickValue * System.Math.Pow(2, RebirthManager.Instance.countRebirthBig);
        }

        /// <summary>
        /// 获取每级提升百分比
        /// </summary>
        /// <returns>每级提升百分比</returns>
        public float GetPercentagePerLevel()
        {
            return levelPercentagePerLevel * (float)System.Math.Pow(2, RebirthManager.Instance.countRebirthBig) * 0.01f;
        }

        /// <summary>
        /// 获取减免系数
        /// </summary>
        /// <returns>减免系数</returns>
        public int GetReductionFactor()
        {
            return levelReductionFactor * (int)System.Math.Pow(2, RebirthManager.Instance.countRebirthBig);
        }

        /// <summary>
        /// 获取暴击率
        /// </summary>
        /// <returns>暴击率</returns>
        public float GetCriticalRate()
        {
            return levelCriticalRate * (float)System.Math.Pow(2, RebirthManager.Instance.countRebirthBig) * 0.01f;
        }

        /// <summary>
        /// 获取暴击倍率
        /// </summary>
        /// <returns>暴击倍率</returns>
        public float GetCriticalMultiplier()
        {
            return levelCriticalMultiplier * (float)System.Math.Pow(2, RebirthManager.Instance.countRebirthBig);
        }

        /// <summary>
        /// 升级加点
        /// </summary>
        /// <param name="benefitType">权益类型</param>
        /// <returns>是否升级成功</returns>
        public bool UpgradeBenefit(BenefitType benefitType)
        {
            int currentLevel = GetBenefitLevel(benefitType);
            if (MemberLevelManager.Instance.pointsOuter < currentLevel) return false;

            MemberLevelManager.Instance.pointsOuter -= currentLevel;
            SetBenefitLevel(benefitType, currentLevel + 1);
            return true;
        }

        /// <summary>
        /// 重置权益等级
        /// </summary>
        public void Reset()
        {
            levelBaseClickValue = 1;
            levelPercentagePerLevel = 1;
            levelReductionFactor = 1;
            levelCriticalRate = 1;
            levelCriticalMultiplier = 1;
        }

        /// <summary>
        /// 获取权益等级
        /// </summary>
        /// <param name="benefitType">权益类型</param>
        /// <returns>等级</returns>
        private int GetBenefitLevel(BenefitType benefitType)
        {
            return benefitType switch
            {
                BenefitType.BaseClickValue => levelBaseClickValue,
                BenefitType.PercentagePerLevel => levelPercentagePerLevel,
                BenefitType.ReductionFactor => levelReductionFactor,
                BenefitType.CriticalRate => levelCriticalRate,
                BenefitType.CriticalMultiplier => levelCriticalMultiplier,
                _ => 0
            };
        }

        /// <summary>
        /// 设置权益等级
        /// </summary>
        /// <param name="benefitType">权益类型</param>
        /// <param name="level">等级</param>
        private void SetBenefitLevel(BenefitType benefitType, int level)
        {
            switch (benefitType)
            {
                case BenefitType.BaseClickValue:
                    levelBaseClickValue = level;
                    break;
                case BenefitType.PercentagePerLevel:
                    levelPercentagePerLevel = level;
                    break;
                case BenefitType.ReductionFactor:
                    levelReductionFactor = level;
                    break;
                case BenefitType.CriticalRate:
                    levelCriticalRate = level;
                    break;
                case BenefitType.CriticalMultiplier:
                    levelCriticalMultiplier = level;
                    break;
            }
        }
    }

    /// <summary>
    /// 权益类型枚举
    /// </summary>
    public enum BenefitType
    {
        /// <summary>   
        /// 基础点击价值
        /// </summary>
        BaseClickValue,
        /// <summary>
        /// 每级提升百分比
        /// </summary>
        PercentagePerLevel,
        /// <summary>
        /// 粉丝等级升级消耗减免系数
        /// </summary>
        ReductionFactor,
        /// <summary>
        /// 暴击率
        /// </summary>
        CriticalRate,
        /// <summary>
        /// 暴击倍率
        /// </summary>
        CriticalMultiplier
    }
} 