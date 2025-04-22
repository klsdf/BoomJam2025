using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer), typeof(Image))]
public class SequenceAnimation : MonoBehaviour
{
    public enum ComponentType
    {
        SpriteRenderer,
        Image
    }

    [Header("组件设置")]
    [SerializeField] private ComponentType componentType = ComponentType.SpriteRenderer;

    [Header("动画数据")]
    [SerializeField] private SequenceAnimationData animationData; // 序列帧动画数据

    [Header("动画设置")]
    [SerializeField] private float frameRate = 12f; // 每秒播放帧数
    [SerializeField] private bool loop = true; // 是否循环播放
    [SerializeField] private bool playOnAwake = true; // 是否自动播放
    [SerializeField] private float delay = 0f; // 延迟播放时间

    [Header("事件")]
    public UnityEvent onAnimationComplete; // 动画播放完成事件

    private SpriteRenderer spriteRenderer;
    private Image image;
    private int currentFrame = 0;
    private bool isPlaying = false;
    private Coroutine animationCoroutine;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        image = GetComponent<Image>();

        // 根据选择的组件类型禁用不需要的组件
        if (componentType == ComponentType.SpriteRenderer)
        {
            image.enabled = false;
        }
        else
        {
            spriteRenderer.enabled = false;
        }

        if (playOnAwake)
        {
            Play();
        }
    }

    public void Play()
    {
        if (isPlaying || animationData == null) return;
        
        isPlaying = true;
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
        animationCoroutine = StartCoroutine(PlayAnimation());
    }

    public void Stop()
    {
        if (!isPlaying) return;
        
        isPlaying = false;
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }
    }

    public void Pause()
    {
        isPlaying = false;
    }

    public void Resume()
    {
        if (!isPlaying && animationData != null)
        {
            isPlaying = true;
            animationCoroutine = StartCoroutine(PlayAnimation());
        }
    }

    private IEnumerator PlayAnimation()
    {
        if (delay > 0)
        {
            yield return new WaitForSeconds(delay);
        }

        while (isPlaying)
        {
            if (animationData == null || animationData.sprites == null || animationData.sprites.Length == 0)
            {
                Debug.LogWarning("序列帧动画数据为空！");
                yield break;
            }

            if (componentType == ComponentType.SpriteRenderer)
            {
                spriteRenderer.sprite = animationData.sprites[currentFrame];
            }
            else
            {
                image.sprite = animationData.sprites[currentFrame];
            }

            currentFrame++;

            if (currentFrame >= animationData.sprites.Length)
            {
                if (loop)
                {
                    currentFrame = 0;
                }
                else
                {
                    isPlaying = false;
                    onAnimationComplete?.Invoke();
                    yield break;
                }
            }

            yield return new WaitForSeconds(1f / frameRate);
        }
    }

    // 设置动画数据
    public void SetAnimationData(SequenceAnimationData newAnimationData)
    {
        animationData = newAnimationData;
        currentFrame = 0;
    }

    // 设置帧率
    public void SetFrameRate(float newFrameRate)
    {
        frameRate = newFrameRate;
    }

    // 设置是否循环
    public void SetLoop(bool newLoop)
    {
        loop = newLoop;
    }

    // 设置组件类型
    public void SetComponentType(ComponentType type)
    {
        if (componentType != type)
        {
            componentType = type;
            
            // 切换组件状态
            if (componentType == ComponentType.SpriteRenderer)
            {
                spriteRenderer.enabled = true;
                image.enabled = false;
            }
            else
            {
                spriteRenderer.enabled = false;
                image.enabled = true;
            }
        }
    }
} 