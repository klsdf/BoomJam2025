/****************************************************************************
 * Author: 张嘉阳
 * Date: 2025-04-25
 * Description: VFX管理器，目前仅控制眨眼效果
 ****************************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;

public class VFXManager : MonoBehaviour
{
    ///<summary>
    ///获取眨眼MMF_Player
    ///</summary>
    public MMF_Player blinkPlayerStart;
    public MMF_Player blinkPlayerEnd;

    ///<summary>
    ///眨眼时长
    ///</summary>
    public float blinkDuration = 2f;

    ///<summary>
    ///调用眨眼协程
    ///</summary>
    public void PlayBlink()
    {
        StartCoroutine(BlinkCoroutine());
    }


    //眨眼协程
    public IEnumerator BlinkCoroutine()
    {
        blinkPlayerStart.PlayFeedbacks();

        yield return new WaitForSeconds(blinkDuration);

        blinkPlayerEnd.PlayFeedbacks();

    }
}
