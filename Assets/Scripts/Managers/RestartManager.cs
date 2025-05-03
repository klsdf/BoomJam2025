namespace BoomJam2025
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using System;
    using TMPro;

    public class RestartManager : MonoBehaviour
    {
        private static RestartManager _instance;
        public static RestartManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<RestartManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("RestartManager");
                        _instance = go.AddComponent<RestartManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        [Header("时间配置")]
        [Tooltip("开始小时（24小时制）")]
        public int startHour = 0;
        [Tooltip("开始分钟")]
        public int startMinute = 0;
        [Tooltip("开始秒")]
        public int startSecond = 0;
        [Tooltip("持续时间（小时）")]
        public int durationHours = 0;
        [Tooltip("持续时间（分钟）")]
        public int durationMinutes = 0;
        [Tooltip("持续时间（秒）")]
        public int durationSeconds = 0;

        [Header("UI引用")]
        [Tooltip("显示时间的Text组件")]
        public TextMeshProUGUI textTimeDisplay;

        private float gameTime = 0f;
        private bool isGamePaused = false;
        private DateTime startDateTime;
        private DateTime endDateTime;
        private bool isTimeEnd = false;
        public bool isTimerRunning { get; private set; } = false;

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

        // Start is called before the first frame update
        void Start()
        {
            InitializeTime();
        }
        
        /// <summary>
        /// 播放开场剧情
        /// </summary>
        private IEnumerator PlayOpeningSequence()
        {
            // 开场对话期间禁用礼物系统
            GiftManager.Instance.StopRunning();
         StreamerStateManager.Instance.SetState(StreamerState.Chatting);
            
            // 等待开场对话完成
            yield return DialogueManager.Instance.StartDialogueAfterCurrent("StreamerStart");
            
            // 开场剧情结束后开始游戏
            isTimerRunning = true;
            isGamePaused = false;
            Time.timeScale = 1f;
            
            // 启用礼物系统
            GiftManager.Instance.StartRunning();
            
            // 播放循环开始对话
            yield return DialogueManager.Instance.StartDialogueAfterCurrent("LoopStart");


            //设置主播状态为唱歌
            StreamerStateManager.Instance.SetState(StreamerState.CasualSinging);
        }

        // Update is called once per frame
        void Update()
        {
            if (isTimerRunning && !isGamePaused && !isTimeEnd)
            {
                gameTime += Time.deltaTime;
                UpdateTimeDisplay();
                CheckTimeLimit();
            }
        }

        private void InitializeTime()
        {
            // 设置开始时间
            startDateTime = DateTime.Today.AddHours(startHour).AddMinutes(startMinute).AddSeconds(startSecond);
            // 计算结束时间
            endDateTime = startDateTime.AddHours(durationHours).AddMinutes(durationMinutes).AddSeconds(durationSeconds);
            gameTime = 0f;
            isTimeEnd = false;
        }

        private void UpdateTimeDisplay()
        {
            if (textTimeDisplay != null)
            {
                DateTime currentTime = startDateTime.AddSeconds(gameTime);
                textTimeDisplay.text = currentTime.ToString("HH:mm:ss");
            }
        }

        private void CheckTimeLimit()
        {
            DateTime currentTime = startDateTime.AddSeconds(gameTime);
            if (currentTime > endDateTime)
            {
                OnTimeEnd();
            }
        }

        /// <summary>
        /// 时间结束
        /// </summary>
        private void OnTimeEnd()
        {
            isTimeEnd = true;

            GiftManager.Instance.DisableGiftGeneration();
            
            // 检查是否达到1 trillion
            if (CoreValueManager.Instance.valueContribution >= 1000000000000m)
            {
                StartCoroutine(PlayGrandFinale());
            }
            else if(RebirthManager.Instance.countRebirth == 0)
            {
                StartCoroutine(PlayFirstEnding());
            }
            else
            {
                StartCoroutine(PlayNormalEnding());
            }
        }

        /// <summary>
        /// 播放第一次结局
        /// </summary>
        private IEnumerator PlayFirstEnding()
        {
            // TODO: 在这里添加普通结局的演出逻辑
            
            //设置主播状态为闲聊
            StreamerStateManager.Instance.SetState(StreamerState.Chatting);
            
            // 分别执行两个对话
            yield return DialogueManager.Instance.StartDialogueAfterCurrent("NormalEnd");
            yield return VNDialogueManager.Instance.StartDialogueCoroutine("FirstDie");
            UIManager.Instance.ShowTimeEndPanel();
        }

        /// <summary>
        /// 播放普通结局
        /// </summary>
        private IEnumerator PlayNormalEnding()
        {
            // TODO: 在这里添加普通结局的演出逻辑
            
            //设置主播状态为闲聊
            StreamerStateManager.Instance.SetState(StreamerState.Chatting);

            // 并行执行两个对话
            StartCoroutine(DialogueManager.Instance.StartDialogueAfterCurrent("NormalEnd"));
            yield return VNDialogueManager.Instance.StartDialogueCoroutine("VN_NormalEnd");
            UIManager.Instance.ShowTimeEndPanel();
        }

        /// <summary>
        /// 播放第二次开始
        /// </summary>
        private IEnumerator PlaySecondStart()
        {
            isTimerRunning = false;
            yield return VNDialogueManager.Instance.StartDialogueCoroutine("Teach_Gift");
            isTimerRunning = true;
        }
        /// <summary>
        /// 播放Teach_FanLevel（协程版本）
        /// </summary>
        private IEnumerator PlayTeachFanLevelCoroutine()
        {
            isTimerRunning = false;
            yield return VNDialogueManager.Instance.StartDialogueCoroutine("Teach_FanLevel");
            isTimerRunning = true;
        }
        /// <summary>
        /// 播放Teach_FanLevel（回调版本）
        /// </summary>
        public void PlayTeachFanLevel()
        {
            isTimerRunning = false;
            StartCoroutine(PlayTeachFanLevelCoroutine());
        }
        /// <summary>
        /// 播放Teach_MemberLevel（协程版本）
        /// </summary>
        private IEnumerator PlayTeachMemberLevelCoroutine()
        {
            isTimerRunning = false;
            yield return VNDialogueManager.Instance.StartDialogueCoroutine("Teach_MemberLevel");
            isTimerRunning = true;
        }
        /// <summary>
        /// 播放Teach_MemberLevel（回调版本）
        /// </summary>
        public void PlayTeachMemberLevel()
        {
            isTimerRunning = false;
            StartCoroutine(PlayTeachMemberLevelCoroutine());
        }
        /// <summary>
        /// 播放Teach_MemberBenefit（协程版本）
        /// </summary>
        private IEnumerator PlayTeachMemberBenefitCoroutine()
        {
            isTimerRunning = false;
            yield return VNDialogueManager.Instance.StartDialogueCoroutine("Teach_MemberBenefit");
            isTimerRunning = true;
        }
        /// <summary>
        /// 播放Teach_MemberBenefit（回调版本）
        /// </summary>
        public void PlayTeachMemberBenefit()
        {
            isTimerRunning = false;
            StartCoroutine(PlayTeachMemberBenefitCoroutine());
        }
        /// <summary>
        /// 播放大结局
        /// </summary>
        private IEnumerator PlayGrandFinale()
        {
            // TODO: 在这里添加大结局的演出逻辑

            yield return DialogueManager.Instance.StartDialogueAfterCurrent("FinalEnd");
            yield return VNDialogueManager.Instance.StartDialogueCoroutine("VN_FinalEnd");
        }

        public void ResumeGame()
        {
            isGamePaused = false;
            isTimerRunning = true;
            Time.timeScale = 1f;
            UIManager.Instance.HideInputBlocker();
            UIManager.Instance.HideRestartPanel();
            UIManager.Instance.HideTimeEndPanel();
            UIManager.Instance.HidePausePanel();
            GiftManager.Instance.EnableGiftGeneration();
        }

        private void RestartGame()
        {
            ResumeGame();
            isTimeEnd = false;
            InitializeTime();
            // 停止当前对话
            DialogueManager.Instance.StopDialogueCoroutine();
            // 播放循环开始对话
            StartCoroutine(PlayLoopStartCoroutine());
            if(RebirthManager.Instance.countRebirth == 0)
            {
                StartCoroutine(PlaySecondStart());
                // 启用按钮第一次点击的检测
                ButtonFirstClickManager.Instance.EnableFirstClickDetection();
            }
            GiftManager.Instance.ClearAllGifts();
            CommentManager.Instance.ClearComments();
            RebirthManager.Instance.TryRebirth();
            //dioManager.Instance.StartGameMusic();
        }
        public void OnRestartButtonClicked()
        {
            RestartGame();
        }

        public float GetGameTime()
        {
            return gameTime;
        }

        public void PauseGame()
        {
            isGamePaused = true;
            Time.timeScale = 0f;
            GiftManager.Instance.DisableGiftGeneration();
        }

        public void OnAdvanceCancelButtonClicked()
        {
            ResumeGame();
        }

        public void StartRunning()
        {
            // 开场对话期间禁用礼物系统
            GiftManager.Instance.DisableGiftGeneration();
            // 如果是第一次运行，播放开场剧情
            if (RebirthManager.Instance.countRebirth == 0)
            {
                StartCoroutine(PlayOpeningSequence());
            }
            else
            {
                // 启用礼物系统
                GiftManager.Instance.EnableGiftGeneration();
                isTimerRunning = true;
                isGamePaused = false;
                Time.timeScale = 1f;
                UIManager.Instance.HideInputBlocker();
                // 播放循环开始对话
                StartCoroutine(PlayLoopStartCoroutine());
            }
        }

        //播放循环后开始说话的协程方法，在结束时触发切换为唱歌状态
        private IEnumerator PlayLoopStartCoroutine()
        {
            StreamerStateManager.Instance.SetState(StreamerState.Chatting);
            yield return DialogueManager.Instance.StartDialogueAfterCurrent("LoopStart");
            StreamerStateManager.Instance.SetState(StreamerState.CasualSinging);
        }

        public void StopRunning()
        {
            isTimerRunning = false;
            isGamePaused = true;
            Time.timeScale = 0f;
            UIManager.Instance.ShowInputBlocker();
            GiftManager.Instance.DisableGiftGeneration();
        }
    }
}
