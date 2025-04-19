/****************************************************************************
 * Author: 张嘉阳
 * Date: 2025-04-19
 * Description: 单条评论 UI 项
 ****************************************************************************/
namespace BoomJam2025
{
    using UnityEngine;
    using TMPro;

    /// <summary>
    /// 一行评论展示
    /// </summary>
    public class CommentItem : MonoBehaviour
    {
        [Header("UI 组件")]
        public TextMeshProUGUI textComment;

        /// <summary>初始化文本</summary>
        public void Initialize(string comment)
        {
            textComment.text = comment;
        }
    }
}
