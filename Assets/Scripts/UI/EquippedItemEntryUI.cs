using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class EquippedItemEntryUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image iconImage;

    private RogueliteItem item;
    private EquippedItemUI owner;

    public void Initialize(RogueliteItem itemData, EquippedItemUI ownerUi)
    {
        item = itemData;
        owner = ownerUi;

        if (iconImage == null)
        {
            iconImage = GetComponent<Image>();
        }

        if (iconImage != null)
        {
            iconImage.sprite = item != null ? item.icon : null;
            iconImage.enabled = iconImage.sprite != null;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (owner != null && item != null)
        {
            owner.ShowTooltip(item);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (owner != null)
        {
            owner.HideTooltip();
        }
    }
}
