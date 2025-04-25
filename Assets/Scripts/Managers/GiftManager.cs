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
    using System.Collections;
    using Febucci.UI;

    /// <summary>
    /// 管理礼物掉落效果和贡献值显示的UI管理器
    /// </summary>
    /// <remarks>
    /// 该类负责管理整个礼物系统的运行，包括：
    /// 1. 监听用户点击事件
    /// 2. 生成礼物对象
    /// 3. 管理礼物对象池
    /// 4. 处理礼物位置和动画
    /// 5. 更新总贡献值显示及其动画
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
        /// 普通礼物预制体
        /// </summary>
        public GameObject normalGiftPrefab;
        
        /// <summary>
        /// 特殊礼物预制体
        /// </summary>
        public GameObject specialGiftPrefab;
        
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
        /// 总贡献值显示 因动画插件弃用
        /// </summary>
        //public TextMeshProUGUI textTotalContribution;

        /// <summary>
        /// 动画组件
        /// </summary>
        public TextAnimator_TMP textAnimator;

        [Header("Settings")]
        /// <summary>
        /// 礼物生成的最小X坐标
        /// </summary>
        public float minX = 0f;
        
        /// <summary>
        /// 礼物生成的最大X坐标
        /// </summary>
        public float maxX = 1920f;

        [Header("Long Press Settings")]
        /// <summary>
        /// 长按送礼的频率（秒）
        /// </summary>
        public float giftSpawnInterval = 0.1f;
        
        [Header("Special Gift Settings")]
        /// <summary>
        /// 特殊礼物生成概率（0-1）
        /// </summary>
        public float specialGiftProbability = 0.1f;
        
        /// <summary>
        /// 特殊礼物冷却时间（秒）
        /// </summary>
        public float specialGiftCooldown = 30f;
        
        private float screenHeight;
        private GiftPool giftPool;
        private bool isGiftGenerationEnabled = true;
        private float lastSpecialGiftTime;
        private bool isSpecialGiftAvailable = true;
        private float lastGiftSpawnTime;
        private bool isLongPressing;
        private bool isSpacePressing;  // 新增：空格键长按状态
     
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
                giftPool.giftItemPrefab = normalGiftPrefab;
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
            lastSpecialGiftTime = Time.time;

            RemoveRainbowEffect(); //初始化总贡献值
        }
        
        /// <summary>
        /// 监听用户点击事件
        /// </summary>
        /// <remarks>
        /// 检查用户是否点击了非UI区域，如果是则生成礼物。
        /// 支持长按连续送礼功能。
        /// </remarks>
        private void Update()
        {
            if (!isGiftGenerationEnabled) return;

            // 检测鼠标按下
            if (Input.GetMouseButtonDown(0))
            {
                isLongPressing = true;
                lastGiftSpawnTime = Time.time;
                SpawnGift();
            }

            // 检测鼠标抬起
            if (Input.GetMouseButtonUp(0))
            {
                isLongPressing = false;
                RemoveRainbowEffect();
            }

            // 检测空格键按下
            if (Input.GetKeyDown(KeyCode.Space))
            {
                isSpacePressing = true;
                lastGiftSpawnTime = Time.time;
                SpawnGift();
            }

            // 检测空格键抬起
            if (Input.GetKeyUp(KeyCode.Space))
            {
                isSpacePressing = false;
                RemoveRainbowEffect();
            }

            // 长按送礼逻辑（鼠标和空格键独立）
            if (isLongPressing || isSpacePressing)
            {
                ApplyRainbowEffect();//炫彩特效

                if (Time.time - lastGiftSpawnTime >= giftSpawnInterval)
                {
                    lastGiftSpawnTime = Time.time;
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
            if (giftContainer == null)
            {
                Debug.LogError("Gift container is not assigned!");
                return;
            }
            
            // 检查是否可以生成特殊礼物
            if (isSpecialGiftAvailable && Random.value < specialGiftProbability)
            {
                SpawnSpecialGift();
                return;
            }
            
            // 生成普通礼物
            SpawnNormalGift();
        }

        private void SpawnNormalGift()
        {
            if (giftPool == null)
            {
                Debug.LogError("GiftPool is not assigned!");
                return;
            }

            // 从对象池获取普通礼物
            IGiftItem giftItem = giftPool.GetGiftItem();
            giftItem.gameObject.transform.SetParent(giftContainer);
            
            // 设置礼物不接收射线检测
            CanvasGroup canvasGroup = giftItem.gameObject.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = giftItem.gameObject.AddComponent<CanvasGroup>();
            }
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
            
            // 设置随机X位置
            float randomX = Random.Range(minX, maxX);
            RectTransform rectTransform = giftItem.gameObject.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(randomX, 0);
            rectTransform.localScale = Vector3.one;
            
            // 初始化礼物
            giftItem.Initialize(screenHeight);
            
            // 设置贡献值
            decimal contributionValue = CoreValueManager.Instance.ClickGiftValue();
            giftItem.SetObtainedContributionValue(CoreValueManager.Instance.FormatValue(contributionValue));

            // 设置礼物图标
            if (giftData != null)
            {
                Sprite giftSprite = giftData.GetRandomGiftSprite((double)contributionValue);
                if (giftSprite != null)
                {
                    giftItem.SetGiftIcon(giftSprite);
                }
            }

            // 确保特殊礼物保持在最上层
            EnsureSpecialGiftOnTop();
        }

        /// <summary>
        /// 确保特殊礼物保持在最上层
        /// </summary>
        private void EnsureSpecialGiftOnTop()
        {
            if (giftContainer == null) return;

            // 查找所有特殊礼物
            for (int i = 0; i < giftContainer.childCount; i++)
            {
                Transform child = giftContainer.GetChild(i);
                SpecialGiftItem specialGift = child.GetComponent<SpecialGiftItem>();
                if (specialGift != null)
                {
                    // 将特殊礼物移到最上层
                    child.SetAsLastSibling();
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

                // 检查是否有可交互的UI元素
                foreach (var result in results)
                {
                    // 检查是否有Button组件且按钮可交互
                    Button button = result.gameObject.GetComponent<Button>();
                    if (button != null && button.interactable)
                    {
                        // 检查按钮是否在活动状态
                        if (button.gameObject.activeInHierarchy)
                        {
                            return true;
                        }
                    }
                    
                    // 检查是否有其他可交互的UI组件
                    Selectable selectable = result.gameObject.GetComponent<Selectable>();
                    if (selectable != null && selectable.interactable && selectable.gameObject.activeInHierarchy)
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
           //改用插件更新 textTotalContribution.text = CoreValueManager.Instance.FormatValueInteger(CoreValueManager.Instance.valueContribution);
        }

        /// <summary>
        /// 更新总贡献值显示（分别是炫彩更新和普通更新）
        /// </summary>
        private void ApplyRainbowEffect()
        {
            textAnimator.SetText($"<wave><rainb>{CoreValueManager.Instance.FormatValueInteger(CoreValueManager.Instance.valueContribution)}</rainb><wave>");
        }

        private void RemoveRainbowEffect()
        {
            textAnimator.SetText(CoreValueManager.Instance.FormatValueInteger(CoreValueManager.Instance.valueContribution));
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

        

        /// <summary>
        /// 生成特殊礼物
        /// </summary>
        private void SpawnSpecialGift()
        {
            if (specialGiftPrefab == null)
            {
                Debug.LogError("Special gift prefab is not assigned!");
                return;
            }

            // 创建特殊礼物
            GameObject specialGift = Instantiate(specialGiftPrefab, giftContainer);
            SpecialGiftItem specialGiftItem = specialGift.GetComponent<SpecialGiftItem>();
            
            if (specialGiftItem == null)
            {
                Debug.LogError("SpecialGiftItem component is missing!");
                Destroy(specialGift);
                return;
            }

            // 设置随机X位置
            float randomX = Random.Range(minX, maxX);
            specialGift.GetComponent<RectTransform>().anchoredPosition = new Vector2(randomX, 0);
            
            // 初始化特殊礼物
            specialGiftItem.Initialize(screenHeight);
            specialGiftItem.SetGiftManager(this);
            
            // 设置特殊礼物图标
            if (giftData != null)
            {
                Sprite specialSprite = giftData.GetSpecialGiftSprite();
                if (specialSprite != null)
                {
                    specialGiftItem.SetGiftIcon(specialSprite);
                }
            }
            
            // 设置特殊礼物文本
            specialGiftItem.SetObtainedContributionValue("特殊礼物");
            
            // 更新冷却时间
            lastSpecialGiftTime = Time.time;
            isSpecialGiftAvailable = false;
            StartCoroutine(ResetSpecialGiftCooldown());
        }

        /// <summary>
        /// 重置特殊礼物冷却时间
        /// </summary>
        private System.Collections.IEnumerator ResetSpecialGiftCooldown()
        {
            yield return new WaitForSeconds(specialGiftCooldown);
            isSpecialGiftAvailable = true;
        }

        /// <summary>
        /// 清理所有礼物，将普通礼物送回对象池，销毁特殊礼物
        /// </summary>
        public void ClearAllGifts()
        {
            if (giftContainer == null) return;

            for (int i = giftContainer.childCount - 1; i >= 0; i--)
            {
                Transform child = giftContainer.GetChild(i);
                NormalGiftItem normalGift = child.GetComponent<NormalGiftItem>();
                SpecialGiftItem specialGift = child.GetComponent<SpecialGiftItem>();

                if (normalGift != null)
                {
                    // 如果是普通礼物，送回对象池
                    giftPool.ReturnGiftItem(normalGift);
                }
                else if (specialGift != null)
                {
                    // 如果是特殊礼物，直接销毁
                    Destroy(child.gameObject);
                }
            }
        }
    }
} 