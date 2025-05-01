using UnityEngine;
using UnityEngine.UI;
using BoomJam2025;

public class ButtonFirstClickComponent : MonoBehaviour
{
    [Header("按钮设置")]
    [Tooltip("按钮的唯一标识符")]
    public string buttonId;
    
    [Header("按钮组设置")]
    [Tooltip("按钮组的唯一标识符")]
    public string groupId;
    
    [Header("首次点击事件")]
    [Tooltip("首次点击时触发的事件")]
    public UnityEngine.Events.UnityEvent onFirstClick;

    private Button button;
    private ButtonFirstClickManager buttonManager;
    private static bool isGroupRegistered = false;

    private void Awake()
    {
        button = GetComponent<Button>();
        if (button == null)
        {
            Debug.LogError($"在 {gameObject.name} 上找不到Button组件！");
            return;
        }

        buttonManager = ButtonFirstClickManager.Instance;
        
        // 只在第一个按钮组件中注册按钮组
        if (!isGroupRegistered)
        {
            buttonManager.RegisterButtonGroup(groupId, OnFirstClick);
            isGroupRegistered = true;
        }
        
        // 将按钮添加到按钮组
        buttonManager.AddButtonToGroup(buttonId, groupId);
        
        // 添加按钮点击事件监听
        button.onClick.AddListener(OnButtonClick);
    }

    private void OnButtonClick()
    {
        buttonManager.CheckFirstClick(buttonId);
    }

    private void OnFirstClick()
    {
        onFirstClick?.Invoke();
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(OnButtonClick);
        }
        
        if (buttonManager != null)
        {
            buttonManager.RemoveButtonFromGroup(buttonId);
            // 只在最后一个按钮组件被销毁时移除回调
            if (isGroupRegistered)
            {
                buttonManager.RemoveButtonGroupCallback(groupId);
                isGroupRegistered = false;
            }
        }
    }
} 