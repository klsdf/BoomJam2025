/****************************************************************************
 * Author: 张嘉阳
 * Date: 2025-04-25
 * Description: 音乐管理器，用于管理游戏中的音乐播放
 * 
 * 使用说明：
 * 1. 背景音乐和游戏音乐设置：
 *    - 在Inspector中设置backgroundMusic（仅主旋律）
 *    - 在gameStages中设置三个阶段的音轨（主题+鼓点、主题+贝斯、完整混音）
 * 
 * 2. 公共接口（不建议直接调用）：
 *    - PlayBackgroundMusic()：播放背景音乐
 *    - StartGameMusic()：开始游戏音乐
 *    - PrepareSwitchToStage(int stageIndex)：准备切换到指定阶段
 *    - RestartGameMusic()：重新开始游戏音乐
 *    - StopAllMusic()：停止所有音乐播放
 *    - GetCurrentStage()：获取当前阶段
 * 
 * 3. 注意事项：
 *    - 确保所有音轨长度一致
 *    - 可以使用autoDetectLoopLength自动检测循环长度
 *    - 或手动设置manualLoopLength
 *    - ！！！其他管理器应该通过AudioManager调用本类的方法，不建议直接调用！！！
 ****************************************************************************/
namespace BoomJam2025
{
    using UnityEngine;
    using System.Collections;
    using System.Collections.Generic;
    using System;

    /// <summary>
    /// 音乐阶段类，用于定义每个阶段的音轨组合
    /// </summary>
    [System.Serializable]
    public class MusicStage
    {
        /// <summary>
        /// 阶段名称
        /// </summary>
        public string stageName;
        
        /// <summary>
        /// 该阶段使用的所有音轨
        /// </summary>
        public List<AudioClip> tracks = new List<AudioClip>();
    }

    /// <summary>
    /// 音乐管理器，负责管理游戏中的音乐播放和阶段切换
    /// </summary>
    public class MusicManager : MonoBehaviour
    {
        #region Events
        // 移除事件定义
        // public event Action<int> OnStageChanged;
        #endregion

        #region Inspector Settings
        [Header("音乐设置")]
        /// <summary>
        /// 是否自动检测循环长度
        /// </summary>
        [SerializeField] private bool autoDetectLoopLength = false;
        
        /// <summary>
        /// 手动设置的循环长度
        /// </summary>
        [SerializeField] private float manualLoopLength = 15.0f;

        /// <summary>
        /// 误差时间，用于跳过音频开始的一小段
        /// </summary>
        [SerializeField] private float errorTime = 0.0f;

        [Header("背景音乐")]
        /// <summary>
        /// 背景音乐片段（仅主旋律）
        /// </summary>
        [SerializeField] private AudioClip backgroundMusic;

        [Header("游戏音乐")]
        /// <summary>
        /// 游戏音乐阶段列表（不包含主题阶段）
        /// </summary>
        [SerializeField] private List<MusicStage> gameStages = new List<MusicStage>
        {
            new MusicStage { stageName = "主题+鼓点" },
            new MusicStage { stageName = "主题+贝斯" },
            new MusicStage { stageName = "完整混音" }
        };
        #endregion

        #region Private Fields
        /// <summary>
        /// 所有音频源组件
        /// </summary>
        private List<AudioSource> audioSources = new List<AudioSource>();
        
        /// <summary>
        /// 当前循环长度
        /// </summary>
        private float currentLoopLength;
        
        /// <summary>
        /// 下一个循环开始的时间
        /// </summary>
        private float nextLoopTime;
        
        /// <summary>
        /// 是否正在播放
        /// </summary>
        private bool isPlaying;
        
        /// <summary>
        /// 当前阶段索引
        /// </summary>
        private int currentStageIndex;

        /// <summary>
        /// 目标阶段索引（用于延迟切换）
        /// </summary>
        private int targetStageIndex;

        /// <summary>
        /// 游戏是否暂停
        /// </summary>
        private bool isPaused;

        /// <summary>
        /// 暂停时的播放时间
        /// </summary>
        private float pauseTime;

        /// <summary>
        /// 是否正在播放背景音乐
        /// </summary>
        private bool isPlayingBackground;
        #endregion

        #region Public Methods
        /// <summary>
        /// 开始播放背景音乐
        /// </summary>
        public void PlayBackgroundMusic()
        {
            if (backgroundMusic == null) return;
            
            StopAllTracks();
            isPlayingBackground = true;
            isPlaying = true;  // 启用Update中的循环检查
            PlayBackgroundTrack();
            nextLoopTime = Time.time + currentLoopLength;  // 设置下一个循环时间
        }

        /// <summary>
        /// 开始播放游戏音乐
        /// </summary>
        public void StartGameMusic()
        {
            isPlayingBackground = false;
            currentStageIndex = 0; // 默认从主题+鼓点开始
            targetStageIndex = currentStageIndex;
            StopAllMusic(); // 确保停止所有音轨
            StartPlayback();
        }

        /// <summary>
        /// 准备切换到指定阶段（在下一个循环开始时切换）
        /// </summary>
        public void PrepareSwitchToStage(int stageIndex)
        {
            if (stageIndex >= 0 && stageIndex < gameStages.Count)
            {
                targetStageIndex = stageIndex;
            }
        }

        /// <summary>
        /// 重新开始游戏音乐
        /// </summary>
        public void RestartGameMusic()
        {
            currentStageIndex = 0; // 回到主题+鼓点
            targetStageIndex = currentStageIndex;
            StartPlayback();
        }

        /// <summary>
        /// 停止所有音乐播放
        /// </summary>
        public void StopAllMusic()
        {
            StopAllTracks();
            isPlaying = false;
            isPlayingBackground = false;
            currentStageIndex = 0;
            targetStageIndex = 0;
        }

        /// <summary>
        /// 获取当前阶段
        /// </summary>
        public int GetCurrentStage()
        {
            return currentStageIndex;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// 初始化音频源组件
        /// </summary>
        private void InitializeAudioSources()
        {
            int maxTracks = 0;
            foreach (var stage in gameStages)
            {
                maxTracks = Mathf.Max(maxTracks, stage.tracks.Count);
            }

            for (int i = 0; i < maxTracks; i++)
            {
                var source = gameObject.AddComponent<AudioSource>();
                source.name = $"TrackSource_{i}";
                source.loop = false;
                source.playOnAwake = false;
                audioSources.Add(source);
            }
        }

        /// <summary>
        /// 验证所有阶段的设置
        /// </summary>
        private void ValidateStages()
        {
            if (gameStages == null || gameStages.Count == 0)
            {
                throw new System.Exception("MusicManager: 未设置任何游戏音乐阶段！");
            }

            foreach (var stage in gameStages)
            {
                if (stage.tracks == null || stage.tracks.Count == 0)
                {
                    Debug.LogWarning($"MusicManager: 阶段 '{stage.stageName}' 没有设置任何音轨！");
                }
            }
        }

        /// <summary>
        /// 设置循环长度
        /// </summary>
        private void SetupLoopLength()
        {
            if (!autoDetectLoopLength)
            {
                // 优先使用手动设置的固定循环长度
                currentLoopLength = manualLoopLength;
                Debug.Log($"MusicManager: 使用固定循环长度: {currentLoopLength}秒");
                
                // 自动检测误差时间
                if (backgroundMusic != null)
                {
                    errorTime = backgroundMusic.length - manualLoopLength;
                    Debug.Log($"MusicManager: 自动检测到误差时间: {errorTime}秒 (来自背景音乐)");
                }
                else if (gameStages.Count > 0 && gameStages[0].tracks.Count > 0 && gameStages[0].tracks[0] != null)
                {
                    errorTime = gameStages[0].tracks[0].length - manualLoopLength;
                    Debug.Log($"MusicManager: 自动检测到误差时间: {errorTime}秒 (来自游戏音乐)");
                }
                return;
            }
            
            // 以下是自动检测循环长度的逻辑
            if (backgroundMusic != null)
            {
                currentLoopLength = backgroundMusic.length;
                Debug.Log($"MusicManager: 自动检测到循环长度: {currentLoopLength}秒 (来自背景音乐)");
                return;
            }

            foreach (var stage in gameStages)
            {
                foreach (var track in stage.tracks)
                {
                    if (track != null)
                    {
                        currentLoopLength = track.length;
                        Debug.Log($"MusicManager: 自动检测到循环长度: {currentLoopLength}秒 (来自游戏音乐)");
                        return;
                    }
                }
            }
            throw new System.Exception("MusicManager: 未找到有效的音频片段！");
        }

        /// <summary>
        /// 开始播放当前阶段的音轨
        /// </summary>
        private void StartPlayback()
        {
            if (audioSources.Count > 0)
            {
                PlayStageTracks();
                nextLoopTime = Time.time + currentLoopLength;
                isPlaying = true;
            }
        }

        /// <summary>
        /// 播放当前阶段的所有音轨
        /// </summary>
        private void PlayStageTracks()
        {
            StopAllTracks();

            if (isPlayingBackground)
            {
                PlayBackgroundTrack();
                return;
            }

            var currentStage = gameStages[currentStageIndex];
            for (int i = 0; i < currentStage.tracks.Count && i < audioSources.Count; i++)
            {
                if (currentStage.tracks[i] != null)
                {
                    audioSources[i].clip = currentStage.tracks[i];
                    audioSources[i].time = errorTime; // 跳过误差时间
                    audioSources[i].Play();
                }
            }
        }

        /// <summary>
        /// 播放背景音乐
        /// </summary>
        private void PlayBackgroundTrack()
        {
            if (audioSources.Count > 0 && backgroundMusic != null)
            {
                var source = audioSources[0];
                source.clip = backgroundMusic;
                source.time = errorTime; // 跳过误差时间
                source.Play();
            }
        }

        /// <summary>
        /// 停止所有音轨的播放
        /// </summary>
        private void StopAllTracks()
        {
            foreach (var source in audioSources)
            {
                source.Stop();
            }
        }

        /// <summary>
        /// 暂停所有音轨的播放
        /// </summary>
        private void PauseAllTracks()
        {
            foreach (var source in audioSources)
            {
                source.Pause();
            }
        }

        /// <summary>
        /// 恢复所有音轨的播放
        /// </summary>
        private void ResumeAllTracks()
        {
            foreach (var source in audioSources)
            {
                source.UnPause();
            }
        }
        #endregion

        #region Unity Methods
        private void Awake()
        {
            InitializeAudioSources();
            ValidateStages();
            SetupLoopLength();
        }

        private void Update()
        {
            if (!isPlaying) return;

            // 处理游戏暂停
            if (Time.timeScale == 0)
            {
                if (!isPaused)
                {
                    isPaused = true;
                    pauseTime = Time.time;
                    PauseAllTracks();
                }
                return;
            }
            else if (isPaused)
            {
                isPaused = false;
                float pauseDuration = Time.time - pauseTime;
                nextLoopTime += pauseDuration;
                ResumeAllTracks();
            }

            // 检查是否需要循环
            if (Time.time >= nextLoopTime)
            {
                if (currentStageIndex != targetStageIndex)
                {
                    // 在循环结束时切换阶段
                    currentStageIndex = targetStageIndex;
                    // 直接调用AudioManager的方法
                    AudioManager.Instance?.OnMusicStageChanged(currentStageIndex);
                }

                PlayStageTracks();
                nextLoopTime = Time.time + currentLoopLength;
            }
        }
        #endregion
    }
} 
