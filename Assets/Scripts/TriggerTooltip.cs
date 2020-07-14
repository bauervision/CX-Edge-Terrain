using UnityEngine;
using UnityEngine.EventSystems;
public class TriggerTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    public string myType;


    public void OnPointerEnter(PointerEventData eventData)
    {
        Tooltip.ShowToolTip(myType);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Tooltip.HideToolTip();
    }
}