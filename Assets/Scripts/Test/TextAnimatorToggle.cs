using UnityEngine;
using Febucci.UI; // 引入 Text Animator 的命名空间

public class TextAnimatorToggle : MonoBehaviour
{
    public TextAnimator_TMP textAnimator; // 在 Inspector 中绑定 TextAnimator_TMP 组件

    private string baseText = "This is shaking text."; // 普通文本
    private string animatedText = "This is <rainb>shaking</rainb> text."; // 含特效的文本

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            textAnimator.SetText(animatedText); // 应用特效
        }

        if (Input.GetKeyUp(KeyCode.Z))
        {
            textAnimator.SetText(baseText); // 移除特效
        }
    }
}
