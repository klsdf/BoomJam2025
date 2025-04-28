/****************************************************************************
 * Author: 周欣悦
 * Date: 2025-04-16
 * Description: UI管理器，负责管理所有UI面板的显示和隐藏
 ****************************************************************************/

namespace BoomJam2025
{
    using UnityEngine;
    using UnityEngine.UI;

    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance;

        [Header("UI Panels")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject liveRoomPanel;

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
                    StopAllManagers();
                    break;
                case GameState.LiveRoom:
                    ShowPanel(liveRoomPanel);
                    StartAllManagers();
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
        }

        /// <summary>
        /// 启动所有管理器
        /// </summary>
        private void StartAllManagers()
        {
            TimerManager.Instance.StartRunning();
            GiftManager.Instance.StartRunning();
            CommentManager.Instance.StartRunning();
        }

        /// <summary>
        /// 停止所有管理器
        /// </summary>
        private void StopAllManagers()
        {
            TimerManager.Instance.StopRunning();
            GiftManager.Instance.StopRunning();
            CommentManager.Instance.StopRunning();
        }
    }
} 