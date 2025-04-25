/****************************************************************************
 * Author: 张嘉阳
 * Date: 2025-04-25
 * Description: 音频管理器，作为中间层管理所有音频相关功能
 * 
 * 使用说明：
 * 1. 功能说明：
 *    - 管理所有音频相关的功能
 *    - 提供统一的接口供其他管理器调用
 *    - 包含调试功能
 * 
 * 2. 调试功能：
 *    - 勾选debugMode启用调试模式
 *    - 使用调试按钮控制音乐：
 *      * debugPlayBackground：播放背景音乐
 *      * debugStartGame：开始游戏音乐
 *      * debugSwitchStage：切换到指定阶段（配合debugTargetStage使用）
 *      * debugRestart：重新开始游戏音乐
 *      * debugStopAll：停止所有音乐
 *    - debugCurrentStage显示当前阶段（只读）
 * 
 * 3. 公共接口：
 *    - PlayBackgroundMusic()：播放背景音乐
 *    - StartGameMusic()：开始游戏音乐
 *    - SwitchToStage(int stageIndex)：切换到指定阶段
 *    - RestartGameMusic()：重新开始游戏音乐
 *    - StopAllMusic()：停止所有音乐播放
 * 
 * 4. 注意事项：
 *    - 其他管理器应该通过AudioManager调用音乐功能
 *    - 不要直接调用MusicManager
 ****************************************************************************/
namespace BoomJam2025
{
    using UnityEngine;
    using System.Collections;

    public class AudioManager : MonoBehaviour
    {
        #region Singleton
        public static AudioManager Instance { get; private set; }

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
        #endregion

        #region Inspector Settings
        [Header("音乐管理器引用")]
        [SerializeField] private MusicManager musicManager;

        [Header("节拍管理器引用")]
        [SerializeField] private BeatManager beatManager;

        [Header("调试设置")]
        /// <summary>
        /// 是否启用调试模式
        /// </summary>
        [SerializeField] private bool debugMode = false;
        
        /// <summary>
        /// 调试模式下的当前阶段
        /// </summary>
        [SerializeField, ReadOnly] private int debugCurrentStage = 0;
        
        /// <summary>
        /// 调试模式下的目标阶段
        /// </summary>
        [SerializeField] private int debugTargetStage = 0;
        
        /// <summary>
        /// 调试按钮：播放背景音乐
        /// </summary>
        [SerializeField] private bool debugPlayBackground = false;
        
        /// <summary>
        /// 调试按钮：开始游戏音乐
        /// </summary>
        [SerializeField] private bool debugStartGame = false;
        
        /// <summary>
        /// 调试按钮：切换阶段
        /// </summary>
        [SerializeField] private bool debugSwitchStage = false;
        
        /// <summary>
        /// 调试按钮：重新开始
        /// </summary>
        [SerializeField] private bool debugRestart = false;

        /// <summary>
        /// 调试按钮：停止所有音乐
        /// </summary>
        [SerializeField] private bool debugStopAll = false;
        #endregion

        private void Start()
        {
            // 监听节拍事件
            if (beatManager != null)
            {
                beatManager.OnBeat += OnBeat;
            }
        }

        private void OnDestroy()
        {
            // 取消监听节拍事件
            if (beatManager != null)
            {
                beatManager.OnBeat -= OnBeat;
            }
        }

        /// <summary>
        /// 节拍事件处理
        /// </summary>
        private void OnBeat(int beat)
        {
            if (debugMode)
            {
                int currentStage = musicManager.GetCurrentStage();
                Debug.Log($"节拍: {beat}, 当前阶段: {currentStage}");
            }
        }

        #region Public Methods
        /// <summary>
        /// 播放背景音乐
        /// </summary>
        public void PlayBackgroundMusic()
        {
            musicManager.PlayBackgroundMusic();
        }

        /// <summary>
        /// 开始游戏音乐
        /// </summary>
        public void StartGameMusic()
        {
            musicManager.StartGameMusic();
            if (beatManager != null)
            {
                beatManager.StartBeat();
            }
        }

        /// <summary>
        /// 切换到指定阶段
        /// </summary>
        public void SwitchToStage(int stageIndex)
        {
            musicManager.PrepareSwitchToStage(stageIndex);
            if (beatManager != null)
            {
                beatManager.StartBeat(); // 重新开始节拍以确保同步
            }
        }

        /// <summary>
        /// 重新开始游戏音乐
        /// </summary>
        public void RestartGameMusic()
        {
            musicManager.RestartGameMusic();
            if (beatManager != null)
            {
                beatManager.StartBeat();
            }
        }

        /// <summary>
        /// 停止所有音乐播放
        /// </summary>
        public void StopAllMusic()
        {
            musicManager.StopAllMusic();
            if (beatManager != null)
            {
                beatManager.StopBeat();
            }
        }
        #endregion

        #region Debug Methods
        /// <summary>
        /// 处理所有调试按钮
        /// </summary>
        private void HandleDebugButtons()
        {
            if (!debugMode) return;

            if (debugPlayBackground)
            {
                debugPlayBackground = false;
                PlayBackgroundMusic();
            }

            if (debugStartGame)
            {
                debugStartGame = false;
                StartGameMusic();
            }

            if (debugSwitchStage)
            {
                debugSwitchStage = false;
                SwitchToStage(debugTargetStage);
            }

            if (debugRestart)
            {
                debugRestart = false;
                RestartGameMusic();
            }

            if (debugStopAll)
            {
                debugStopAll = false;
                StopAllMusic();
            }
        }

        /// <summary>
        /// 更新调试显示
        /// </summary>
        private void UpdateDebugDisplay()
        {
            if (!debugMode) return;
            debugCurrentStage = musicManager.GetCurrentStage();
        }
        #endregion

        private void Update()
        {
            HandleDebugButtons();
            UpdateDebugDisplay();
        }
    }

    /// <summary>
    /// 只读属性特性
    /// </summary>
    public class ReadOnlyAttribute : PropertyAttribute { }
} 