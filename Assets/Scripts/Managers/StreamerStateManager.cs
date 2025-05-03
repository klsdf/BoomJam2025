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
        // 定义状态切换的阈值
        private const decimal CASUAL_SINGING_THRESHOLD = 0m;    // 随性唱歌阈值
        private const decimal FOCUSED_SINGING_THRESHOLD = 200m;  // 投入唱歌阈值
        private const decimal PASSIONATE_SINGING_THRESHOLD = 20000m; // 激情唱歌阈值

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
              //if (_currentState != value)
               //
                 //   Debug.Log($"主播状态从 {_currentState} 切换到 {value}");
                    _currentState = value;
                    OnStateChanged?.Invoke(_currentState);
                    HandleStateChange(_currentState);
               //
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
        }

        /// <summary>
        /// 设置状态
        /// </summary>
        public void SetState(StreamerState newState)
        {
            // 如果当前状态和目标状态都是唱歌状态且相同，则不触发切换
            if (IsInSingingState() && newState == CurrentState)
            {
               // Debug.Log("当前状态和目标状态都是唱歌状态且相同，不触发切换");
                return;
            }
            //Debug.Log($"尝试设置主播状态为: {newState}");
            CurrentState = newState;

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
           // Debug.Log($"处理状态变化: {newState}");
            
            // 通知UI管理器准备切换状态
            if (StreamerUIManager.Instance != null)
            {
                StreamerUIManager.Instance.PrepareSwitchToState(newState);
            }
            
            // 根据新状态播放相应的音乐
            switch (newState)
            {
                case StreamerState.Chatting:
                   // Debug.Log("切换到闲聊状态");
                    AudioManager.Instance.StopAllMusic();
                    AudioManager.Instance.PlayBackgroundMusic();
                    break;
                case StreamerState.CasualSinging:
                  //  Debug.Log("切换到随性唱歌状态，切换到阶段0");
                    AudioManager.Instance.StartGameMusic();
                    AudioManager.Instance.SwitchToStage(0);
                    break;
                case StreamerState.FocusedSinging:
                  //  Debug.Log("切换到投入唱歌状态，切换到阶段1");
                    AudioManager.Instance.SwitchToStage(1);
                    break;
                case StreamerState.PassionateSinging:
                  //  Debug.Log("切换到激情唱歌状态，切换到阶段2");
                    AudioManager.Instance.SwitchToStage(2);
                    break;
            }
        }

        /// <summary>
        /// 根据最大每秒普通礼物贡献值更新主播状态
        /// </summary>
        public void UpdateStateBasedOnContribution(decimal maxNormalValuePerSecond)
        {
            // 如果当前状态是Chatting，则直接返回，不进行任何状态更新
            if (CurrentState == StreamerState.Chatting)
            {
                //Debug.Log("当前状态是Chatting，不进行状态更新");
                return;
            }

            if (maxNormalValuePerSecond >= PASSIONATE_SINGING_THRESHOLD)
            {
              //  Debug.Log($"达到激情唱歌阈值: {PASSIONATE_SINGING_THRESHOLD}");
                SetState(StreamerState.PassionateSinging);
            }
            else if (maxNormalValuePerSecond >= FOCUSED_SINGING_THRESHOLD)
            {
              //  Debug.Log($"达到投入唱歌阈值: {FOCUSED_SINGING_THRESHOLD}");
                SetState(StreamerState.FocusedSinging);
            }
            else if (maxNormalValuePerSecond >= CASUAL_SINGING_THRESHOLD)
            {
              //  Debug.Log($"达到随性唱歌阈值: {CASUAL_SINGING_THRESHOLD}");
                SetState(StreamerState.CasualSinging);
            }
            else
            {
               // Debug.Log("未达到任何唱歌阈值，保持闲聊状态");
                SetState(StreamerState.Chatting);
            }
        }

        /// <summary>
        /// 开始运行
        /// </summary>
        public void StartRunning()
        {
            SetState(StreamerState.Chatting);
            
        }

        /// <summary>
        /// 停止运行
        /// </summary>
        public void StopRunning()
        {
            SetState(StreamerState.Chatting);
        }

        /// <summary>
        /// 暂停所有相关功能
        /// </summary>
        public void PauseAll()
        {
            // 暂停音乐
            AudioManager.Instance.PauseAllMusic();
            // 暂停主播动画
            StreamerUIManager.Instance.PauseStreamerAnimation();
            Debug.Log("暂停所有相关功能");
        }

        /// <summary>
        /// 继续所有相关功能
        /// </summary>
        public void ResumeAll()
        {
            // 继续音乐
            AudioManager.Instance.ResumeAllMusic();
            // 继续主播动画
            StreamerUIManager.Instance.ResumeStreamerAnimation();
        }

        private bool isPaused = false;

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
            // 调试功能：通过P键暂停/继续
            else if (Input.GetKeyDown(KeyCode.P))
            {
                Debug.Log("暂停/继续");
                if (isPaused)
                {
                    ResumeAll();
                    isPaused = false;
                }
                else
                {
                    PauseAll();
                    isPaused = true;
                }
            }
            
            //打印当前状态
         // Debug.Log($"当前状态: {CurrentState}");
        }
    }
}
