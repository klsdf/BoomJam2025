using UnityEngine;
using System;
using System.Collections;

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

        private StreamerState targetState;  // 目标状态
        private bool isStateChangePending;  // 是否有状态切换待处理
        private float nextSwitchTime;       // 下一次切换时间

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
                    break;
                case StreamerState.CasualSinging:
                    if (casualSingingStreamer != null) casualSingingStreamer.SetActive(true);
                    break;
                case StreamerState.FocusedSinging:
                    if (focusedSingingStreamer != null) focusedSingingStreamer.SetActive(true);
                    break;
                case StreamerState.PassionateSinging:
                    if (passionateStreamer != null) passionateStreamer.SetActive(true);
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
    }
} 