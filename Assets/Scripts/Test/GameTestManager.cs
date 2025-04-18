using UnityEngine;

namespace BoomJam2025
{
    public class GameTestManager : MonoBehaviour
    {
        [Header("会员等级设置")]
        [SerializeField] private int memberLevel = 1;
        [SerializeField] private bool applyMemberLevel = false;

        [Header("粉丝等级设置")]
        [SerializeField] private int fanLevel = 1;
        [SerializeField] private bool applyFanLevel = false;

        [Header("贡献值设置")]
        [SerializeField] private double contributionValue = 0;
        [SerializeField] private bool applyContribution = false;

        [Header("局外点数设置")]
        [SerializeField] private int outerPoints = 0;
        [SerializeField] private bool applyOuterPoints = false;

        [Header("会员权益设置")]
        [SerializeField] private int baseClickValueLevel = 1;
        [SerializeField] private int percentagePerLevel = 1;
        [SerializeField] private int reductionFactorLevel = 1;
        [SerializeField] private int criticalRateLevel = 1;
        [SerializeField] private int criticalMultiplierLevel = 1;
        [SerializeField] private bool applyMemberBenefits = false;

        private void Update()
        {
            if (applyMemberLevel)
            {
                MemberLevelManager.Instance.levelMember = memberLevel;
                applyMemberLevel = false;
            }

            if (applyFanLevel)
            {
                FanLevelManager.Instance.levelFan = fanLevel;
                applyFanLevel = false;
            }

            if (applyContribution)
            {
                CoreValueManager.Instance.valueContribution = contributionValue;
                applyContribution = false;
            }

            if (applyOuterPoints)
            {
                MemberBenefitManager.Instance.pointsOuter = outerPoints;
                applyOuterPoints = false;
            }

            if (applyMemberBenefits)
            {
                MemberBenefitManager.Instance.levelBaseClickValue = baseClickValueLevel;
                MemberBenefitManager.Instance.levelPercentagePer = percentagePerLevel;
                MemberBenefitManager.Instance.levelReductionFactor = reductionFactorLevel;
                MemberBenefitManager.Instance.levelCriticalRate = criticalRateLevel;
                MemberBenefitManager.Instance.levelCriticalMultiplier = criticalMultiplierLevel;
                applyMemberBenefits = false;
            }
        }
    }
}