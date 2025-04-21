/****************************************************************************
 * Author: 周欣悦
 * Date: 2025-04-21
 * Description: 评论池
 ****************************************************************************/

namespace BoomJam2025
{
    using UnityEngine;
    using System.Collections.Generic;

    public class CommentPool
    {
        private GameObject prefab;
        private Transform container;
        private Queue<GameObject> recyclePool;  // 回收池
        private List<GameObject> activePool;    // 显示池
        private int maxActiveSize;              // 最大显示数量

        public CommentPool(GameObject commentPrefab, Transform commentContainer, int maxPoolSize)
        {
            prefab = commentPrefab;
            container = commentContainer;
            maxActiveSize = maxPoolSize;
            recyclePool = new Queue<GameObject>();
            activePool = new List<GameObject>();
        }

        /// <summary>
        /// 获取一个评论对象，优先从回收池中获取
        /// </summary>
        public GameObject GetComment()
        {
            GameObject commentObj;
            
            // 优先从回收池中获取
            if (recyclePool.Count > 0)
            {
                commentObj = recyclePool.Dequeue();
                commentObj.SetActive(true);
            }
            else
            {
                // 回收池为空时创建新对象
                commentObj = GameObject.Instantiate(prefab, container);
            }

            // 添加到显示池
            activePool.Add(commentObj);

            // 如果显示池超过最大限制，回收最早的评论
            if (activePool.Count > maxActiveSize)
            {
                RecycleOldestComment();
            }

            // 确保新获取的评论显示在最下方
            commentObj.transform.SetAsLastSibling();

            return commentObj;
        }

        /// <summary>
        /// 回收指定的评论对象
        /// </summary>
        public void RecycleComment(GameObject comment)
        {
            if (activePool.Contains(comment))
            {
                activePool.Remove(comment);
                comment.SetActive(false);
                recyclePool.Enqueue(comment);
            }
        }

        /// <summary>
        /// 回收最早的评论（显示池中的第一个）
        /// </summary>
        private void RecycleOldestComment()
        {
            if (activePool.Count > 0)
            {
                GameObject oldestComment = activePool[0];
                RecycleComment(oldestComment);
            }
        }

        /// <summary>
        /// 获取当前显示池中的评论数量
        /// </summary>
        public int ActiveCount => activePool.Count;

        /// <summary>
        /// 获取当前回收池中的评论数量
        /// </summary>
        public int RecycleCount => recyclePool.Count;

        /// <summary>
        /// 清空所有评论
        /// </summary>
        public void Clear()
        {
            // 将所有显示中的评论移到回收池
            while (activePool.Count > 0)
            {
                RecycleOldestComment();
            }
        }
    }
}
