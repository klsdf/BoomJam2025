namespace BoomJam2025
{
    using UnityEngine;
    using System;

    public enum StreamerState
    {
        Chatting,           // 闲聊
        CasualSinging,     // 唱歌（随性）
        FocusedSinging,    // 唱歌（投入）
        PassionateSinging  // 唱歌（激情）
    }

    public class StreamerStateManager : MonoBehaviour
    {
        private static StreamerStateManager _instance;
        public static StreamerStateManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("StreamerStateManager");
                    _instance = go.AddComponent<StreamerStateManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        private StreamerState _currentState = StreamerState.Chatting;

        public StreamerState CurrentState
        {
            get => _currentState;
            private set
            {
                if (_currentState != value)
                {
                    Debug.Log($"主播状态从 {_currentState} 切换到 {value}");
                    _currentState = value;
                    OnStateChanged?.Invoke(_currentState);
                    HandleStateChange(_currentState);
                }
            }
        }

        public delegate void StateChangedHandler(StreamerState newState);
        public event StateChangedHandler OnStateChanged;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
            AudioManager.Instance.StartGameMusic();
        }

        /// <summary>
        /// 设置状态
        /// </summary>
        public void SetState(StreamerState newState)
        {
            Debug.Log($"尝试设置主播状态为: {newState}");
            CurrentState = newState;
            HandleStateChange(newState);
        }

        /// <summary>
        /// 判断是否在唱歌状态
        /// </summary>
        public bool IsInSingingState()
        {
            return CurrentState == StreamerState.CasualSinging ||
                   CurrentState == StreamerState.FocusedSinging ||
                   CurrentState == StreamerState.PassionateSinging;
        }

        /// <summary>
        /// 处理状态变化
        /// </summary>
        private void HandleStateChange(StreamerState newState)
        {
            Debug.Log($"处理状态变化: {newState}");
            
            // 通知UI管理器准备切换状态
            if (StreamerUIManager.Instance != null)
            {
                StreamerUIManager.Instance.PrepareSwitchToState(newState);
            }
            
            // 根据新状态播放相应的音乐
            switch (newState)
            {
                case StreamerState.Chatting:
                    Debug.Log("切换到闲聊状态");
                    AudioManager.Instance.StopAllMusic();
                    AudioManager.Instance.PlayBackgroundMusic();
                    break;
                case StreamerState.CasualSinging:
                    Debug.Log("切换到随性唱歌状态，切换到阶段0");
                    AudioManager.Instance.StopAllMusic();
                    AudioManager.Instance.StartGameMusic();

                    AudioManager.Instance.SwitchToStage(0);
                    break;
                case StreamerState.FocusedSinging:
                    Debug.Log("切换到投入唱歌状态，切换到阶段1");
                    AudioManager.Instance.SwitchToStage(1);
                    break;
                case StreamerState.PassionateSinging:
                    Debug.Log("切换到激情唱歌状态，切换到阶段2");
                    AudioManager.Instance.SwitchToStage(2);
                    break;
            }
        }

        private void Update()
        {
            // 调试功能：通过按键切换状态
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                SetState(StreamerState.Chatting);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                SetState(StreamerState.CasualSinging);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                SetState(StreamerState.FocusedSinging);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                SetState(StreamerState.PassionateSinging);
            }
        }
    }
}
