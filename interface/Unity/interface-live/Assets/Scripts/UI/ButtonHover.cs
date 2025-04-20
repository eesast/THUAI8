using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [TextArea]
    public string hoverText;

    public void OnPointerEnter(PointerEventData eventData)
    {
        ToolTipController.Instance.ShowTooltip(hoverText, Input.mousePosition);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ToolTipController.Instance.HideTooltip();
    }
}
