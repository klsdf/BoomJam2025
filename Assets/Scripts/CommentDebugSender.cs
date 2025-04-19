/****************************************************************************
 * Author: 张嘉阳
 * Date: 2025-04-19
 * Description: 按 C 键发送一条测试评论
 ****************************************************************************/

namespace BoomJam2025
{
    using UnityEngine;

    /// <summary>
    /// 用于测试评论系统，按 C 键发送一条评论
    /// </summary>
    public class CommentDebugSender : MonoBehaviour
    {
        private int commentIndex = 1;
        private void Start()
        {
            CommentManager.Instance.AddComment("Hello");
            CommentManager.Instance.AddComment("www");
            CommentManager.Instance.AddComment("2333");
        }
        private void Update()
        {

            if (Input.GetKeyDown(KeyCode.C))
            {
                string content = $"test #{commentIndex++}";
                CommentManager.Instance.AddComment(content);
                Debug.Log($"发送评论：{content}");
            }
        }
    }
}
