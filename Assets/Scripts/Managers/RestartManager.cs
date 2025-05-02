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
        [Tooltip("时间结束提示面板")]
        public GameObject timeEndPanel;
        [Tooltip("提前重开提示面板")]
        public GameObject restartPanel;
        [Tooltip("暂停时禁用玩家输入的遮罩")]
        public GameObject inputBlocker;

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
            if (timeEndPanel != null)
            {
                timeEndPanel.SetActive(false);
            }
            if (inputBlocker != null)
            {
                inputBlocker.SetActive(false);
            }
            if (restartPanel != null)
            {
                restartPanel.SetActive(false);
            }
        }
        
        /// <summary>
        /// 播放开场剧情
        /// </summary>
        private IEnumerator PlayOpeningSequence()
        {
            // 开场对话期间禁用礼物系统
            GiftManager.Instance.StopRunning();
            
            // 等待开场对话完成
            yield return DialogueManager.Instance.StartDialogueAfterCurrent("StreamerStart");
            
            // 开场剧情结束后开始游戏
            isTimerRunning = true;
            isGamePaused = false;
            Time.timeScale = 1f;
            if (inputBlocker != null)
            {
                inputBlocker.SetActive(false);
            }
            
            // 启用礼物系统
            GiftManager.Instance.StartRunning();
            
            // 播放循环开始对话
            yield return DialogueManager.Instance.StartDialogueAfterCurrent("LoopStart");
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
            
            // 并行执行两个对话
            StartCoroutine(DialogueManager.Instance.StartDialogueAfterCurrent("NormalEnd"));
            yield return VNDialogueManager.Instance.StartDialogueCoroutine("FirstDie");

            if (timeEndPanel != null)
            {
                timeEndPanel.SetActive(true);
            }
            if (inputBlocker != null)
            {
                inputBlocker.SetActive(true);
            }
        }

        /// <summary>
        /// 播放普通结局
        /// </summary>
        private IEnumerator PlayNormalEnding()
        {
            // TODO: 在这里添加普通结局的演出逻辑
            
            StartCoroutine(DialogueManager.Instance.StartDialogueAfterCurrent("NormalEnd"));
            yield return VNDialogueManager.Instance.StartDialogueCoroutine("VN_NormalEnd");
            if (timeEndPanel != null)
            {
                timeEndPanel.SetActive(true);
            }
            if (inputBlocker != null)
            {
                inputBlocker.SetActive(true);
            }
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

            if (timeEndPanel != null)
            {
                timeEndPanel.SetActive(true);
            }
            if (inputBlocker != null)
            {
                inputBlocker.SetActive(true);
            }
        }

        private void ResumeGame()
        {
            isGamePaused = false;
            isTimerRunning = true;
            Time.timeScale = 1f;
            if (inputBlocker != null)
            {
                inputBlocker.SetActive(false);
            }
            if (restartPanel != null)
            {
                restartPanel.SetActive(false);
            }
            if (timeEndPanel != null)
            {
                timeEndPanel.SetActive(false);
            }
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
            StartCoroutine(DialogueManager.Instance.StartDialogueAfterCurrent("LoopStart"));
            if(RebirthManager.Instance.countRebirth == 0)
            {
                StartCoroutine(PlaySecondStart());
                // 启用按钮第一次点击的检测
                ButtonFirstClickManager.Instance.EnableFirstClickDetection();
            }
            GiftManager.Instance.ClearAllGifts();
            CommentManager.Instance.ClearComments();
            RebirthManager.Instance.TryRebirth();
            AudioManager.Instance.StartGameMusic();
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

        public void OnAdvanceRestartButtonClicked()
        {
            StopRunning();
            if (restartPanel != null)
            {
                restartPanel.SetActive(true);
            }
            if (inputBlocker != null)
            {
                inputBlocker.SetActive(true);
            }
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
                if (inputBlocker != null)
                {
                    inputBlocker.SetActive(false);
                }        
                // 播放循环开始对话
                StartCoroutine(DialogueManager.Instance.StartDialogueAfterCurrent("LoopStart"));
            }
        }

        public void StopRunning()
        {
            isTimerRunning = false;
            isGamePaused = true;
            Time.timeScale = 0f;
            if (inputBlocker != null)
            {
                inputBlocker.SetActive(true);
            }
            GiftManager.Instance.DisableGiftGeneration();
        }
    }

}
