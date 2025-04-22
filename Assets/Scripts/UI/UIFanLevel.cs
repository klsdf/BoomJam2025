/****************************************************************************
 * Author: 周欣悦
 * Date: 2025-04-17
 * Description: 粉丝等级信息显示UI
 ****************************************************************************/

namespace BoomJam2025
{
    using UnityEngine;
    using TMPro;
    using UnityEngine.UI;

    public class UIFanLevel : MonoBehaviour
    {
        [Header("UI References")]
        /// <summary>
        /// 当前等级
        /// </summary>
        [SerializeField] private TextMeshProUGUI textCurrentLevel;

        /// <summary>
        /// 点击价值计算公式
        /// </summary>
        [SerializeField] private TextMeshProUGUI textComputationalFormula;

        /// <summary>
        /// 单次点击价值
        /// </summary>
        [SerializeField] private TextMeshProUGUI textSingleClickValue;

        /// <summary>
        /// 升级所需贡献值
        /// </summary>
        [SerializeField] private TextMeshProUGUI textRequiredContribution;

        /// <summary>
        /// 暴击率
        /// </summary>
        [SerializeField] private TextMeshProUGUI textCriticalRate;

        private void Start()
        {
            UpdateUI();
        }

        private void Update()
        {
            UpdateUI();
        }

        /// <summary>
        /// 更新UI显示
        /// </summary>
        private void UpdateUI()
        {
            if (textCurrentLevel != null)
            {
                textCurrentLevel.text = $"Lv.{FanLevelManager.Instance.levelFan}";
            }

            if (textComputationalFormula != null)
            {
                decimal percentage = MemberBenefitManager.Instance.GetPercentagePer() * (FanLevelManager.Instance.levelFan - 1);
                textComputationalFormula.text = $"{percentage:P0}";
            }

            if (textSingleClickValue != null)
            {
                decimal singleClickValue = CoreValueManager.Instance.GetClickValue();
                textSingleClickValue.text = $"{CoreValueManager.Instance.FormatValue(singleClickValue)}";
            }

            if (textRequiredContribution != null)
            {
                decimal required = FanLevelManager.Instance.GetUpgradeCost();
                textRequiredContribution.text = $"{CoreValueManager.Instance.FormatValueShort(required)}";
            }

            if (textCriticalRate != null)
            {
                decimal critRate = MemberBenefitManager.Instance.GetCriticalRate();
                textCriticalRate.text = $"暴击率+{critRate:P0}";
            }
        }

        /// <summary>
        /// 升级粉丝等级
        /// </summary>
        public void UpgradeFanLevel()
        {
            if (FanLevelManager.Instance.TryUpgrade() != 0)
            {
                UpdateUI();
            }
            else
            {
                Debug.Log("升级粉丝等级失败");
            }
        }

        /// <summary>
        /// 升级粉丝等级到最大
        /// </summary>
        public void UpgradeFanLevelMax()
        {
            // 循环尝试升级，直到升级失败
            while (FanLevelManager.Instance.TryUpgrade() != 0)
            {
                // 升级成功，继续循环
                continue;
            }
            
            // 更新UI显示
            UpdateUI();
        }
    }
} 