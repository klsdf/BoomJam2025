/****************************************************************************
 * Author: 张嘉阳
 * Date: 2025-04-24
 * Description: 挂载此代码使物体Enables时自动播放Typewriter动画
 ****************************************************************************/
using UnityEngine;
using Febucci.UI;

[RequireComponent(typeof(TypewriterByCharacter))]
public class AutoTextAnimator : MonoBehaviour
{
    private TypewriterByCharacter typewriter;
    private string originalText;

    void Awake()
    {
        typewriter = GetComponent<TypewriterByCharacter>();
        originalText = typewriter.GetComponent<TextAnimator_TMP>().textFull;
    }

    void OnEnable()
    {
        if (typewriter != null)
        {
            typewriter.ShowText(originalText);
        }
    }
}
