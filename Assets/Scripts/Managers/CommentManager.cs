/****************************************************************************
 * Author: 张嘉阳
 * Date: 2025-04-19
 * Description: 评论管理器（基于 ScrollRect + 平滑滚动）
 ****************************************************************************/
namespace BoomJam2025
{
    using UnityEngine;
    using UnityEngine.UI;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// 单例：自动生成评论、维护队列、滚动到底部
    /// </summary>
    public class CommentManager : MonoBehaviour
    {
        public static CommentManager Instance { get; private set; }

        [Header("UI 引用")]
        [Tooltip("挂在 Scroll View 上的 ScrollRect")]
        public ScrollRect scrollRect;
        [Tooltip("ScrollRect.Viewport 下的 Content（需 VerticalLayoutGroup + ContentSizeFitter）")]
        public RectTransform content;
        [Tooltip("CommentItem 预制体，需含 CommentItem 脚本 + TextMeshProUGUI")]
        public GameObject commentPrefab;

        [Header("配置")]
        [Tooltip("最大保留评论数，超出则回收最旧")]
        public int maxComments = 20;
        [Tooltip("滚动到底部的动画时长 (秒)")]
        public float scrollAnimDuration = 0.2f;

        private readonly Queue<CommentItem> activeQueue = new();
        private readonly List<CommentItem> pool = new();
        private Coroutine scrollCoroutine;

        private void Awake()
        {
            // 单例初始化
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // 引用校验
            if (scrollRect == null) scrollRect = GetComponent<ScrollRect>();
            if (content == null ||
                commentPrefab == null)
                Debug.LogError("请在 Inspector 设置 ScrollRect、Content、CommentPrefab！");
        }

        /// <summary>
        /// 对外接口：添加一条评论，并平滑滚动到底部
        /// </summary>
        public void AddComment(string text)
        {
            // 1. 取或新建一个 CommentItem
            var item = GetFromPool();
            item.gameObject.SetActive(true);
            item.transform.SetParent(content, false);
            item.Initialize(text);
            activeQueue.Enqueue(item);

            // 2. 超出 maxComments 回收最旧
            if (activeQueue.Count > maxComments)
            {
                var old = activeQueue.Dequeue();
                old.transform.SetParent(transform, false);
                old.gameObject.SetActive(false);
            }

            // 3. 强制刷新布局后平滑滚动到底部
            Canvas.ForceUpdateCanvases();
            if (scrollCoroutine != null) StopCoroutine(scrollCoroutine);
            scrollCoroutine = StartCoroutine(SmoothScrollToBottom());
        }

        private IEnumerator SmoothScrollToBottom()
        {
            float elapsed = 0f;
            float start = scrollRect.verticalNormalizedPosition;
            while (elapsed < scrollAnimDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / scrollAnimDuration);
                scrollRect.verticalNormalizedPosition = Mathf.Lerp(start, 0f, t);
                yield return null;
            }
            scrollRect.verticalNormalizedPosition = 0f;
        }

        /// <summary>
        /// 从对象池取可用 CommentItem，或新建
        /// </summary>
        private CommentItem GetFromPool()
        {
            foreach (var it in pool)
            {
                if (!it.gameObject.activeSelf)
                    return it;
            }
            var go = Instantiate(commentPrefab);
            var ci = go.GetComponent<CommentItem>();
            if (ci == null)
                Debug.LogError("commentPrefab 缺少 CommentItem 脚本！");
            pool.Add(ci);
            return ci;
        }
    }
}
