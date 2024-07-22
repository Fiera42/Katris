using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WheelButtons : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public bool isHover { get; private set; }

    public Button moveButton;
    public Button patrolButton;
    public Button scoutButton;
    public Button attackButton;
    public Button centerCameraButton;

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        isHover = true;
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        isHover = false;
    }

    private void OnDisable()
    {
        isHover = false;
    }
}
