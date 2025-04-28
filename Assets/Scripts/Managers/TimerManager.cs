/****************************************************************************
 * Author: 周欣悦
 * Date: 2025-04-16
 * Description: 时间管理器，负责控制游戏时间的暂停和开始，以及强制重生
 ****************************************************************************/

namespace BoomJam2025
{
    using UnityEngine;

    /// <summary>
    /// 时间管理器类，负责控制游戏时间的暂停和开始，以及强制重生
    /// </summary>
    public class TimerManager : MonoBehaviour
    {
        /// <summary>
        /// 单例实例
        /// </summary>
        private static TimerManager instance;
        public static TimerManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<TimerManager>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("TimerManager");
                        instance = go.AddComponent<TimerManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return instance;
            }
        }

        /// <summary>
        /// 是否暂停
        /// </summary>
        private bool _isPaused = false;

        /// <summary>
        /// 是否启用
        /// </summary>
        private bool _isEnabled = false;

        /// <summary>
        /// 强制重生时间间隔（秒）
        /// </summary>
        private float _intervalForceRebirth = 60f; // 默认1分钟

        /// <summary>
        /// 当前计时器
        /// </summary>
        private float _timerCurrent = 0f;

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
        /// Update 更新计时器
        /// </summary>
        private void Update()
        {
            if (!_isEnabled || _isPaused) return;

            _timerCurrent += Time.deltaTime;
            if (_timerCurrent >= _intervalForceRebirth)
            {
                ForceRebirth();
                _timerCurrent = 0f;
            }
        }

        /// <summary>
        /// 开始运行
        /// </summary>
        public void StartRunning()
        {
            _isEnabled = true;
            _isPaused = false;
            Time.timeScale = 1f;
        }

        /// <summary>
        /// 停止运行
        /// </summary>
        public void StopRunning()
        {
            _isEnabled = false;
            _isPaused = true;
            Time.timeScale = 0f;
        }

        /// <summary>
        /// 暂停游戏
        /// </summary>
        public void PauseGame()
        {
            if (!_isEnabled) return;
            _isPaused = true;
            Time.timeScale = 0f;
        }

        /// <summary>
        /// 继续游戏
        /// </summary>
        public void ResumeGame()
        {
            if (!_isEnabled) return;
            _isPaused = false;
            Time.timeScale = 1f;
        }

        /// <summary>
        /// 设置强制重生时间间隔
        /// </summary>
        /// <param name="seconds">时间间隔（秒）</param>
        public void SetForceRebirthInterval(float seconds)
        {
            _intervalForceRebirth = seconds;
        }

        /// <summary>
        /// 强制重生
        /// </summary>
        private void ForceRebirth()
        {
            RebirthManager.Instance.TryRebirth();
        }

        /// <summary>
        /// 重置计时器
        /// </summary>
        public void ResetTimer()
        {
            _timerCurrent = 0f;
        }
    }
} 