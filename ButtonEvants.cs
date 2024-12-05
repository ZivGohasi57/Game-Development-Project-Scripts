using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonEvents : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public System.Action onHover;
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (onHover != null)
        {
            onHover.Invoke();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
     
    }
}
