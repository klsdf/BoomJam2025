/****************************************************************************
 * Author: 周欣悦
 * Date: 2025-04-16
 * Description: 游戏管理器，负责管理游戏的场景跳转和游戏进程
 ****************************************************************************/

namespace BoomJam2025
{
    using UnityEngine;
    using UnityEngine.Events;
    using System.Collections;

    /// <summary>
    /// 游戏状态枚举
    /// </summary>
    public enum GameState
    {
        MainMenu,       // 主菜单
        LiveRoom,       // 直播间
    }

    /// <summary>
    /// 游戏管理器类，使用单例模式管理游戏的核心系统
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        /// <summary>
        /// 单例实例
        /// </summary>
        public static GameManager Instance;

        /// <summary>
        /// 当前游戏状态
        /// </summary>
        private GameState currentState;

        /// <summary>
        /// 游戏状态改变事件
        /// </summary>
        public UnityEvent<GameState> onGameStateChanged = new UnityEvent<GameState>();

        /// <summary>
        /// 当前游戏状态属性
        /// </summary>
        public GameState CurrentState
        {
            get => currentState;
            set
            {
                if (currentState != value)
                {
                    currentState = value;
                    onGameStateChanged.Invoke(currentState);
                    
                    // 根据状态启动或停止管理器
                    if (currentState == GameState.MainMenu)
                    {
                        StopAllManagers();
                    }
                    else if (currentState == GameState.LiveRoom)
                    {
                        StartAllManagers();
                    }
                }
            }
        }

        /// <summary>
        /// Awake 初始化单例
        /// </summary>
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

        /// <summary>
        /// Start 加载游戏数据并初始化游戏状态
        /// </summary>
        private void Start()
        {
            // 设置初始状态为主菜单
            CurrentState = GameState.MainMenu;
            
            // 延迟一帧重置所有管理器状态，确保所有管理器都已经初始化完成
            StartCoroutine(DelayedReset());
        }

        private IEnumerator DelayedReset()
        {
            yield return null; // 等待一帧
            ResetAllManagers();
        }

        /// <summary>
        /// 重置所有管理器状态
        /// </summary>
        private void ResetAllManagers()
        {
            // 重置贡献值
            CoreValueManager.Instance.ResetAllValues();
            
            // 重置其他管理器状态
            GiftManager.Instance.ClearAllGifts();
            CommentManager.Instance.ClearComments();
        }

        /// <summary>
        /// 切换游戏状态
        /// </summary>
        /// <param name="newState">新的游戏状态</param>
        public void ChangeGameState(GameState newState)
        {
            CurrentState = newState;
        }

        public void StartGame()
        {
            Debug.Log("游戏场景加载完成，切换游戏状态到LiveRoom");
            // 切换游戏状态
            ChangeGameState(GameState.LiveRoom);
        }

        /// <summary>
        /// Update 更新核心数值
        /// </summary>
        private void Update()
        {
            
        }

        /// <summary>
        /// OnApplicationQuit 退出时保存游戏
        /// </summary>
        private void OnApplicationQuit()
        {
            SaveManager.Instance.SaveGame();
        }

        /// <summary>
        /// 启动所有管理器
        /// </summary>
        private void StartAllManagers()
        {
            GiftManager.Instance.StartRunning();
            RestartManager.Instance.StartRunning();
            CommentManager.Instance.StartRunning();
            StreamerStateManager.Instance.StartRunning();
        }

        /// <summary>
        /// 停止所有管理器
        /// </summary>
        private void StopAllManagers()
        {
            RestartManager.Instance.StopRunning();
            GiftManager.Instance.StopRunning();
            CommentManager.Instance.StopRunning();
            StreamerStateManager.Instance.StopRunning();
        }
    }
}
