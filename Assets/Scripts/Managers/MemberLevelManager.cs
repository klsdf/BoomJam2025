/****************************************************************************
 * Author: 周欣悦
 * Date: 2025-04-16
 * Description: 会员等级管理器，负责管理会员等级系统
 ****************************************************************************/

namespace BoomJam2025
{
    using UnityEngine;
    using System.Numerics;

    /// <summary>
    /// 会员等级管理器类，负责管理会员等级系统
    /// </summary>
    public class MemberLevelManager : MonoBehaviour
    {
        /// <summary>
        /// 单例实例
        /// </summary>
        private static MemberLevelManager instance;
        public static MemberLevelManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<MemberLevelManager>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("MemberLevelManager");
                        instance = go.AddComponent<MemberLevelManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return instance;
            }
        }

        /// <summary>
        /// 会员等级
        /// </summary>
        public int levelMember = 1;

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
        /// 升级会员等级
        /// </summary>
        /// <returns>升级消耗的贡献值，升级失败返回0</returns>
        public decimal TryUpgrade()
        {
            // 计算升级所需贡献值
            decimal cost = GetUpgradeCost();

            if (CoreValueManager.Instance.valueContribution >= cost)
            {
                levelMember++;
                MemberBenefitManager.Instance.pointsOuter++;
                CoreValueManager.Instance.valueContribution -= cost;
                return cost;
            }
            return 0;
        }

        /// <summary>
        /// 获取升级所需贡献值
        /// </summary>
        /// <returns>升级所需贡献值</returns>
        public decimal GetUpgradeCost()
        {
            return (decimal)(500 * System.Math.Pow(2, levelMember - 1));
        }

        /// <summary>
        /// 重置会员等级
        /// </summary>
        public void Reset()
        {
            levelMember = 1;
            // 局外点数和会员权益在小重开时不重置
        }
    }
} 