/****************************************************************************
 * Author: 张嘉阳
 * Date: 2025-04-19
 * Description: 评论管理器，基于 ScrollRect + 布局组件
 ****************************************************************************/
namespace BoomJam2025
{
    using UnityEngine;
    using UnityEngine.UI;
    using System.Collections.Generic;

    /// <summary>
    /// 负责生成评论、自动排版并滚动到最新
    /// </summary>
    /// 


    public class CommentManager : MonoBehaviour
    {
        public static CommentManager Instance { get; private set; }

        [Header("UI 引用")]
        public ScrollRect scrollRect;
        public RectTransform content;
        public GameObject commentPrefab;

        [Header("配置")]
        public int maxComments = 10;

        private readonly Queue<CommentItem> activeQueue = new();
        private readonly List<CommentItem> pool = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (scrollRect == null || content == null || commentPrefab == null)
                Debug.LogError("请在 Inspector 里正确设置 ScrollRect、Content、CommentPrefab！");
        }

        /// <summary>
        /// 对外调用：插入一条新评论并滚动到最底部
        /// </summary>
        public void AddComment(string text)
        {
            // 1. 获取或新建
            CommentItem item = GetFromPool();
            item.transform.SetParent(content, false);
            item.Initialize(text);
            activeQueue.Enqueue(item);

            // 2. 超出时回收最旧
            if (activeQueue.Count > maxComments)
            {
                var old = activeQueue.Dequeue();
                Recycle(old);
            }

            // 3. 强制刷新布局后滚动到底部
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
        }

        private CommentItem GetFromPool()
        {
            foreach (var it in pool)
                if (!it.gameObject.activeSelf)
                    return it;

            var go = Instantiate(commentPrefab);
            var ci = go.GetComponent<CommentItem>();
            if (ci == null) Debug.LogError("CommentPrefab 缺少 CommentItem 脚本！");
            pool.Add(ci);
            return ci;
        }

        private void Recycle(CommentItem item)
        {
            item.gameObject.SetActive(false);
        }
    }

}
