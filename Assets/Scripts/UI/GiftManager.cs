/****************************************************************************
 * Author: 周欣悦
 * Date: 2025-04-16
 * Description: 管理礼物掉落UI效果的管理器
 ****************************************************************************/

namespace BoomJam2025
{
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;
    using System.Collections.Generic;
    using UnityEngine.EventSystems;

    /// <summary>
    /// 管理礼物掉落效果和贡献值显示的UI管理器
    /// </summary>
    /// <remarks>
    /// 该类负责管理整个礼物系统的运行，包括：
    /// 1. 监听用户点击事件
    /// 2. 生成礼物对象
    /// 3. 管理礼物对象池
    /// 4. 处理礼物位置和动画
    /// </remarks>
    public class GiftManager : MonoBehaviour
    {
        private static GiftManager _instance;
        public static GiftManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<GiftManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("GiftManager");
                        _instance = go.AddComponent<GiftManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        [Header("References")]
        /// <summary>
        /// 礼物预制体
        /// </summary>
        public GameObject giftItemPrefab;
        
        /// <summary>
        /// 礼物容器Transform（屏幕Canvas）
        /// </summary>
        public Transform giftContainer;
        
        /// <summary>
        /// 主摄像机引用
        /// </summary>
        public Camera mainCamera;

        /// <summary>
        /// 礼物数据配置
        /// </summary>
        public GiftData giftData;

        /// <summary>
        /// 总贡献值显示
        /// </summary>
        public TextMeshProUGUI textTotalContribution;

        [Header("Settings")]
        /// <summary>
        /// 礼物生成的最小X坐标
        /// </summary>
        public float minX = 0f;
        
        /// <summary>
        /// 礼物生成的最大X坐标
        /// </summary>
        public float maxX = 1920f;
        
        private float screenHeight;
        private GiftPool giftPool;
        
        private bool isGiftGenerationEnabled = true;
        
        /// <summary>
        /// 初始化组件和对象池
        /// </summary>
        /// <remarks>
        /// 在Awake中初始化GiftPool组件，并确保预制体引用正确设置。
        /// </remarks>
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            // 初始化对象池
            giftPool = GetComponent<GiftPool>();
            if (giftPool == null)
            {
                giftPool = gameObject.AddComponent<GiftPool>();
            }
            
            // 确保预制体引用已设置
            if (giftPool.giftItemPrefab == null)
            {
                giftPool.giftItemPrefab = giftItemPrefab;
            }

            // 初始化总贡献值显示
            UpdateTotalContribution();
        }
        
        /// <summary>
        /// 初始化屏幕高度和摄像机引用
        /// </summary>
        private void Start()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }
            
            screenHeight = Screen.height;
        }
        
        /// <summary>
        /// 监听用户点击事件
        /// </summary>
        /// <remarks>
        /// 检查用户是否点击了非UI区域，如果是则生成礼物。
        /// </remarks>
        private void Update()
        {
            if (!isGiftGenerationEnabled) return;

            if (Input.GetMouseButtonDown(0))
            {
                // 检查是否点击了UI按钮
                if (!IsPointerOverUIObject())
                {
                    SpawnGift();
                }
            }

            // 更新总贡献值显示
            UpdateTotalContribution();
        }
        
        /// <summary>
        /// 生成新的礼物
        /// </summary>
        /// <remarks>
        /// 从对象池获取礼物对象，设置随机位置并初始化。
        /// </remarks>
        private void SpawnGift()
        {
            if (giftPool == null || giftContainer == null)
            {
                Debug.LogError("GiftPool or container is not assigned!");
                return;
            }
            
            // 从对象池获取礼物
            GiftItem giftItem = giftPool.GetGiftItem();
            giftItem.transform.SetParent(giftContainer);
            
            // 设置随机X位置
            float randomX = Random.Range(minX, maxX);
            giftItem.GetComponent<RectTransform>().anchoredPosition = new Vector2(randomX, 0);
            
            // 初始化礼物
            giftItem.Initialize(screenHeight);
            
            // 设置贡献值
            double contributionValue = CoreValueManager.Instance.ClickGiftValue();
            giftItem.SetObtainedContributionValue(CoreValueManager.Instance.FormatValue(contributionValue));

            // 设置礼物图标
            if (giftData != null)
            {
                Sprite giftSprite = giftData.GetRandomGiftSprite(contributionValue);
                if (giftSprite != null)
                {
                    giftItem.SetGiftIcon(giftSprite);
                }
            }
        }
        
        /// <summary>
        /// 检查鼠标是否点击了Button组件
        /// </summary>
        /// <returns>如果鼠标点击了Button组件返回true，否则返回false</returns>
        private bool IsPointerOverUIObject()
        {
            // 检查是否点击了UI元素
            if (EventSystem.current.IsPointerOverGameObject())
            {
                // 获取当前指针下的所有UI元素
                var pointerData = new PointerEventData(EventSystem.current)
                {
                    position = Input.mousePosition
                };
                var results = new List<RaycastResult>();
                EventSystem.current.RaycastAll(pointerData, results);

                // 检查是否有Button组件
                foreach (var result in results)
                {
                    if (result.gameObject.GetComponent<Button>() != null)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 更新总贡献值显示
        /// </summary>
        private void UpdateTotalContribution()
        {
            textTotalContribution.text = CoreValueManager.Instance.FormatValue(CoreValueManager.Instance.valueContribution);
        }

        /// <summary>
        /// 启用礼物生成
        /// </summary>
        public void EnableGiftGeneration()
        {
            isGiftGenerationEnabled = true;
        }

        /// <summary>
        /// 禁用礼物生成
        /// </summary>
        public void DisableGiftGeneration()
        {
            isGiftGenerationEnabled = false;
        }
    }
} 