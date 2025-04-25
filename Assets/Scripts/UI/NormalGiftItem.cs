namespace BoomJam2025
{
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;
    using DG.Tweening;

    /// <summary>
    /// 普通礼物项
    /// </summary>
    public class NormalGiftItem : MonoBehaviour, IGiftItem
    {
        [Header("UI Components")]
        public Image imageGiftIcon;
        public TextMeshProUGUI textContribution;
        
        [Header("Animation Settings")]
        public float fallDuration = 1f;
        public float screenBottomOffset = -100f; // 屏幕下边界的偏移量
        public Ease fallEase = Ease.InQuad;

        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;
        private float startY;
        private float targetY;
        private GiftPool giftPool;
        private Tween fallTween;
        private Canvas canvas;

        private void Awake()
        {
            // 设置 DOTween 容量
            DOTween.SetTweensCapacity(200, 50);
            
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            giftPool = FindObjectOfType<GiftPool>();
            
            // 获取画布
            if (GiftManager.Instance != null && GiftManager.Instance.giftContainer != null)
            {
                canvas = GiftManager.Instance.giftContainer.GetComponentInParent<Canvas>();
                if (canvas == null)
                {
                    Debug.LogWarning("Canvas not found in gift container hierarchy");
                }
            }
            else
            {
                Debug.LogWarning("GiftManager or giftContainer not found");
            }
        }

        public void Initialize(float screenHeight)
        {
            // 清理之前的动画
            if (fallTween != null && fallTween.IsActive())
            {
                fallTween.Kill();
                fallTween = null;
            }
            
            startY = screenHeight;
            
            // 计算目标位置
            if (GiftManager.Instance != null && GiftManager.Instance.mainCamera != null && canvas != null)
            {
                // 使用相机坐标计算屏幕下边界
                Vector3 bottomLeft = GiftManager.Instance.mainCamera.ScreenToWorldPoint(new Vector3(0, 0, 0));
                targetY = bottomLeft.y + screenBottomOffset;
            }
            else
            {
                // 备选方案：使用固定值
                targetY = -screenHeight;
            }
            
            canvasGroup.alpha = 1f;
            
            rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, startY);
            
            // 使用 DOTween 实现自由落体运动
            fallTween = rectTransform.DOAnchorPosY(targetY, fallDuration)
                .SetEase(fallEase)
                .OnComplete(() => {
                    if (this == null || gameObject == null) return;
                    // 直接回收，不再停留
                    ReturnToPool();
                });
        }

        private void OnDestroy()
        {
            if (fallTween != null && fallTween.IsActive())
            {
                fallTween.Kill();
                fallTween = null;
            }
        }

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

        public void SetGiftIcon(Sprite sprite)
        {
            if (imageGiftIcon != null)
            {
                imageGiftIcon.sprite = sprite;
            }
        }

        public void SetObtainedContributionValue(string value)
        {
            if (textContribution != null)
            {
                textContribution.text = $"+{value}";
            }
        }

        GameObject IGiftItem.gameObject => gameObject;
    }
} 