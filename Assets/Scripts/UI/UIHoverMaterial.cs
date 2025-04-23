using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIHoverMaterial : MonoBehaviour
{
    [SerializeField] private Material hoverMaterial;
    private Image image;
    private Material originalMaterial;
    private EventTrigger eventTrigger;

    private void Awake()
    {
        image = GetComponent<Image>();
        if (image != null)
        {
            originalMaterial = image.material;
        }

        // 添加或获取EventTrigger组件
        eventTrigger = GetComponent<EventTrigger>();
        if (eventTrigger == null)
        {
            eventTrigger = gameObject.AddComponent<EventTrigger>();
        }

        // 设置PointerEnter事件
        var pointerEnterEntry = new EventTrigger.Entry();
        pointerEnterEntry.eventID = EventTriggerType.PointerEnter;
        pointerEnterEntry.callback.AddListener((data) => { OnPointerEnter(); });
        eventTrigger.triggers.Add(pointerEnterEntry);

        // 设置PointerExit事件
        var pointerExitEntry = new EventTrigger.Entry();
        pointerExitEntry.eventID = EventTriggerType.PointerExit;
        pointerExitEntry.callback.AddListener((data) => { OnPointerExit(); });
        eventTrigger.triggers.Add(pointerExitEntry);
    }

    private void OnDisable()
    {
        // 确保在物体被隐藏时恢复原始材质
        if (image != null)
        {
            image.material = originalMaterial;
        }
    }

    public void OnPointerEnter()
    {
        if (image != null && hoverMaterial != null)
        {
            image.material = hoverMaterial;
        }
    }

    public void OnPointerExit()
    {
        if (image != null)
        {
            image.material = originalMaterial;
        }
    }
} 