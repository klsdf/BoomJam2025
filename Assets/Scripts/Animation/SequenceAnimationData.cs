using UnityEngine;

[CreateAssetMenu(fileName = "NewSequenceAnimation", menuName = "动画/序列帧动画")]
public class SequenceAnimationData : ScriptableObject
{
    [Tooltip("序列帧数组")]
    public Sprite[] sprites;
}