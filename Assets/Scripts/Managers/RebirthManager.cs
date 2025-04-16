/****************************************************************************
 * Author: 周欣悦
 * Date: 2025-04-16
 * Description: 重生管理器，负责管理游戏的重生系统
 ****************************************************************************/

namespace BoomJam2025
{
    using UnityEngine;
    using System.Numerics;

    /// <summary>
    /// 重生管理器类，负责管理游戏的重生系统
    /// </summary>
    public class RebirthManager : MonoBehaviour
    {
        /// <summary>
        /// 单例实例
        /// </summary>
        private static RebirthManager instance;
        public static RebirthManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<RebirthManager>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("RebirthManager");
                        instance = go.AddComponent<RebirthManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return instance;
            }
        }

        /// <summary>
        /// 小重开次数
        /// </summary>
        public int countRebirth = 0;

        /// <summary>
        /// 大重开次数
        /// </summary>
        public int countRebirthBig = 0;

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
        /// 小重开
        /// </summary>
        public void TryRebirth()
        {
            countRebirth++;
            CoreValueManager.Instance.Reset();
            MemberLevelManager.Instance.Reset();
            FanLevelManager.Instance.Reset();
        }

        /// <summary>
        /// 大重开
        /// </summary>
        public void TryRebirthBig()
        {
            countRebirthBig++;
            CoreValueManager.Instance.Reset();
            MemberLevelManager.Instance.Reset();
            FanLevelManager.Instance.Reset();
            MemberBenefitManager.Instance.Reset();
        }
    }
}