/****************************************************************************
 * Author: 张嘉阳
 * Date: 2025-04-28
 * Description: 故障效果管理器，用于处理UI元素的故障效果
 * 
 * 使用说明：
 * 1. 功能说明：
 *    - 管理UI元素的故障效果
 *    - 提供统一的接口供其他组件调用
 *    - 包含调试功能
 * 
 * 2. 调试功能：
 *    - 使用调试按钮控制效果：
 *      * TinyGlitch：触发小型故障效果
 *      * Appear：触发出现效果
 *      * Disappear：触发消失效果
 * 
 * 3. 公共接口：
 *    - TinyGlitch()：触发小型故障效果
 *    - Appear()：触发出现效果
 *    - Disappear()：触发消失效果
 *    - UpdateDissolve()：动态修改消散度
 *    - UpdateStrenghth()：动态修改强度

 ****************************************************************************/
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[DisallowMultipleComponent]
[RequireComponent(typeof(Image))]
public class GlitchVFX : MonoBehaviour
{
    #region Inspector Settings
    [Header("Shader 设置")]
    [Tooltip("Shader 名称（在 ShaderGraph 里设置的名称）")]
    [SerializeField] private string dissolveShaderName = "Shader Graphs/Glitch";

    [Header("基础参数")]
    [SerializeField, Range(0f, 1f)] private float dissolveAmount = 0f;
    [SerializeField, Range(0f, 1f)] private float strenghth = 0f;
    [SerializeField] private float tiling = 16f;
    [SerializeField] private float speed = 1f;
    [SerializeField, Range(0f, 1f)] private float borderFade = 0.149f;
    [SerializeField, Range(0f, 1f)] private float range = 0.784f;

    [Header("TinyGlitch 配置")]
    [SerializeField, Range(0f, 1f)] private float tinyGlitchDissolve = 0.066f;
    [SerializeField, Range(0f, 1f)] private float tinyGlitchStrenghth = 0.009f;
    [SerializeField] private float tinyGlitchDuration = 1f;

    [Header("Appear 配置")]
    [SerializeField, Range(0f, 1f)] private float appearStartDissolve = 0.641f;
    [SerializeField, Range(0f, 1f)] private float appearStartStrenghth = 0.2f;
    [SerializeField] private float appearHoldTime = 0.5f;
    [SerializeField] private float appearFadeTime = 0.2f;

    [Header("Disappear 配置")]
    [SerializeField, Range(0f, 1f)] private float disappearEndDissolve = 1f;
    [SerializeField, Range(0f, 1f)] private float disappearEndStrenghth = 0.133f;
    [SerializeField] private float disappearFadeTime = 0.7f;
    [SerializeField] private float disappearHoldTime = 0f;
    #endregion

    #region Debug Methods
    [ContextMenu("▶ TinyGlitch")]
    public void DebugTinyGlitch() => StartCoroutine(TinyGlitch());

    [ContextMenu("▶ Appear")]
    public void DebugAppear() => StartCoroutine(Appear());

    [ContextMenu("▶ Disappear")]
    public void DebugDisappear() => StartCoroutine(Disappear());
    #endregion

    #region Private Members
    private Image _image;
    private Material _instancedMat;
    private static readonly int k_DissolveID = Shader.PropertyToID("_DissolveAmount");
    private static readonly int k_StrenghthID = Shader.PropertyToID("_Strenghth");
    private static readonly int k_TilingID = Shader.PropertyToID("_Tiling");
    private static readonly int k_SpeedID = Shader.PropertyToID("_Speed");
    private static readonly int k_BorderFadeID = Shader.PropertyToID("_BorderFade");
    private static readonly int k_RangeID = Shader.PropertyToID("_Range");
    private static readonly int k_BaseTexID = Shader.PropertyToID("_BaseMap");
    #endregion

    #region Unity Methods
    private void Awake()
    {
        _image = GetComponent<Image>();
        var shader = Shader.Find(dissolveShaderName);
        if (shader == null)
        {
            Debug.LogError($"[{name}] 找不到 Shader：{dissolveShaderName}");
            enabled = false;
            return;
        }
        _instancedMat = new Material(shader);
        if (_image.sprite != null)
            _instancedMat.SetTexture(k_BaseTexID, _image.sprite.texture);
        _image.material = _instancedMat;
        UpdateAllShaderParams();
    }

    private void OnValidate() => UpdateAllShaderParams();
    #endregion

    #region Public Methods
    /// <summary>
    /// 动态修改消散度
    /// </summary>
    /// <param name="amount">消散度值（0-1）</param>
    public void UpdateDissolve(float amount)
    {
        dissolveAmount = Mathf.Clamp01(amount);
        if (_instancedMat != null)
            _instancedMat.SetFloat(k_DissolveID, dissolveAmount);
    }

    /// <summary>
    /// 动态修改强度
    /// </summary>
    /// <param name="value">强度值（0-1）</param>
    public void UpdateStrenghth(float value)
    {
        strenghth = Mathf.Clamp01(value);
        if (_instancedMat != null)
            _instancedMat.SetFloat(k_StrenghthID, strenghth);
    }
    #endregion

    #region Private Methods
    private void UpdateAllShaderParams()
    {
        if (_instancedMat == null) return;
        _instancedMat.SetFloat(k_DissolveID, dissolveAmount);
        _instancedMat.SetFloat(k_StrenghthID, strenghth);
        _instancedMat.SetFloat(k_TilingID, tiling);
        _instancedMat.SetFloat(k_SpeedID, speed);
        _instancedMat.SetFloat(k_BorderFadeID, borderFade);
        _instancedMat.SetFloat(k_RangeID, range);
    }
    #endregion

    #region Animation Coroutines
    public IEnumerator TinyGlitch()
    {
        float oldDissolve = dissolveAmount;
        float oldStrenghth = strenghth;
        UpdateDissolve(tinyGlitchDissolve);
        UpdateStrenghth(tinyGlitchStrenghth);
        yield return new WaitForSeconds(tinyGlitchDuration);
        UpdateDissolve(oldDissolve);
        UpdateStrenghth(oldStrenghth);
    }

    public IEnumerator Appear()
    {
        // 阶段1：高消散度/强度
        UpdateDissolve(appearStartDissolve);
        UpdateStrenghth(appearStartStrenghth);
        yield return new WaitForSeconds(appearHoldTime);

        // 阶段2：逐渐归0
        float t = 0;
        float startD = appearStartDissolve, startS = appearStartStrenghth;
        while (t < appearFadeTime)
        {
            float p = t / appearFadeTime;
            UpdateDissolve(Mathf.Lerp(startD, 0f, p));
            UpdateStrenghth(Mathf.Lerp(startS, 0f, p));
            t += Time.deltaTime;
            yield return null;
        }
        UpdateDissolve(0f);
        UpdateStrenghth(0f);
    }

    public IEnumerator Disappear()
    {
        // 阶段1：逐渐升高
        float t = 0;
        float startD = 0f, startS = 0f;
        while (t < disappearFadeTime)
        {
            float p = t / disappearFadeTime;
            UpdateDissolve(Mathf.Lerp(startD, disappearEndDissolve, p));
            UpdateStrenghth(Mathf.Lerp(startS, disappearEndStrenghth, p));
            t += Time.deltaTime;
            yield return null;
        }
        UpdateDissolve(disappearEndDissolve);
        UpdateStrenghth(disappearEndStrenghth);

        // 阶段2：保持
        yield return new WaitForSeconds(disappearHoldTime);
    }
    #endregion
}
