/****************************************************************************
 * Author: 周欣悦
 * Date: 2025-04-16
 * Description: 游戏管理器，负责管理游戏的核心系统和保存加载功能
 ****************************************************************************/

namespace BoomJam2025
{
    using UnityEngine;

    /// <summary>
    /// 游戏管理器类，使用单例模式管理游戏的核心系统
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        /// <summary>
        /// 单例实例
        /// </summary>
        public static GameManager Instance;

        /// <summary>
        /// Awake 初始化单例
        /// </summary>
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Start 加载游戏数据
        /// </summary>
        private void Start()
        {
            SaveManager.Instance.LoadGame();
        }

        /// <summary>
        /// Update 更新核心数值
        /// </summary>
        private void Update()
        {
            
        }

        /// <summary>
        /// OnApplicationQuit 退出时保存游戏
        /// </summary>
        private void OnApplicationQuit()
        {
            SaveManager.Instance.SaveGame();
        }
    }
}
