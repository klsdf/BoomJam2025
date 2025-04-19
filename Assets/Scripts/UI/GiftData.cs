/****************************************************************************
 * Author: 周欣悦
 * Date: 2025-04-17
 * Description: 礼物数据配置
 ****************************************************************************/

namespace BoomJam2025
{
    using UnityEngine;
    using System.Collections.Generic;

    /// <summary>
    /// 礼物数据类，用于存储礼物价值与图片的映射关系
    /// </summary>
    [CreateAssetMenu(fileName = "GiftData", menuName = "BoomJam2025/Gift Data")]
    public class GiftData : ScriptableObject
    {
        [System.Serializable]
        public class GiftValueGroup
        {
            /// <summary>
            /// 最小值
            /// </summary>
            public double minValue;
            
            /// <summary>
            /// 最大值，-1 表示无穷大
            /// </summary>
            public double maxValue;
            
            /// <summary>
            /// 礼物图片列表
            /// </summary>
            public List<Sprite> giftSprites;

            /// <summary>
            /// 检查贡献值是否在区间内
            /// </summary>
            public bool IsInRange(double value)
            {
                return value >= minValue && (maxValue == -1 || value < maxValue);
            }
        }

        public List<GiftValueGroup> valueGroups = new List<GiftValueGroup>();

        [Header("Special Gift Settings")]
        /// <summary>
        /// 特殊礼物图标
        /// </summary>
        public Sprite specialGiftSprite;

        /// <summary>
        /// 根据贡献值获取对应的随机礼物图片
        /// </summary>
        public Sprite GetRandomGiftSprite(double contributionValue)
        {
            foreach (var group in valueGroups)
            {
                if (group.IsInRange(contributionValue))
                {
                    if (group.giftSprites.Count > 0)
                    {
                        int randomIndex = Random.Range(0, group.giftSprites.Count);
                        return group.giftSprites[randomIndex];
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 获取特殊礼物图标
        /// </summary>
        public Sprite GetSpecialGiftSprite()
        {
            return specialGiftSprite;
        }
    }
} 