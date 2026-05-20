using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Lightweight helper you can attach to the UI icon to forward pointer enter/exit
/// to the EquippedItemUI component in the scene.
/// </summary>
public class ItemTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public EquippedItemUI uiOwner;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (uiOwner != null) uiOwner.OnPointerEnterIcon();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (uiOwner != null) uiOwner.OnPointerExitIcon();
    }
}
