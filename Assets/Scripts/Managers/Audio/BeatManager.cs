/****************************************************************************
 * Author: 张嘉阳
 * Date: 2025-04-25
 * Description: 节拍管理器，用于管理音乐节拍
 * 
 * 使用说明：
 * 1. 在Inspector中设置：
 *    - BPM：每分钟节拍数
 *    - 拍子：每小节的拍数（如4/4拍）
 *    - 节拍预设：定义不同类型的节拍（如超轻、轻、重）
 *    - 阶段配置：为每个阶段选择使用的节拍预设和阶段强度
 * 
 * 2. 功能：
 *    - 根据BPM计算节拍时间
 *    - 根据当前阶段调整节拍
 *    - 根据配置触发不同强度的节拍反馈
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
    using MoreMountains.Feedbacks;
    using System.Collections.Generic;

    [System.Serializable]
    public class BeatFeedbackConfig
    {
        public string name;  // 节拍名称
        public bool[] triggerBeats = new bool[4];  // 触发拍子（1-4拍）
        public float intensity = 1f;  // 反馈强度
        public MMF_Player feedback;  // 反馈组件
    }

    [System.Serializable]
    public class StageConfig
    {
        public string name;  // 阶段名称
        public float stageIntensity = 1f;  // 阶段强度乘值
        public List<int> enabledBeatIndices = new List<int>();  // 启用的节拍预设索引
    }

    public class BeatManager : MonoBehaviour
    {
        #region Events
        public event Action<int, int> OnBeat;  // 节拍事件，参数为当前拍子和当前阶段
        #endregion

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

        [Header("节拍预设")]
        [SerializeField]
        private List<BeatFeedbackConfig> beatFeedbacks = new List<BeatFeedbackConfig>
        {
            new BeatFeedbackConfig { name = "超轻", triggerBeats = new bool[] { true, true, true, true }, intensity = 0.5f },
            new BeatFeedbackConfig { name = "轻", triggerBeats = new bool[] { false, true, false, true }, intensity = 0.8f },
            new BeatFeedbackConfig { name = "重", triggerBeats = new bool[] { true, false, true, false }, intensity = 1.0f }
        };

        [Header("阶段配置")]
        [SerializeField]
        private List<StageConfig> stageConfigs = new List<StageConfig>
        {
            new StageConfig { name = "阶段1", stageIntensity = 1f, enabledBeatIndices = new List<int> { 0, 1, 2 } },
            new StageConfig { name = "阶段2", stageIntensity = 1.2f, enabledBeatIndices = new List<int> { 1, 2 } },
            new StageConfig { name = "阶段3", stageIntensity = 1.5f, enabledBeatIndices = new List<int> { 2 } }
        };
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
        #endregion

        #region Public Methods
        /// <summary>
        /// 开始节拍
        /// </summary>
        public void StartBeat(int stage)
        {
            currentStage = stage;
            beatDuration = 60f / bpm;
            nextBeatTime = Time.time + beatDuration;
            currentBeat = 0;
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

        /// <summary>
        /// 获取当前拍子
        /// </summary>
        public int GetCurrentBeat()
        {
            return currentBeat;
        }

        /// <summary>
        /// 获取当前阶段
        /// </summary>
        public int GetCurrentStage()
        {
            return currentStage;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// 处理节拍
        /// </summary>
        private void HandleBeat()
        {
            currentBeat = (currentBeat % beatsPerBar) + 1;
            OnBeat?.Invoke(currentBeat, currentStage);

            // 检查当前阶段配置
            if (currentStage >= 0 && currentStage < stageConfigs.Count)
            {
                var stageConfig = stageConfigs[currentStage];
                
                // 检查每个启用的节拍配置是否在当前拍子触发
                foreach (var beatIndex in stageConfig.enabledBeatIndices)
                {
                    if (beatIndex >= 0 && beatIndex < beatFeedbacks.Count)
                    {
                        var config = beatFeedbacks[beatIndex];
                        if (config.feedback != null && config.triggerBeats[currentBeat - 1])
                        {
                            // 应用节拍强度和阶段强度
                            config.feedback.FeedbacksIntensity = config.intensity * stageConfig.stageIntensity;
                            config.feedback.PlayFeedbacks();
                        }
                    }
                }
            }

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
                StartBeat(currentStage);
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