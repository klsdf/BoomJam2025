using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System.Linq;

namespace BoomJam2025
{
    public class RankingListUI : MonoBehaviour
    {
        [System.Serializable]
        public class RankerInfo
        {
            public Sprite avatar;
            public string nickname;
            [TextArea(1, 3)]
            public string scoreString;
            public bool isPlayer;

            public RankerInfo Clone()
            {
                return new RankerInfo
                {
                    avatar = this.avatar,
                    nickname = this.nickname,
                    scoreString = this.scoreString,
                    isPlayer = this.isPlayer
                };
            }
        }

        [Header("预设NPC信息")]
        [SerializeField] private RankerInfo[] npcRankers = new RankerInfo[3];

        [Header("UI引用")]
        [SerializeField] private Image[] rankerAvatars = new Image[3];
        [SerializeField] private TextMeshProUGUI[] rankerNicknames = new TextMeshProUGUI[3];
        [SerializeField] private TextMeshProUGUI[] rankerScores = new TextMeshProUGUI[3];

        [Header("玩家信息")]
        [SerializeField] private Sprite playerAvatar;
        [SerializeField] private string playerNickname;

        private List<RankerInfo> allRankers = new List<RankerInfo>();
        private List<decimal> allScores = new List<decimal>();

        private void Start()
        {
            InitializeRanking();
        }

        private void Update()
        {
            UpdateRanking();
        }

        private void InitializeRanking()
        {
            allRankers.Clear();
            allScores.Clear();

            // 添加3个NPC
            for (int i = 0; i < 3; i++)
            {
                if (i < npcRankers.Length && npcRankers[i] != null)
                {
                    var npcInfo = npcRankers[i].Clone();
                    npcInfo.isPlayer = false;
                    allRankers.Add(npcInfo);

                    decimal score;
                    if (decimal.TryParse(npcRankers[i].scoreString, out score))
                    {
                        allScores.Add(score);
                    }
                    else
                    {
                        Debug.LogError($"无法解析NPC {i} 的分数: {npcRankers[i].scoreString}");
                        allScores.Add(0m);
                    }
                }
            }

            // 添加玩家作为第四个位置
            var playerInfo = new RankerInfo
            {
                avatar = playerAvatar,
                nickname = playerNickname,
                scoreString = "0",
                isPlayer = true
            };
            allRankers.Add(playerInfo);
            allScores.Add(0m);

            SortRankingList();
            UpdateUI();
        }

        public void UpdateRanking()
        {
            decimal playerScore = CoreValueManager.Instance.valueContribution;

            // 更新玩家分数
            for (int i = 0; i < allRankers.Count; i++)
            {
                if (allRankers[i].isPlayer)
                {
                    allScores[i] = playerScore;
                    allRankers[i].scoreString = playerScore.ToString();
                    break;
                }
            }

            SortRankingList();
            UpdateUI();
        }

        private void SortRankingList()
        {
            // 使用冒泡排序同时排序两个列表
            for (int i = 0; i < allScores.Count - 1; i++)
            {
                for (int j = 0; j < allScores.Count - 1 - i; j++)
                {
                    if (allScores[j] < allScores[j + 1])
                    {
                        // 交换分数
                        decimal tempScore = allScores[j];
                        allScores[j] = allScores[j + 1];
                        allScores[j + 1] = tempScore;

                        // 交换信息
                        RankerInfo tempInfo = allRankers[j];
                        allRankers[j] = allRankers[j + 1];
                        allRankers[j + 1] = tempInfo;
                    }
                }
            }
        }

        private void UpdateUI()
        {
            // 只显示前三名
            for (int i = 0; i < 3; i++)
            {
                if (i < allRankers.Count)
                {
                    rankerAvatars[i].sprite = allRankers[i].avatar;
                    rankerNicknames[i].text = allRankers[i].nickname;
                    rankerScores[i].text = CoreValueManager.Instance.FormatValue(allScores[i]);
                }
            }
        }

        public void SetPlayerAvatar(Sprite avatar)
        {
            playerAvatar = avatar;
            // 更新玩家头像
            for (int i = 0; i < allRankers.Count; i++)
            {
                if (allRankers[i].isPlayer)
                {
                    allRankers[i].avatar = avatar;
                    break;
                }
            }
            UpdateUI();
        }

        public void SetPlayerNickname(string nickname)
        {
            playerNickname = nickname;
            // 更新玩家昵称
            for (int i = 0; i < allRankers.Count; i++)
            {
                if (allRankers[i].isPlayer)
                {
                    allRankers[i].nickname = nickname;
                    break;
                }
            }
            UpdateUI();
        }
    }
}
