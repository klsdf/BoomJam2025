namespace BoomJam2025
{
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;
    using UnityEngine.EventSystems;
    using MoreMountains.Feedbacks;
    
    /// <summary>
    /// 特殊礼物项
    /// </summary>
    public class SpecialGiftItem : MonoBehaviour, IGiftItem
    {
        [Header("UI Components")]
        public Image imageGiftIcon;
        public GameObject textFadeOut;
        
        [Header("Animation Settings")]
        public float fallSpeed = 500f;
        public float stayDuration = 5f;
        public float fadeOutDuration = 1f;

        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;
        private float startY;
        private float targetY;
        private float currentStayTime;
        private bool isFalling = true;
        private bool isFading = false;
        private GiftManager giftManager;
        private bool isInitialized = false;
        private bool isAnimating = false;

        private Button button;
        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            button = GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(OnButtonClick);
            }
        }

        public void Initialize(float screenHeight)
        {
            startY = screenHeight;
            targetY = 0;
            currentStayTime = 0f;
            isFalling = true;
            isFading = false;
            canvasGroup.alpha = 1f;
            isInitialized = true;
            
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
                    Destroy(gameObject);
                }
            }
        }

        /// <summary>
        /// 处理特殊礼物的奖励
        /// </summary>
        private void HandleSpecialGiftReward(SpecialGiftItem specialGift)
        {
            float rewardType = Random.value;
            
            if (rewardType < 0.9f) // 90% 概率获得贡献值
            {
                double contributionValue = CoreValueManager.Instance.GetCritValueAtLevel(200);
                CoreValueManager.Instance.valueContribution += contributionValue;
                ShowFadeOutText($"Contribution +{contributionValue}");
            }
            else // 10% 概率获得加点
            {
                MemberBenefitManager.Instance.pointsOuter += 1;
                ShowFadeOutText($"Point +1");
            }
            
            
        }
        
        private void OnButtonClick()
        {
            if (!isInitialized || isAnimating) return;
            
            isAnimating = true;
            button.interactable = false;
            
            var feedbackPlayer = textFadeOut.GetComponent<MMF_Player>();
            feedbackPlayer.Events.OnComplete.AddListener(() => {
                // 销毁特殊礼物
                Destroy(gameObject);
            });
            
            HandleSpecialGiftReward(this);
        }

        /// <summary>
        /// 显示淡出文本
        /// </summary>
        /// <param name="text">要显示的文本内容</param>
        private void ShowFadeOutText(string text)
        {
            if (textFadeOut == null) return;
            
            textFadeOut.GetComponent<TextMeshProUGUI>().text = text;
            textFadeOut.GetComponent<MMF_Player>().PlayFeedbacks();
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
            
        }

        public void SetGiftManager(GiftManager manager)
        {
            giftManager = manager;
        }

        GameObject IGiftItem.gameObject => gameObject;
    }
} 