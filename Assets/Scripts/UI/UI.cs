using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SU2.UI
{
    public class UIElement : MonoBehaviour
    {
        public Panel panel;
    }
    public class Panel
    {
        public static float tooltipTime = 0.5f;
        public static Font defaultFont = Resources.Load<Font>("Roboto-Regular");
        public static Panel mainPanel;
        public Canvas canvas;
        public HoodButton CreateHoodFramePanel(Vector2 pos, Neighborhood hood)
        {
            var ob = new GameObject();
            ob.transform.SetParent(canvas.transform);
            ob.transform.localPosition = new Vector3(pos.x, pos.y, 0f);
            var comp = ob.AddComponent<HoodButton>();
            comp.currentHood = hood;
            comp.myPanel = this;
            return comp;
        }
        public Panel()
        {
            var ob = new GameObject();
            canvas = ob.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.scaleFactor = Environment.config.ui_size;
            canvas.pixelPerfect = true;
            ob.AddComponent<CanvasScaler>();
            ob.AddComponent<GraphicRaycaster>();
        }
        public SButton AddButton(Texture2D[] images, Vector2 pos, bool enabled = true, string tooltip = "")
        {
            var ob = AddImage(images[1], pos);
            var butt = ob.AddComponent<SButton>();
            butt.images = images;
            butt.panel = this;
            butt.unlocked = enabled;
            butt.tooltip = tooltip;
            return butt;
        }
        public Text AddText(string text, Vector2 pos, int size)
        {
            canvas.scaleFactor = 1f;
            var ob = new GameObject();
            ob.AddComponent<CanvasRenderer>();
            var tst = ob.AddComponent<Text>();
            ob.transform.SetParent(canvas.transform);
            ob.GetComponent<RectTransform>().localPosition = new Vector3(pos.x, pos.y, 0f);
            tst.fontSize = size;
            tst.text = text;
            tst.font = defaultFont;
            tst.horizontalOverflow = HorizontalWrapMode.Overflow;
            tst.verticalOverflow = VerticalWrapMode.Overflow;
            canvas.scaleFactor = Environment.config.ui_size;
            return tst;
        }
        public LoopingImage AddLoopingImage(Texture2D[] images, Vector2 pos, float speed)
        {
            var bgObject = new GameObject();
            bgObject.AddComponent<CanvasRenderer>();
            var raw = bgObject.AddComponent<RawImage>();
            raw.texture = images[0];
            bgObject.transform.SetParent(canvas.transform);
            raw.SetNativeSize();
            bgObject.GetComponent<RectTransform>().localPosition = new Vector3(pos.x, pos.y, 0f);
            var loop = bgObject.AddComponent<LoopingImage>();
            loop.speed = speed;
            loop.images = images;
            loop.raw = raw;
            return loop;
        }
        public GameObject AddImage(Texture2D image, Vector2 pos)
        {
            canvas.scaleFactor = 1f;
            var bgObject = new GameObject();
            bgObject.AddComponent<CanvasRenderer>();
            var raw = bgObject.AddComponent<RawImage>();
            raw.texture = image;
            bgObject.transform.SetParent(canvas.transform);
            raw.SetNativeSize();
            bgObject.GetComponent<RectTransform>().localPosition = new Vector3(pos.x, pos.y, 0f);
            canvas.scaleFactor = Environment.config.ui_size;
            return bgObject;
        }
        public GameObject MakePlainBackground(Color color)
        {
            var bgObject = new GameObject();
            bgObject.AddComponent<CanvasRenderer>();
            var raw = bgObject.AddComponent<RawImage>();
            bgObject.transform.SetParent(canvas.transform);
            raw.color = color;
            bgObject.GetComponent<RectTransform>().anchorMax = Vector2.one;
            bgObject.GetComponent<RectTransform>().anchorMin = Vector2.zero;
            bgObject.GetComponent<RectTransform>().localPosition = Vector3.zero;
            bgObject.GetComponent<RectTransform>().localScale = Vector3.one;
            //raw.SetNativeSize();
            return bgObject;
        }
    }
}
