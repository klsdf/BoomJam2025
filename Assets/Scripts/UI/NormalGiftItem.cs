namespace BoomJam2025
{
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;

    /// <summary>
    /// 普通礼物项
    /// </summary>
    public class NormalGiftItem : MonoBehaviour, IGiftItem
    {
        [Header("UI Components")]
        public Image imageGiftIcon;
        public TextMeshProUGUI textContribution;
        
        [Header("Animation Settings")]
        public float fallSpeed = 500f;
        public float stayDuration = 2f;
        public float fadeOutDuration = 0.5f;

        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;
        private float startY;
        private float targetY;
        private float currentStayTime;
        private bool isFalling = true;
        private bool isFading = false;
        private GiftPool giftPool;

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

        public void Initialize(float screenHeight)
        {
            startY = screenHeight;
            targetY = 0;
            currentStayTime = 0f;
            isFalling = true;
            isFading = false;
            canvasGroup.alpha = 1f;
            
            rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, startY);
        }

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