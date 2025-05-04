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

        private bool isCountdownStarted = false;
        private bool isCountdownPlayed = false;

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
            
            // 设置开始时间为23:58:23
            startDateTime = DateTime.Today.AddHours(23).AddMinutes(58).AddSeconds(23);

            // 开场剧情结束后开始游戏
            isTimerRunning = true;
            isGamePaused = false;
            Time.timeScale = 1f;
            
            // 启动开场对话但不等待完成
            StartCoroutine(DialogueManager.Instance.StartDialogueAfterCurrent("StreamerStart"));
            // 等待30秒（原StreamerStart对话时间）
            yield return new WaitForSeconds(30f);
            

            
            // 启动循环开始对话但不等待完成
            StartCoroutine(DialogueManager.Instance.StartDialogueAfterCurrent("LoopStart"));
            // 等待7秒（原LoopStart对话时间）
            yield return new WaitForSeconds(7f);

            // 启用礼物系统
            GiftManager.Instance.StartRunning();

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
                
                // 检查是否应该播放倒计时
                if (isCountdownStarted && !isCountdownPlayed)
                {
                    CheckCountdownTime();
                }
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
            PauseAllSystems();
            yield return VNDialogueManager.Instance.StartDialogueCoroutine("Teach_Gift");
            ResumeAllSystems();
        }
        
        /// <summary>
        /// 播放Teach_FanLevel（协程版本）
        /// </summary>
        private IEnumerator PlayTeachFanLevelCoroutine()
        {
            PauseAllSystems();
            yield return VNDialogueManager.Instance.StartDialogueCoroutine("Teach_FanLevel");
            ResumeAllSystems();
        }
        
        /// <summary>
        /// 播放Teach_FanLevel（回调版本）
        /// </summary>
        public void PlayTeachFanLevel()
        {
            StartCoroutine(PlayTeachFanLevelCoroutine());
        }
        
        /// <summary>
        /// 播放Teach_MemberLevel（协程版本）
        /// </summary>
        private IEnumerator PlayTeachMemberLevelCoroutine()
        {
            PauseAllSystems();
            yield return VNDialogueManager.Instance.StartDialogueCoroutine("Teach_MemberLevel");
            ResumeAllSystems();
        }
        
        /// <summary>
        /// 播放Teach_MemberLevel（回调版本）
        /// </summary>
        public void PlayTeachMemberLevel()
        {
            StartCoroutine(PlayTeachMemberLevelCoroutine());
        }
        
        /// <summary>
        /// 播放Teach_MemberBenefit（协程版本）
        /// </summary>
        private IEnumerator PlayTeachMemberBenefitCoroutine()
        {
            PauseAllSystems();
            yield return VNDialogueManager.Instance.StartDialogueCoroutine("Teach_MemberBenefit");
            ResumeAllSystems();
        }

        /// <summary>
        /// 播放Teach_MemberBenefit（回调版本）
        /// </summary>
        public void PlayTeachMemberBenefit()
        {
            StartCoroutine(PlayTeachMemberBenefitCoroutine());
        }

        /// <summary>
        /// 播放大结局
        /// </summary>
        private IEnumerator PlayGrandFinale()
        {
            // TODO: 在这里添加大结局的演出逻辑
            StreamerStateManager.Instance.SetState(StreamerState.Chatting);

            yield return DialogueManager.Instance.StartDialogueAfterCurrent("FinalEnd");
            yield return VNDialogueManager.Instance.StartDialogueCoroutine("VN_FinalEnd");
            // 返回主菜单
            yield return new WaitForSeconds(1f);
            StreamerUIManager.Instance.StopAllCoroutines();
            GameManager.Instance.ChangeGameState(GameState.MainMenu);
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
            // GiftManager.Instance.EnableGiftGeneration();
        }

        private void RestartGame()
        {
            ResumeGame();
            isTimeEnd = false;
            InitializeTime();
            // 停止当前对话
            DialogueManager.Instance.StopDialogueCoroutine();
            
            // 启动倒计时检测
            StartCountdownCheck();
            
            // 播放循环开始对话
            StartCoroutine(PlayLoopStartCoroutine());
            GiftManager.Instance.ClearAllGifts();
            CommentManager.Instance.ClearComments();
            RebirthManager.Instance.TryRebirth();
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
                isTimerRunning = true;
                isGamePaused = false;
                Time.timeScale = 1f;
                UIManager.Instance.HideInputBlocker();
                
                // 启动倒计时检测
                StartCountdownCheck();
                
                // 播放循环开始对话
                StartCoroutine(PlayLoopStartCoroutine());
            }
        }

        //播放循环后开始说话的协程方法，在结束时触发切换为唱歌状态
        private IEnumerator PlayLoopStartCoroutine()
        {
            StreamerStateManager.Instance.SetState(StreamerState.Chatting);
            // 启动循环开始对话但不等待完成
            StartCoroutine(DialogueManager.Instance.StartDialogueAfterCurrent("LoopStart"));
            // 等待7秒（原LoopStart对话时间）
            yield return new WaitForSeconds(7f);
            StreamerStateManager.Instance.SetState(StreamerState.CasualSinging);
            if(RebirthManager.Instance.countRebirth == 1)
            {
                StartCoroutine(PlaySecondStart());
                Debug.Log("播放第二次开始");
                // 启用按钮第一次点击的检测
                ButtonFirstClickManager.Instance.EnableFirstClickDetection();
            }
            // 启用礼物系统
            // 如果不是第二次开始的教程阶段,才启用礼物系统
            if (RebirthManager.Instance.countRebirth != 1)
            {
                GiftManager.Instance.EnableGiftGeneration();
            }
        }

        /// <summary>
        /// 检查是否应该播放倒计时
        /// </summary>
        private void CheckCountdownTime()
        {
            DateTime currentTime = startDateTime.AddSeconds(gameTime);
            
            // 目标时间：23:59:50
            if (currentTime.Hour == 23 && currentTime.Minute == 59 && currentTime.Second >= 50)
            {
                Debug.Log($"时间到达23:59:50，播放倒计时音效，当前时间：{currentTime:HH:mm:ss}");
                
                try
                {
                    if (AudioManager.Instance != null)
                    {
                        AudioManager.Instance.PlayCountdown();
                        Debug.Log("倒计时音效播放指令已发送");
                    }
                    else
                    {
                        Debug.LogError("错误：AudioManager实例为空");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError("播放倒计时音效时发生异常: " + e.Message);
                }
                
                isCountdownPlayed = true;
            }
        }
        
        /// <summary>
        /// 开始倒计时检测
        /// </summary>
        public void StartCountdownCheck()
        {
            isCountdownStarted = true;
            isCountdownPlayed = false;
            Debug.Log("倒计时检测已开始");
        }
        
        /// <summary>
        /// 重置倒计时状态
        /// </summary>
        private void ResetCountdown()
        {
            isCountdownStarted = false;
            isCountdownPlayed = false;
        }

        public void StopRunning()
        {
            isTimerRunning = false;
            isGamePaused = true;
            Time.timeScale = 0f;
            UIManager.Instance.ShowInputBlocker();
            GiftManager.Instance.DisableGiftGeneration();
        }
        
        /// <summary>
        /// 暂停所有系统（主播状态、评论、礼物）
        /// </summary>
        private void PauseAllSystems()
        {
            isTimerRunning = false;
            StreamerStateManager.Instance.PauseAll();
            CommentManager.Instance.PauseComments();
            GiftManager.Instance.DisableGiftGeneration();
            AudioManager.Instance.PauseCountdown();
        }
        
        /// <summary>
        /// 恢复所有系统（主播状态、评论、礼物）
        /// </summary>
        private void ResumeAllSystems()
        {
            isTimerRunning = true;
            StreamerStateManager.Instance.ResumeAll();
            CommentManager.Instance.ResumeComments();
            GiftManager.Instance.EnableGiftGeneration();
            AudioManager.Instance.ResumeCountdown();
        }
    }
}
