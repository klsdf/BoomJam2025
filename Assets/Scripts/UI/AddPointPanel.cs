using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

namespace BoomJam2025
{
    public class AddPointPanel : MonoBehaviour
    {
        [SerializeField]
        private BenefitType currentValueType;

        [Header("Events")]
        /// <summary>
        /// 升级成功事件
        /// </summary>
        [SerializeField] private UnityEvent onUpgradeSuccess;

        /// <summary>
        /// 升级失败事件
        /// </summary>
        [SerializeField] private UnityEvent onUpgradeFailed;

        private TextMeshProUGUI textTitle;
        private TextMeshProUGUI textValue;
        private TextMeshProUGUI textLevel;
        private Button buttonAddPoint;
        private TextMeshProUGUI textRequirePoint;

        private void Awake()
        {
            // 获取所有子组件
            textTitle = transform.Find("Title").GetComponent<TextMeshProUGUI>();
            textValue = transform.Find("Value").GetComponent<TextMeshProUGUI>();
            textLevel = transform.Find("Level").GetComponent<TextMeshProUGUI>();
            buttonAddPoint = transform.Find("ButtonAddPoint").GetComponent<Button>();
            textRequirePoint = transform.Find("ButtonAddPoint/TextRequirePoint").GetComponent<TextMeshProUGUI>();

            UpdateValue();

            buttonAddPoint.onClick.AddListener(OnAddButtonClick);
        }

        private void Update()
        {
            UpdateValue();
        }
        private void OnAddButtonClick()
        {
            if(MemberBenefitManager.Instance.UpgradeBenefit(currentValueType))
            {
                OnUpgradeSuccess();
                UpdateValue();
            }
            else
            {
                OnUpgradeFailed();
                Debug.Log("升级会员权益失败失败");
            }
        }

        /// <summary>
        /// 升级成功回调
        /// </summary>
        private void OnUpgradeSuccess()
        {
            onUpgradeSuccess?.Invoke();
            Debug.Log("会员权益升级成功！");
        }

        /// <summary>
        /// 升级失败回调
        /// </summary>
        private void OnUpgradeFailed()
        {
            onUpgradeFailed?.Invoke();
            Debug.Log("会员权益升级失败，可能是点数不足");
        }

        // 更新值的显示
        public void UpdateValue()
        {
            switch (currentValueType)
            {
                case BenefitType.BaseClickValue:
                    textTitle.text = "基础价值";
                    textValue.text = $"X{MemberBenefitManager.Instance.GetBaseClickValue()}";
                    textRequirePoint.text = $"{MemberBenefitManager.Instance.levelBaseClickValue}";
                    textLevel.text = $"Lv.{MemberBenefitManager.Instance.levelBaseClickValue}";
                    break;
                case BenefitType.PercentagePer:
                    textTitle.text = "每级提升";
                    textValue.text = $"{MemberBenefitManager.Instance.GetPercentagePer():P0}";
                    textRequirePoint.text = $"{MemberBenefitManager.Instance.levelPercentagePer}";
                    textLevel.text = $"Lv.{MemberBenefitManager.Instance.levelPercentagePer}";
                    break;
                case BenefitType.ReductionFactor:
                    textTitle.text = "升级消耗";
                    textValue.text = $"/{MemberBenefitManager.Instance.GetReductionFactor()}";
                    textRequirePoint.text = $"{MemberBenefitManager.Instance.levelReductionFactor}";
                    textLevel.text = $"Lv.{MemberBenefitManager.Instance.levelReductionFactor}";
                    break;
                case BenefitType.CriticalRate:
                    textTitle.text = "暴击率";
                    textValue.text = $"{MemberBenefitManager.Instance.GetCriticalRate():P0}";
                    textRequirePoint.text = $"{MemberBenefitManager.Instance.levelCriticalRate}";
                    textLevel.text = $"Lv.{MemberBenefitManager.Instance.levelCriticalRate}";
                    break;
                case BenefitType.CriticalMultiplier:
                    textTitle.text = "暴击倍率";
                    textValue.text = $"X{MemberBenefitManager.Instance.GetCriticalMultiplier()}";
                    textRequirePoint.text = $"{MemberBenefitManager.Instance.levelCriticalMultiplier}";
                    textLevel.text = $"Lv.{MemberBenefitManager.Instance.levelCriticalMultiplier}";
                    break;
            }
        }
    }
}
