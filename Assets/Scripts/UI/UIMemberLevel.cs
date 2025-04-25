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
    using UnityEngine.Events;

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

        [Header("Events")]
        /// <summary>
        /// 升级成功事件
        /// </summary>
        [SerializeField] private UnityEvent onUpgradeSuccess;

        /// <summary>
        /// 升级失败事件
        /// </summary>
        [SerializeField] private UnityEvent onUpgradeFailed;

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
                decimal required = MemberLevelManager.Instance.GetUpgradeCost();
                textRequiredContribution.text = $"{FormatValueShort(required)}";
            }
        }

        /// <summary>
        /// 升级会员等级
        /// </summary>
        public void UpgradeMemberLevel()
        {
            if (MemberLevelManager.Instance.TryUpgrade() != 0)
            {
                OnUpgradeSuccess();
                UpdateUI();
            }
            else
            {
                OnUpgradeFailed();
                Debug.Log("升级会员等级失败");
            }
        }

        /// <summary>
        /// 会员等级升级成功回调
        /// </summary>
        private void OnUpgradeSuccess()
        {
            onUpgradeSuccess?.Invoke();
            Debug.Log("会员等级升级成功！");
        }

        /// <summary>
        /// 会员等级升级失败回调
        /// </summary>
        private void OnUpgradeFailed()
        {
            onUpgradeFailed?.Invoke();
        }

        private string FormatValueShort(decimal value)
        {
            if (value < 1000)
                return (value / 1000).ToString() + "K";
            else if (value < 1000000)
                return ((int)(value / 1000)).ToString() + "K";
            else
                return ((int)(value / 1000000)).ToString() + "M";
        }
    }    
} 