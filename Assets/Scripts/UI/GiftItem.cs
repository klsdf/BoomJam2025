/****************************************************************************
 * Author: 周欣悦
 * Date: 2025-04-16
 * Description: 礼物UI项的行为控制脚本
 ****************************************************************************/

namespace BoomJam2025
{
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;

    /// <summary>
    /// 控制单个礼物项的显示和动画效果
    /// </summary>
    /// <remarks>
    /// 该类负责管理单个礼物UI项的显示、动画和生命周期。
    /// 礼物会从屏幕顶部掉落，停留一段时间后淡出消失。
    /// </remarks>
    public class GiftItem : MonoBehaviour
    {
        [Header("UI Components")]
        /// <summary>
        /// 礼物图标图片组件
        /// </summary>
        public Image imageGiftIcon;

        /// <summary>
        /// 贡献值文本显示组件
        /// </summary>
        public TextMeshProUGUI textContribution;
        
        [Header("Animation Settings")]
        /// <summary>
        /// 礼物掉落速度（像素/秒）
        /// </summary>
        public float fallSpeed = 500f;
        
        /// <summary>
        /// 礼物停留时间（秒）
        /// </summary>
        public float stayDuration = 2f;
        
        /// <summary>
        /// 淡出动画持续时间（秒）
        /// </summary>
        public float fadeOutDuration = 0.5f;
        
        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;
        private float startY;
        private float targetY;
        private float currentStayTime;
        private bool isFalling = true;
        private bool isFading = false;
        private GiftPool giftPool;

        /// <summary>
        /// 初始化组件引用
        /// </summary>
        /// <remarks>
        /// 在Awake中初始化必要的组件引用，包括RectTransform、CanvasGroup和GiftPool。
        /// 如果CanvasGroup不存在，会自动添加。
        /// </remarks>
        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            giftPool = FindObjectOfType<GiftPool>();
        }

        /// <summary>
        /// 初始化礼物项
        /// </summary>
        /// <param name="screenHeight">屏幕高度，用于设置初始位置</param>
        /// <remarks>
        /// 重置所有状态并设置初始位置到屏幕顶部。
        /// </remarks>
        public void Initialize(float screenHeight)
        {
            startY = screenHeight;
            targetY = 0;
            currentStayTime = 0f;
            isFalling = true;
            isFading = false;
            canvasGroup.alpha = 1f;
            
            // 设置初始位置
            rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, startY);
        }

        /// <summary>
        /// 更新礼物动画状态
        /// </summary>
        /// <remarks>
        /// 处理礼物的掉落、停留和淡出动画。
        /// </remarks>
        private void Update()
        {
            if (isFalling)
            {
                float newY = rectTransform.anchoredPosition.y - fallSpeed * Time.deltaTime;
                rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, newY);
                
                if (newY <= targetY)
                {
                    isFalling = false;
                }
            }
            else if (!isFading)
            {
                currentStayTime += Time.deltaTime;
                if (currentStayTime >= stayDuration)
                {
                    isFading = true;
                }
            }
            else
            {
                canvasGroup.alpha -= Time.deltaTime / fadeOutDuration;
                if (canvasGroup.alpha <= 0)
                {
                    ReturnToPool();
                }
            }
        }

        /// <summary>
        /// 将礼物返回到对象池
        /// </summary>
        /// <remarks>
        /// 如果对象池存在，将礼物返回到池中；否则销毁对象。
        /// </remarks>
        private void ReturnToPool()
        {
            if (giftPool != null)
            {
                giftPool.ReturnGiftItem(this);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 设置单次点击获得贡献值显示文字
        /// </summary>
        /// <param name="value">格式化后贡献值</param>
        public void SetObtainedContributionValue(string value)
        {
            if (textContribution != null)
            {
                textContribution.text = value;
            }
        }

        /// <summary>
        /// 设置礼物图标
        /// </summary>
        /// <param name="sprite">礼物图标</param>
        public void SetGiftIcon(Sprite sprite)
        {
            if (imageGiftIcon != null)
            {
                imageGiftIcon.sprite = sprite;
            }
        }
    }
} 