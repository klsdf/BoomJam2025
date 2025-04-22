/****************************************************************************
 * Author: 周欣悦
 * Date: 2025-04-16
 * Description: 存档管理器，负责管理游戏的保存和加载功能
 ****************************************************************************/

namespace BoomJam2025
{
    using UnityEngine;
    using System.Numerics;

    /// <summary>
    /// 存档管理器类，负责管理游戏的保存和加载功能
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        /// <summary>
        /// 单例实例
        /// </summary>
        private static SaveManager instance;
        public static SaveManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<SaveManager>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("SaveManager");
                        instance = go.AddComponent<SaveManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return instance;
            }
        }

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
        /// 保存游戏数据
        /// </summary>
        public void SaveGame()
        {
            PlayerPrefs.SetString("value", CoreValueManager.Instance.valueContribution.ToString());
            PlayerPrefs.SetInt("rebirth", RebirthManager.Instance.countRebirth);
        }

        /// <summary>
        /// 加载游戏数据
        /// </summary>
        public void LoadGame()
        {
            string valueStr = PlayerPrefs.GetString("value", "0");
            CoreValueManager.Instance.valueContribution = decimal.Parse(valueStr);
            RebirthManager.Instance.countRebirth = PlayerPrefs.GetInt("rebirth", 0);
        }
    }
}