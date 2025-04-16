/****************************************************************************
 * Author: 周欣悦
 * Date: 2025-04-16
 * Description: 粉丝等级管理器，负责管理粉丝等级系统
 ****************************************************************************/

namespace BoomJam2025
{
    using UnityEngine;
    using System.Numerics;

    /// <summary>
    /// 粉丝等级管理器类，负责管理粉丝等级系统
    /// </summary>
    public class FanLevelManager : MonoBehaviour
    {
        /// <summary>
        /// 单例实例
        /// </summary>
        private static FanLevelManager instance;
        public static FanLevelManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<FanLevelManager>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("FanLevelManager");
                        instance = go.AddComponent<FanLevelManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return instance;
            }
        }

        /// <summary>
        /// 粉丝等级
        /// </summary>
        public int levelFan = 1;

        /// <summary>
        /// Awake 初始化单例
        /// </summary>
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 获取每次点击的提升百分比
        /// </summary>
        /// <returns>提升百分比</returns>
        public float GetClickBoostPercentage()
        {
            float percentagePerLevel = MemberBenefitManager.Instance.GetPercentagePerLevel();
            return (levelFan - 1) * percentagePerLevel;
        }

        /// <summary>
        /// 升级粉丝等级
        /// </summary>
        /// <param name="contribution">当前贡献值</param>
        /// <returns>升级消耗的贡献值，升级失败返回0</returns>
        public double TryUpgrade(double contribution)
        {
            // 计算升级所需贡献值
            double cost = GetUpgradeCost();

            if (contribution >= cost)
            {
                levelFan++;
                return cost;
            }
            return 0;
        }

        /// <summary>
        /// 获取升级所需贡献值
        /// </summary>
        /// <returns>升级所需贡献值</returns>
        public double GetUpgradeCost()
        {
            int reductionFactor = MemberBenefitManager.Instance.GetReductionFactor();
            return 11 * System.Math.Pow(1.1, levelFan - 1) / reductionFactor;
        }

        /// <summary>
        /// 获取当前粉丝等级
        /// </summary>
        /// <returns>当前粉丝等级</returns>
        public int GetLevelFan()
        {
            return levelFan;
        }

        /// <summary>
        /// 重置粉丝等级
        /// </summary>
        public void Reset()
        {
            levelFan = 1;
        }
    }
} 