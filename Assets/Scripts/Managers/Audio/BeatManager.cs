/****************************************************************************
 * Author: 张嘉阳
 * Date: 2025-04-25
 * Description: 节拍管理器，用于管理音乐节拍
 * 
 * 使用说明：
 * 1. 在Inspector中设置：
 *    - BPM：每分钟节拍数
 *    - 拍子：每小节的拍数（如4/4拍）
 * 
 * 2. 功能：
 *    - 根据BPM计算节拍时间
 *    - 在重拍时触发OnBeat事件
 *    - 提供调试功能显示当前拍子
 * 
 * 3. 注意事项：
 *    - 确保BPM设置正确
 *    - 可以通过调试模式查看节拍效果
 ****************************************************************************/
namespace BoomJam2025
{
    using UnityEngine;
    using System;

    public class BeatManager : MonoBehaviour
    {
        #region Inspector Settings
        [Header("节拍设置")]
        /// <summary>
        /// 每分钟节拍数
        /// </summary>
        [SerializeField] private float bpm = 120f;
        
        /// <summary>
        /// 每小节的拍数
        /// </summary>
        [SerializeField] private int beatsPerBar = 4;

        [Header("调试设置")]
        /// <summary>
        /// 是否启用调试模式
        /// </summary>
        [SerializeField] private bool debugMode = false;
        
        /// <summary>
        /// 调试按钮：开始节拍
        /// </summary>
        [SerializeField] private bool debugStartBeat = false;
        
        /// <summary>
        /// 调试按钮：停止节拍
        /// </summary>
        [SerializeField] private bool debugStopBeat = false;
        
        /// <summary>
        /// 当前拍子（只读）
        /// </summary>
        [SerializeField, ReadOnly] private int currentBeat = 0;

        /// <summary>
        /// 当前阶段（只读）
        /// </summary>
        [SerializeField, ReadOnly] private int currentStage = 0;
        #endregion

        #region Private Fields
        /// <summary>
        /// 每个节拍的时长（秒）
        /// </summary>
        private float beatDuration;
        
        /// <summary>
        /// 下一个节拍的时间
        /// </summary>
        private float nextBeatTime;
        
        /// <summary>
        /// 是否正在运行
        /// </summary>
        private bool isRunning;

        /// <summary>
        /// 音乐管理器引用
        /// </summary>
        private MusicManager musicManager;
        #endregion

        #region Events
        /// <summary>
        /// 节拍事件
        /// </summary>
        public event Action<int> OnBeat;
        #endregion

        private void Start()
        {
            musicManager = FindObjectOfType<MusicManager>();
        }

        #region Public Methods
        /// <summary>
        /// 开始节拍
        /// </summary>
        public void StartBeat()
        {
            if (musicManager == null) return;

            beatDuration = 60f / bpm;  // 计算每个节拍的时长
            nextBeatTime = Time.time + beatDuration;
            currentBeat = 0;
            currentStage = musicManager.GetCurrentStage();
            isRunning = true;
        }

        /// <summary>
        /// 停止节拍
        /// </summary>
        public void StopBeat()
        {
            isRunning = false;
            currentBeat = 0;
            currentStage = 0;
        }

        /// <summary>
        /// 设置BPM
        /// </summary>
        public void SetBPM(float newBPM)
        {
            bpm = newBPM;
            if (isRunning)
            {
                beatDuration = 60f / bpm;
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// 处理节拍
        /// </summary>
        private void HandleBeat()
        {
            if (musicManager == null) return;

            currentBeat = (currentBeat % beatsPerBar) + 1;
            currentStage = musicManager.GetCurrentStage();
            
            // 触发节拍事件
            OnBeat?.Invoke(currentBeat);

            // 调试输出
            if (debugMode)
            {
                Debug.Log($"Beat: {currentBeat}, Stage: {currentStage}");
            }
        }

        /// <summary>
        /// 处理调试按钮
        /// </summary>
        private void HandleDebugButtons()
        {
            if (!debugMode) return;

            if (debugStartBeat)
            {
                debugStartBeat = false;
                StartBeat();
            }

            if (debugStopBeat)
            {
                debugStopBeat = false;
                StopBeat();
            }
        }
        #endregion

        #region Unity Methods
        private void Update()
        {
            HandleDebugButtons();

            if (!isRunning) return;

            if (Time.time >= nextBeatTime)
            {
                HandleBeat();
                nextBeatTime = Time.time + beatDuration;
            }
        }
        #endregion
    }
} 