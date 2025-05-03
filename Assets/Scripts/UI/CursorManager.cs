using UnityEngine;
using UnityEngine.EventSystems;
using MoreMountains.Feedbacks;

namespace BoomJam2025
{
    /// <summary>
    /// 光标管理器，用于管理游戏中的自定义光标
    /// </summary>
    public class CursorManager : MonoBehaviour
    {
        #region Singleton
        public static CursorManager Instance { get; private set; }

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
        #endregion

        [Header("光标设置")]
        [Tooltip("默认光标")]
        public Texture2D defaultCursor;
        
        [Tooltip("点击时的光标")]
        public Texture2D clickCursor;
        
        [Tooltip("光标识别点，通常为(0,0)，即左上角")]
        public Vector2 hotspot = Vector2.zero;
        
        [Tooltip("光标渲染模式，Auto为自动，ForceSoftware为强制软件渲染")]
        public CursorMode cursorMode = CursorMode.Auto;

        [Header("音效设置")]
        [Tooltip("点击音效")]
        public AudioClip clickSound;

        [Tooltip("音效音量")]
        [Range(0f, 1f)]
        public float clickSoundVolume = 1f;
        
        [Header("MMFeedbacks设置")]
        [Tooltip("点击音效Feedback播放器")]
        public MMF_Player clickSoundPlayer;

        private bool isClicking = false;

        private void Start()
        {
            // 设置默认光标
            SetDefaultCursor();
        }

        private void Update()
        {
            // 检测鼠标点击
            if (Input.GetMouseButtonDown(0))
            {
                // 设置点击光标
                SetClickCursor();
                // 播放点击音效
                PlayClickSound();
                isClicking = true;
            }
            
            if (Input.GetMouseButtonUp(0) && isClicking)
            {
                // 恢复默认光标
                SetDefaultCursor();
                isClicking = false;
            }
        }

        /// <summary>
        /// 设置默认光标
        /// </summary>
        public void SetDefaultCursor()
        {
            if (defaultCursor != null)
            {
                Cursor.SetCursor(defaultCursor, hotspot, cursorMode);
            }
        }

        /// <summary>
        /// 设置点击光标
        /// </summary>
        public void SetClickCursor()
        {
            if (clickCursor != null)
            {
                Cursor.SetCursor(clickCursor, hotspot, cursorMode);
            }
        }



        /// <summary>
        /// 播放点击音效
        /// </summary>
        private void PlayClickSound()
        {
            if (clickSoundPlayer != null)
            {
                clickSoundPlayer.PlayFeedbacks();
            }
            else if (clickSound != null && Camera.main != null)
            {
                // 如果没有配置MMF_Player，仍然使用旧方法作为备选
                AudioSource.PlayClipAtPoint(clickSound, Camera.main.transform.position, clickSoundVolume);
            }
        }
    }
}
