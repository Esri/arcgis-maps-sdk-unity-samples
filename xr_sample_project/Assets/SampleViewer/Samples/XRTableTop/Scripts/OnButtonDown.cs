using UnityEngine;
using UnityEngine.EventSystems;

public class OnButtonDown : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    bool pressed;

    public enum type
    {
        ZoomIn,
        ZoomOut
    }

    public XRTableTopInteractor tableTopInteractor;
    public type Type;

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        pressed = true;
    }

    void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
    {
        pressed = false;
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData) 
    {
        pressed = false;
    }

    private void Update()
    {
        if (pressed)
        {
            if (Type == type.ZoomIn)
            {
                tableTopInteractor.ZoomMap(1.0f);
            }
            else if (Type == type.ZoomOut)
            {
                tableTopInteractor.ZoomMap(-1.0f);
            }
        }
    }
}
