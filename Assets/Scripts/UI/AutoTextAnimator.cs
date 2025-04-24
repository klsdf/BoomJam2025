/****************************************************************************
 * Author: 张嘉阳
 * Date: 2025-04-24
 * Description: 挂载在含有TypeWriter的组件上以实现启用时自动播放入场动画
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
        originalText = typewriter.GetComponent<TextAnimator_TMP>().text;
    }

    void OnEnable()
    {
        if (typewriter != null)
        {
            typewriter.ShowText(originalText);
        }
    }
}
