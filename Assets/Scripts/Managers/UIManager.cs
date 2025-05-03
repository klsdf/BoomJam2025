/****************************************************************************
 * Author: 周欣悦
 * Date: 2025-04-16
 * Description: UI管理器，负责管理所有UI面板的显示和隐藏
 ****************************************************************************/

namespace BoomJam2025
{
    using UnityEngine;
    using UnityEngine.UI;
    using MoreMountains.Feedbacks;

    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance;

        [Header("UI Panels")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject liveRoomPanel;
        [SerializeField] private GameObject timeEndPanel;
        [SerializeField] private GameObject restartPanel;
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject inputBlocker;
        [SerializeField] private MMF_Player restartHintText;

        private GameObject currentActivePanel;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // 初始化时隐藏所有面板
            HideAllPanels();
            
            // 显示主菜单
            ShowPanel(mainMenuPanel);
            
            // 注册场景切换事件
            GameManager.Instance.onGameStateChanged.AddListener(OnGameStateChanged);
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.onGameStateChanged.RemoveListener(OnGameStateChanged);
            }
        }

        private void OnGameStateChanged(GameState newState)
        {
            // 根据新的游戏状态切换UI面板
            switch (newState)
            {
                case GameState.MainMenu:
                    ShowPanel(mainMenuPanel);
                    break;
                case GameState.LiveRoom:
                    ShowPanel(liveRoomPanel);
                    break;
            }
        }

        private void ShowPanel(GameObject panel)
        {
            if (panel == null) return;

            // 如果当前有活动的面板，先隐藏它
            if (currentActivePanel != null)
            {
                HidePanel(currentActivePanel);
            }

            // 显示新面板
            panel.SetActive(true);
            currentActivePanel = panel;

            // 获取并初始化所有动画组件
            var animations = panel.GetComponentsInChildren<SequenceAnimation>(true);
            foreach (var anim in animations)
            {
                // 确保动画组件被启用
                anim.enabled = true;
                // 重新播放动画
                anim.Play();
            }
        }

        private void HidePanel(GameObject panel)
        {
            if (panel == null) return;

            // 停止所有动画
            var animations = panel.GetComponentsInChildren<SequenceAnimation>(true);
            foreach (var anim in animations)
            {
                anim.Stop();
            }

            panel.SetActive(false);
        }

        private void HideAllPanels()
        {
            if (mainMenuPanel != null) HidePanel(mainMenuPanel);
            if (liveRoomPanel != null) HidePanel(liveRoomPanel);
            if (timeEndPanel != null) HidePanel(timeEndPanel);
            if (restartPanel != null) HidePanel(restartPanel);
            if (pausePanel != null) HidePanel(pausePanel);
            if (inputBlocker != null) HidePanel(inputBlocker);
        }

        public void ShowTimeEndPanel()
        {
            if (timeEndPanel != null)
            {
                timeEndPanel.SetActive(true);
            }
            if (inputBlocker != null)
            {
                inputBlocker.SetActive(true);
            }
            RestartManager.Instance.PauseGame();
        }

        public void ShowRestartPanel()
        {
            // 检查是否可以重启
            if (RebirthManager.Instance.countRebirth >= 2)
            {
                if (restartPanel != null)
                {
                    restartPanel.SetActive(true);
                }
                if (inputBlocker != null)
                {
                    inputBlocker.SetActive(true);
                }
                RestartManager.Instance.PauseGame();
            }
            else
            {
                restartHintText.PlayFeedbacks();
            }

        }

        public void ShowPausePanel()
        {
            if (pausePanel != null)
            {
                pausePanel.SetActive(true);
            }
            if (inputBlocker != null)
            {
                inputBlocker.SetActive(true);
            }
            RestartManager.Instance.PauseGame();
        }

        public void HideTimeEndPanel()
        {
            if (timeEndPanel != null)
            {
                timeEndPanel.SetActive(false);
            }
            if (inputBlocker != null)
            {
                inputBlocker.SetActive(false);
            }
        }

        public void HideRestartPanel()
        {
            if (restartPanel != null)
            {
                restartPanel.SetActive(false);
            }
            if (inputBlocker != null)
            {
                inputBlocker.SetActive(false);
            }
        }

        public void HidePausePanel()
        {
            if (pausePanel != null)
            {
                pausePanel.SetActive(false);
            }
            if (inputBlocker != null)
            {
                inputBlocker.SetActive(false);
            }
        }
        public void ShowInputBlocker()
        {
            if (inputBlocker != null)
            {
                inputBlocker.SetActive(true);
            }
        }

        public void HideInputBlocker()
        {
            if (inputBlocker != null)
            {
                inputBlocker.SetActive(false);
            }
        }
    }
} 