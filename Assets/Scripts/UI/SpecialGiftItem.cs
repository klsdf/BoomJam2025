namespace BoomJam2025
{
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;
    using UnityEngine.EventSystems;
    using MoreMountains.Feedbacks;
    using DG.Tweening;
    
    /// <summary>
    /// 特殊礼物项
    /// </summary>
    public class SpecialGiftItem : MonoBehaviour, IGiftItem
    {
        [Header("UI Components")]
        public Image imageGiftIcon;
        public GameObject textFadeOut;
        public Image imageRewardIcon;
        public Sprite spriteContribution;
        public Sprite spritePoints;
        
        [Header("Animation Settings")]
        public float fallDuration = 1f;
        public float stayDuration = 5f;
        public float fadeOutDuration = 1f;
        public Ease fallEase = Ease.InQuad;

        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;
        private float startY;
        private float targetY;
        private bool isAnimating = false;
        private Tween fallTween;
        private Tween fadeTween;
        private Tween delayTween;
        private GiftManager giftManager;

        private Button button;
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

            button = GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(OnButtonClick);
            }
        }

        public void Initialize(float screenHeight)
        {
            // 清理之前的动画
            KillAllTweens();
            
            startY = screenHeight;
            targetY = 0;
            canvasGroup.alpha = 1f;
            
            rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, startY);
            
            // 使用 DOTween 实现自由落体运动
            fallTween = rectTransform.DOAnchorPosY(targetY, fallDuration)
                .SetEase(fallEase)
                .OnComplete(() => {
                    if (this == null || gameObject == null) return;
                    // 开始停留计时
                    delayTween = DOVirtual.DelayedCall(stayDuration, () => {
                        if (this == null || gameObject == null) return;
                        // 开始淡出
                        fadeTween = canvasGroup.DOFade(0, fadeOutDuration)
                            .OnComplete(() => {
                                if (this == null || gameObject == null) return;
                                if (!isAnimating) // 如果没有被点击，则销毁
                                {
                                    Destroy(gameObject);
                                }
                            });
                    });
                });
        }

        private void KillAllTweens()
        {
            if (fallTween != null && fallTween.IsActive())
            {
                fallTween.Kill();
                fallTween = null;
            }
            if (fadeTween != null && fadeTween.IsActive())
            {
                fadeTween.Kill();
                fadeTween = null;
            }
            if (delayTween != null && delayTween.IsActive())
            {
                delayTween.Kill();
                delayTween = null;
            }
        }

        private void OnDestroy()
        {
            KillAllTweens();
        }

        /// <summary>
        /// 处理特殊礼物的奖励
        /// </summary>
        private void HandleSpecialGiftReward(SpecialGiftItem specialGift)
        {
            float rewardType = Random.value;
            
            if (rewardType < 0.7f) // 70% 概率获得贡献值
            {
                int level = 60 + FanLevelManager.Instance.levelFan;
                decimal contributionValue = CoreValueManager.Instance.GetCritValueAtLevel(level);
                CoreValueManager.Instance.valueContribution += contributionValue;
                ShowFadeOutText($"贡献值+{CoreValueManager.Instance.FormatValue(contributionValue)}", spriteContribution);
            }
            else // 30% 概率获得加点
            {
                MemberBenefitManager.Instance.pointsOuter += 1;
                ShowFadeOutText($"点数+1", spritePoints);
            }
        }
        
        private void OnButtonClick()
        {
            if (isAnimating) return;
            
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
        /// 显示淡出文本和图标
        /// </summary>
        /// <param name="text">要显示的文本内容</param>
        /// <param name="rewardSprite">奖励类型图标</param>
        private void ShowFadeOutText(string text, Sprite rewardSprite)
        {
            if (textFadeOut == null) return;
            
            textFadeOut.GetComponent<TextMeshProUGUI>().text = text;
            if (imageRewardIcon != null)
            {
                imageRewardIcon.sprite = rewardSprite;
                imageRewardIcon.gameObject.SetActive(true);
            }
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
            // 特殊礼物不需要显示贡献值
        }

        public void SetGiftManager(GiftManager manager)
        {
            giftManager = manager;
        }

        GameObject IGiftItem.gameObject => gameObject;
    }
} 