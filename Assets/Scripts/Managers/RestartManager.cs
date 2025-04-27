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

        // Update is called once per frame
        void Update()
        {
            if (!isGamePaused && !isTimeEnd)
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
            else
            {
                StartCoroutine(PlayNormalEnding());
            }
        }

        /// <summary>
        /// 播放普通结局
        /// </summary>
        private IEnumerator PlayNormalEnding()
        {
            // TODO: 在这里添加普通结局的演出逻辑
            
            // 等待当前对话完成（如果有的话）
            while (DialogueManager.Instance.IsDialogueRunning())
            {
                yield return null;
            }
            
            // 开始对话并等待对话完成
            bool dialogueCompleted = false;
            Action<string> onDialogueComplete = null;
            onDialogueComplete = (nodeName) => {
                if (nodeName == "NormalEnd") {
                    dialogueCompleted = true;
                    DialogueManager.Instance.OnDialogueNodeComplete -= onDialogueComplete;
                }
            };
            
            DialogueManager.Instance.OnDialogueNodeComplete += onDialogueComplete;
            DialogueManager.Instance.StartDialogue("NormalEnd");
            
            // 等待对话完成
            while (!dialogueCompleted) {
                yield return null;
            }

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
        /// 播放大结局
        /// </summary>
        private IEnumerator PlayGrandFinale()
        {
            // TODO: 在这里添加大结局的演出逻辑
            yield return new WaitForSeconds(10f); // 临时占位，等待实际演出逻辑
            
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
            if (inputBlocker != null)
            {
                inputBlocker.SetActive(true);
            }
            GiftManager.Instance.DisableGiftGeneration();
        }

        public void OnAdvanceCancelButtonClicked()
        {
            ResumeGame();
        }

        public void OnAdvanceRestartButtonClicked()
        {
            PauseGame();
            if (restartPanel != null)
            {
                restartPanel.SetActive(true);
            }
            if (inputBlocker != null)
            {
                inputBlocker.SetActive(true);
            }
        }
    }

}
