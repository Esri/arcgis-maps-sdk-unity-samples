using UnityEngine;
using UnityEngine.EventSystems;

public class OnButtonDown : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [SerializeField] private bool isLocationButton;
    [SerializeField] private bool isWristButton;
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
        if (!isLocationButton && !isWristButton)
        {
            if (pressed)
            {
                if (Type == type.ZoomIn)
                {
                    tableTopInteractor.ZoomInMap();
                }
                else if (Type == type.ZoomOut)
                {
                    tableTopInteractor.ZoomOutMap();
                }
            }
        }
    }
}
