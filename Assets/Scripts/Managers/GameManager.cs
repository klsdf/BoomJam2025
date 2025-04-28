/****************************************************************************
 * Author: 周欣悦
 * Date: 2025-04-16
 * Description: 游戏管理器，负责管理游戏的场景跳转和游戏进程
 ****************************************************************************/

namespace BoomJam2025
{
    using UnityEngine;
    using UnityEngine.Events;

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
            // 重置所有管理器状态
            ResetAllManagers();
            
            // 加载游戏数据
            SaveManager.Instance.LoadGame();
            
            // 设置初始状态为主菜单
            CurrentState = GameState.MainMenu;
        }

        /// <summary>
        /// 重置所有管理器状态
        /// </summary>
        private void ResetAllManagers()
        {
            // 重置贡献值
            CoreValueManager.Instance.ResetAllValues();
            
            // 重置其他管理器状态
            TimerManager.Instance.ResetTimer();
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
            CurrentState = GameState.LiveRoom;
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
    }
}
