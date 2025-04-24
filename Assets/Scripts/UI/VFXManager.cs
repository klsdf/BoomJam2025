/****************************************************************************
 * Author: 张嘉阳
 * Date: 2025-04-25
 * Description: VFX管理器
 ****************************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;

public class VFXManager : MonoBehaviour
{
    ///<summary>
    ///获取眨眼的MMF_Player
    ///</summary>
    public MMF_Player blinkPlayerStart;
    public MMF_Player blinkPlayerEnd;

    ///<summary>
    ///设置眨眼时长
    ///</summary>
    public float blinkDuration = 2f;

    ///<summary>
    ///播放眨眼特效
    ///</summary>
    public void PlayBlink()
    {
        StartCoroutine(BlinkCoroutine());
    }


    //协程播放眨眼特效
    public IEnumerator BlinkCoroutine()
    {
        blinkPlayerStart.PlayFeedbacks();

        yield return new WaitForSeconds(blinkDuration);

        blinkPlayerEnd.PlayFeedbacks();

    }
}
