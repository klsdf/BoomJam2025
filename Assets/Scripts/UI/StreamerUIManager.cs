using UnityEngine;
using System;
using System.Collections;
using MoreMountains.Feedbacks;

namespace BoomJam2025
{
    public class StreamerUIManager : MonoBehaviour
    {
        private static StreamerUIManager _instance;
        public static StreamerUIManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("StreamerUIManager");
                    _instance = go.AddComponent<StreamerUIManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        [Header("主播预制体")]
        [SerializeField] private GameObject chattingStreamer;        // 闲聊状态预制体实例
        [SerializeField] private GameObject casualSingingStreamer;  // 随性唱歌状态预制体实例
        [SerializeField] private GameObject focusedSingingStreamer; // 投入唱歌状态预制体实例
        [SerializeField] private GameObject passionateStreamer;     // 激情状态预制体实例

        [Header("MMF播放器")]
        [Tooltip("闲聊状态晃动效果")]
        [SerializeField] private MMF_Player chattingShakeMMF;
        [Tooltip("随性唱歌状态晃动效果")]
        [SerializeField] private MMF_Player casualSingingShakeMMF;
        [Tooltip("投入唱歌状态晃动效果")]
        [SerializeField] private MMF_Player focusedSingingShakeMMF;
        [Tooltip("激情状态晃动效果")]
        [SerializeField] private MMF_Player passionateSingingShakeMMF;

        [Header("晃动参数设置")]
        [Tooltip("基础晃动频率（每秒）")]
        [SerializeField] private float shakeFrequency = 1f;
        [Tooltip("闲聊状态晃动间隔倍率")]
        [SerializeField] private int chattingShakeMultiplier = 3;
        [Tooltip("随性唱歌晃动间隔倍率")]
        [SerializeField] private int casualSingingShakeMultiplier = 2;
        [Tooltip("投入唱歌晃动间隔倍率")]
        [SerializeField] private int focusedSingingShakeMultiplier = 1;
        [Tooltip("激情状态晃动间隔倍率")]
        [SerializeField] private int passionateShakeMultiplier = 1;

        private StreamerState targetState;  // 目标状态
        private bool isStateChangePending;  // 是否有状态切换待处理
        private float nextSwitchTime;       // 下一次切换时间

        // 晃动相关
        private Coroutine chattingShakeCoroutine;
        private Coroutine casualSingingShakeCoroutine;
        private Coroutine focusedSingingShakeCoroutine;
        private Coroutine passionateShakeCoroutine;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            // 移除事件订阅协程
            // StartCoroutine(SubscribeToMusicEvents());
        }

        private void OnDestroy()
        {
            // 停止所有晃动协程
            StopAllShakeCoroutines();
            
            // 移除事件取消订阅
            // if (AudioManager.Instance != null && AudioManager.Instance.MusicManager != null)
            // {
            //     AudioManager.Instance.MusicManager.OnStageChanged -= OnMusicLoop;
            // }
        }

        /// <summary>
        /// 准备切换到指定状态
        /// </summary>
        public void PrepareSwitchToState(StreamerState newState)
        {
            targetState = newState;
            
            if (newState == StreamerState.Chatting || newState == StreamerState.CasualSinging)
            {
                ShowStreamer(targetState);
                isStateChangePending = false;
            }
            else
            {
                isStateChangePending = true;
            }
        }

        /// <summary>
        /// 显示指定状态的主播
        /// </summary>
        private void ShowStreamer(StreamerState state)
        {
            // 停止所有晃动协程
            StopAllShakeCoroutines();
            
            // 隐藏所有实例
            if (chattingStreamer != null) chattingStreamer.SetActive(false);
            if (casualSingingStreamer != null) casualSingingStreamer.SetActive(false);
            if (focusedSingingStreamer != null) focusedSingingStreamer.SetActive(false);
            if (passionateStreamer != null) passionateStreamer.SetActive(false);

            // 显示对应状态的实例
            switch (state)
            {
                case StreamerState.Chatting:
                    if (chattingStreamer != null) chattingStreamer.SetActive(true);
                    StartChattingShake();
                    break;
                case StreamerState.CasualSinging:
                    if (casualSingingStreamer != null) casualSingingStreamer.SetActive(true);
                    StartCasualSingingShake();
                    break;
                case StreamerState.FocusedSinging:
                    if (focusedSingingStreamer != null) focusedSingingStreamer.SetActive(true);
                    StartFocusedSingingShake();
                    break;
                case StreamerState.PassionateSinging:
                    if (passionateStreamer != null) passionateStreamer.SetActive(true);
                    StartPassionateSingingShake();
                    break;
            }
        }

        public void OnMusicStageChanged(int stageIndex)
        {
            Debug.Log("音乐阶段改变时的回调");
            if (isStateChangePending)
            {
                ShowStreamer(targetState);
                isStateChangePending = false;
            }
        }

        /// <summary>
        /// 暂停主播动画
        /// </summary>
        public void PauseStreamerAnimation()
        {
            StopAllShakeCoroutines();
        }

        /// <summary>
        /// 继续主播动画
        /// </summary>
        public void ResumeStreamerAnimation()
        {
            // 根据当前显示的主播状态，重新开始晃动
            if (chattingStreamer != null && chattingStreamer.activeSelf)
                StartChattingShake();
            else if (casualSingingStreamer != null && casualSingingStreamer.activeSelf)
                StartCasualSingingShake();
            else if (focusedSingingStreamer != null && focusedSingingStreamer.activeSelf)
                StartFocusedSingingShake();
            else if (passionateStreamer != null && passionateStreamer.activeSelf)
                StartPassionateSingingShake();
        }

        #region 晃动方法

        /// <summary>
        /// 开始闲聊状态晃动
        /// </summary>
        public void StartChattingShake()
        {
            StopChattingShake();
            if (chattingShakeMMF != null)
            {
                chattingShakeCoroutine = StartCoroutine(ShakeCoroutine(chattingShakeMMF, chattingShakeMultiplier));
            }
        }

        /// <summary>
        /// 停止闲聊状态晃动
        /// </summary>
        public void StopChattingShake()
        {
            if (chattingShakeCoroutine != null)
            {
                StopCoroutine(chattingShakeCoroutine);
                chattingShakeCoroutine = null;
            }
        }

        /// <summary>
        /// 开始随性唱歌状态晃动
        /// </summary>
        public void StartCasualSingingShake()
        {
            StopCasualSingingShake();
            if (casualSingingShakeMMF != null)
            {
                casualSingingShakeCoroutine = StartCoroutine(ShakeCoroutine(casualSingingShakeMMF, casualSingingShakeMultiplier));
            }
        }

        /// <summary>
        /// 停止随性唱歌状态晃动
        /// </summary>
        public void StopCasualSingingShake()
        {
            if (casualSingingShakeCoroutine != null)
            {
                StopCoroutine(casualSingingShakeCoroutine);
                casualSingingShakeCoroutine = null;
            }
        }

        /// <summary>
        /// 开始投入唱歌状态晃动
        /// </summary>
        public void StartFocusedSingingShake()
        {
            StopFocusedSingingShake();
            if (focusedSingingShakeMMF != null)
            {
                focusedSingingShakeCoroutine = StartCoroutine(ShakeCoroutine(focusedSingingShakeMMF, focusedSingingShakeMultiplier));
            }
        }

        /// <summary>
        /// 停止投入唱歌状态晃动
        /// </summary>
        public void StopFocusedSingingShake()
        {
            if (focusedSingingShakeCoroutine != null)
            {
                StopCoroutine(focusedSingingShakeCoroutine);
                focusedSingingShakeCoroutine = null;
            }
        }

        /// <summary>
        /// 开始激情状态晃动
        /// </summary>
        public void StartPassionateSingingShake()
        {
            StopPassionateSingingShake();
            if (passionateSingingShakeMMF != null)
            {
                passionateShakeCoroutine = StartCoroutine(ShakeCoroutine(passionateSingingShakeMMF, passionateShakeMultiplier));
            }
        }

        /// <summary>
        /// 停止激情状态晃动
        /// </summary>
        public void StopPassionateSingingShake()
        {
            if (passionateShakeCoroutine != null)
            {
                StopCoroutine(passionateShakeCoroutine);
                passionateShakeCoroutine = null;
            }
        }

        /// <summary>
        /// 停止所有晃动协程
        /// </summary>
        private void StopAllShakeCoroutines()
        {
            StopChattingShake();
            StopCasualSingingShake();
            StopFocusedSingingShake();
            StopPassionateSingingShake();
        }

        /// <summary>
        /// 晃动协程
        /// </summary>
        private IEnumerator ShakeCoroutine(MMF_Player mmfPlayer, int intervalMultiplier)
        {
            while (true)
            {
                mmfPlayer.PlayFeedbacks();
                yield return new WaitForSeconds(intervalMultiplier / shakeFrequency);
            }
        }

        #endregion
    }
} 