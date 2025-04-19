/****************************************************************************
 * Author: 周欣悦
 * Date: 2025-04-16
 * Description: 礼物对象池管理器
 ****************************************************************************/

namespace BoomJam2025
{
    using UnityEngine;
    using System.Collections.Generic;

    /// <summary>
    /// 礼物对象池管理器
    /// </summary>
    /// <remarks>
    /// 该类实现了对象池模式，用于管理礼物对象的创建、获取和回收。
    /// 通过重用对象来减少内存分配和垃圾回收，提高性能。
    /// </remarks>
    public class GiftPool : MonoBehaviour
    {
        [Header("Pool Settings")]
        /// <summary>
        /// 普通礼物预制体
        /// </summary>
        public GameObject giftItemPrefab;
        
        /// <summary>
        /// 初始池大小
        /// </summary>
        public int initialPoolSize = 20;

        private Queue<IGiftItem> pool = new Queue<IGiftItem>();
        private Transform poolContainer;
        private bool isInitialized = false;

        /// <summary>
        /// 初始化对象池容器
        /// </summary>
        /// <remarks>
        /// 创建并设置对象池容器的Transform。
        /// </remarks>
        private void Awake()
        {
            poolContainer = new GameObject("GiftPoolContainer").transform;
            poolContainer.SetParent(transform);
        }

        /// <summary>
        /// 初始化对象池
        /// </summary>
        /// <remarks>
        /// 检查预制体引用并初始化对象池。
        /// </remarks>
        private void Start()
        {
            if (giftItemPrefab == null)
            {
                Debug.LogError("GiftItem prefab is not assigned in GiftPool!");
                return;
            }
            InitializePool();
        }

        /// <summary>
        /// 初始化对象池
        /// </summary>
        /// <remarks>
        /// 创建初始数量的礼物对象并加入池中。
        /// </remarks>
        private void InitializePool()
        {
            if (isInitialized) return;
            
            if (giftItemPrefab == null)
            {
                Debug.LogError("Cannot initialize pool: GiftItem prefab is null!");
                return;
            }

            for (int i = 0; i < initialPoolSize; i++)
            {
                CreateNewGiftItem();
            }
            
            isInitialized = true;
        }

        /// <summary>
        /// 创建新的礼物对象
        /// </summary>
        /// <remarks>
        /// 实例化预制体并添加到对象池中。
        /// </remarks>
        private void CreateNewGiftItem()
        {
            if (giftItemPrefab == null)
            {
                Debug.LogError("Cannot create gift item: prefab is null!");
                return;
            }

            GameObject giftObj = Instantiate(giftItemPrefab, poolContainer);
            IGiftItem giftItem = giftObj.GetComponent<IGiftItem>();
            if (giftItem == null)
            {
                Debug.LogError("IGiftItem component not found on prefab!");
                Destroy(giftObj);
                return;
            }
            
            giftObj.SetActive(false);
            pool.Enqueue(giftItem);
        }

        /// <summary>
        /// 从对象池获取礼物对象
        /// </summary>
        /// <returns>可用的礼物对象</returns>
        /// <remarks>
        /// 如果池中没有可用对象，会创建新的对象。
        /// </remarks>
        public IGiftItem GetGiftItem()
        {
            if (!isInitialized)
            {
                InitializePool();
            }

            if (pool.Count == 0)
            {
                CreateNewGiftItem();
            }

            IGiftItem giftItem = pool.Dequeue();
            giftItem.gameObject.SetActive(true);
            return giftItem;
        }

        /// <summary>
        /// 将礼物对象返回到对象池
        /// </summary>
        /// <param name="giftItem">要回收的礼物对象</param>
        /// <remarks>
        /// 重置对象状态并将其放回对象池以供重用。
        /// </remarks>
        public void ReturnGiftItem(IGiftItem giftItem)
        {
            if (giftItem == null) return;
            
            giftItem.gameObject.SetActive(false);
            giftItem.gameObject.transform.SetParent(poolContainer);
            pool.Enqueue(giftItem);
        }
    }
} 