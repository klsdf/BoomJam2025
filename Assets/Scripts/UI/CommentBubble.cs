/****************************************************************************
 * Author: 周欣悦
 * Date: 2025-04-21
 * Description: 评论气泡
 ****************************************************************************/
namespace BoomJam2025
{
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;

    public class CommentBubble : MonoBehaviour
    {
        private TextMeshProUGUI userNameText;
        private TextMeshProUGUI commentText;
        
        private void Awake()
        {
            // 根据层级结构自动获取组件引用
            userNameText = transform.Find("UserName").GetComponent<TextMeshProUGUI>();
            commentText = transform.Find("TextBG/TextComment").GetComponent<TextMeshProUGUI>();
            
            if (userNameText == null || commentText == null)
            {
                Debug.LogError("CommentBubble: 无法找到必要的文本组件！");
            }
        }
        
        public void Initialize(string username, string comment)
        {
            if (userNameText != null && commentText != null)
            {
                userNameText.text = username;
                commentText.text = comment;
                gameObject.SetActive(true);
            }
        }
    }
}
