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

    public class UIMemberLevel : MonoBehaviour
    {
        [Header("UI References")]
        /// <summary>
        /// 当前等级
        /// </summary>
        [SerializeField] private TextMeshProUGUI textCurrentLevel;

        /// <summary>
        /// 剩余点数
        /// </summary>
        [SerializeField] private TextMeshProUGUI textLeftPoints;

        /// <summary>
        /// 升级所需贡献值
        /// </summary>
        [SerializeField] private TextMeshProUGUI textRequiredContribution;

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
                textCurrentLevel.text = $"Lv.{MemberLevelManager.Instance.levelMember}";
            }

            if (textLeftPoints != null)
            {
                textLeftPoints.text = $"剩余点数：{MemberBenefitManager.Instance.pointsOuter}";
            }

            if (textRequiredContribution != null)
            {
                double required = MemberLevelManager.Instance.GetUpgradeCost();
                textRequiredContribution.text = $"{GetThousands(required)}K";
            }

        }

        /// <summary>
        /// 升级会员等级
        /// </summary>
        public void UpgradeMemberLevel()
        {
            if (MemberLevelManager.Instance.TryUpgrade() != 0)
            {
                UpdateUI();
            }
            else
            {
                Debug.Log("升级会员等级失败");
            }
        }

        /// <summary>
        /// 获取千位数字
        /// </summary>
        /// <param name="number">数字</param>
        /// <returns>千位数字</returns>
        private string GetThousands(double number)
        {
            if (number == 0) return "0";
            
            number = System.Math.Abs(number);
            // 将数字除以1000并向下取整
            double thousands = System.Math.Floor(number / 1000);
            
            return thousands.ToString("0");
        }
    }
} 