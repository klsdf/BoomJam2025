using UnityEngine;
using UnityEngine.UI;

namespace BoomJam2025
{
    public class ContributionProgressSlider : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Slider progressSlider;
        [SerializeField] private Text progressText;

        private void Start()
        {
            if (progressSlider == null)
            {
                progressSlider = GetComponent<Slider>();
            }
            
            // 设置Slider的最大值为1万亿
            progressSlider.maxValue = (float)1000000000000m;
            progressSlider.minValue = 0;
        }

        private void Update()
        {
            // 获取当前贡献值
            decimal currentValue = CoreValueManager.Instance.valueContribution;
            
            // 更新Slider的值
            progressSlider.value = (float)currentValue;
            
            // 更新进度文本
            if (progressText != null)
            {
                progressText.text = $"{CoreValueManager.Instance.FormatValue(currentValue)} / {CoreValueManager.Instance.FormatValue(1000000000000m)}";
            }
        }
    }
} 