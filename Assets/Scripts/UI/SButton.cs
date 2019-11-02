using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using SU2.UI;

public class SButton : UIElement
    , IPointerDownHandler
     , IPointerEnterHandler
     , IPointerExitHandler
    , IPointerUpHandler
{
    public Texture2D[] images;
    RawImage raw;
    public bool unlocked = true;
    bool hover = false;
    bool pressing = false;
    public delegate void PressButton();
    public PressButton onPress;
    public string tooltip = "";
    GameObject TooltipObject;

    // Start is called before the first frame update
    void Start()
    {
        raw = GetComponent<RawImage>();
        if (tooltip != "")
        {
            var tex = panel.AddText(tooltip, GetComponent<RectTransform>().localPosition+(Vector3.up*5f), 12);
            TooltipObject = tex.gameObject;
            tex.raycastTarget = false;
            tex.alignment = TextAnchor.MiddleCenter;
            tex.color = Color.black;
            TooltipObject.SetActive(false);
        }
    }
    void ToolTipShow()
    {
        TooltipObject.SetActive(true);
    }
    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        hover = true;
        if (tooltip != "")
            Invoke("ToolTipShow", Panel.tooltipTime);
    }
    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        hover = false;
        if (tooltip != "")
            TooltipObject.SetActive(false);
        CancelInvoke();
    }
    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        pressing = true;
    }
    void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
    {
        pressing = false;
        if (unlocked && hover && onPress != null)
            onPress();
    }
    // Update is called once per frame
    void Update()
    {
        if (!unlocked)
        {
            raw.texture = images[0];
            return;
        }
        raw.texture = images[1];
        if (hover)
            raw.texture = images[3];
        if (pressing)
            raw.texture = images[2];
    }
}
