/****************************************************************************
 * Author: 周欣悦
 * Date: 2025-04-21
 * Description: 评论管理器
 ****************************************************************************/
namespace BoomJam2025
{
    using UnityEngine;
    using System.Collections.Generic;
    using System.IO;
    using UnityEngine.UI;
    using System.Linq;

    public class CommentManager : MonoBehaviour
    {
        public static CommentManager Instance { get; private set; }
        
        /// <summary>
        /// 评论预制体
        /// </summary>
        [SerializeField] private GameObject commentPrefab;
        /// <summary>
        /// 评论容器
        /// </summary>
        [SerializeField] private Transform commentContainer;
        /// <summary>
        /// 最大评论数量
        /// </summary>
        [SerializeField] private int maxComments = 50;
        
        [SerializeField] private ScrollRect scrollRect;  // 添加ScrollRect引用
        
        private List<string> userNames = new List<string>();
        private List<string> comments = new List<string>();
        private CommentPool commentPool;
        private bool isInitialized = false;
        private bool _isEnabled = false;
        
        public enum SpawnSpeed
        {
            VeryFast,    // 2秒以内
            Fast,        // 6秒以内
            QuickPaced,  // 10秒以内
            Medium,      // 15秒以内
            Slow         // 30秒以内
        }
        
        [SerializeField] private SpawnSpeed currentSpeed = SpawnSpeed.Medium;
        private float nextSpawnTime;
        
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
                return;
            }

            // 检查必要组件
            if (commentPrefab == null)
            {
                Debug.LogError("CommentManager: 未设置评论预制体！");
                enabled = false;
                return;
            }

            if (commentContainer == null)
            {
                Debug.LogError("CommentManager: 未设置评论容器！");
                enabled = false;
                return;
            }
        }
        
        private void Start()
        {
            if (!enabled) return;
            
            InitializeData();
            
            // 检查数据是否加载成功
            if (userNames.Count == 0 || comments.Count == 0)
            {
                Debug.LogError("CommentManager: CSV数据加载失败！");
                enabled = false;
                return;
            }

            commentPool = new CommentPool(commentPrefab, commentContainer, maxComments);
            
            // 设置ScrollRect
            if (scrollRect == null)
            {
                Debug.LogError("CommentManager: 未设置ScrollRect！");
                enabled = false;
                return;
            }
            commentPool.SetScrollRect(scrollRect);
            
            isInitialized = true;
            SetNextSpawnTime();
        }
        
        private void Update()
        {
            if (!isInitialized || !_isEnabled) return;
            
            if (Time.time >= nextSpawnTime)
            {
                SpawnComment();
                SetNextSpawnTime();
            }
        }
        
        private void InitializeData()
        {
            // 读取用户名CSV
            TextAsset userNamesCsv = Resources.Load<TextAsset>("CSVData/usernames");
            if (userNamesCsv != null)
            {
                userNames = userNamesCsv.text.Split('\n')
                    .Where(x => !string.IsNullOrEmpty(x.Trim()))
                    .Select(x => x.Trim())
                    .ToList();
            }
            else
            {
                Debug.LogError("CommentManager: 无法加载usernames.csv文件！");
            }
            
            // 读取评论内容CSV
            TextAsset commentsCsv = Resources.Load<TextAsset>("CSVData/comments");
            if (commentsCsv != null)
            {
                comments = commentsCsv.text.Split('\n')
                    .Where(x => !string.IsNullOrEmpty(x.Trim()))
                    .Select(x => x.Trim())
                    .ToList();
            }
            else
            {
                Debug.LogError("CommentManager: 无法加载comments.csv文件！");
            }
        }
        
        private void SpawnComment()
        {
            if (!isInitialized || userNames.Count == 0 || comments.Count == 0) return;
            
            string randomUserName = userNames[Random.Range(0, userNames.Count)];
            string randomComment = comments[Random.Range(0, comments.Count)];
            
            GameObject commentObj = commentPool.GetComment();
            if (commentObj != null)
            {
                CommentBubble commentBubble = commentObj.GetComponent<CommentBubble>();
                if (commentBubble != null)
                {
                    commentBubble.Initialize(randomUserName, randomComment);
                }
                else
                {
                    Debug.LogError("CommentManager: 评论预制体缺少CommentBubble组件！");
                }
            }
        }
        
        private void SetNextSpawnTime()
        {
            float maxDelay = currentSpeed switch
            {
                SpawnSpeed.VeryFast => 2f,
                SpawnSpeed.Fast => 6f,
                SpawnSpeed.QuickPaced => 10f,
                SpawnSpeed.Medium => 15f,
                SpawnSpeed.Slow => 30f,
                _ => 15f
            };
            
            nextSpawnTime = Time.time + Random.Range(0.5f, maxDelay);
        }
        
        public void SetSpawnSpeed(SpawnSpeed speed)
        {
            currentSpeed = speed;
        }

        /// <summary>
        /// 清空所有评论
        /// </summary>
        public void ClearComments()
        {
            commentPool.Clear();
        }

        /// <summary>
        /// 开始运行
        /// </summary>
        public void StartRunning()
        {
            _isEnabled = true;
            ClearComments();
            SetNextSpawnTime();
        }

        /// <summary>
        /// 停止运行
        /// </summary>
        public void StopRunning()
        {
            _isEnabled = false;
            ClearComments();
        }
    }
}
