using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SU2.UI;
public class HoodButton : MonoBehaviour
{
    public Neighborhood currentHood;
    GameObject children;
    RawImage picture;
    public Panel myPanel;
    public Text hoodName;
    //RectTransform trans;
    private void Start()
    {
        var picObj = myPanel.AddImage(currentHood.thumbnail, new Vector2(transform.position.x, transform.position.y));
        picObj.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
        var frameObj = myPanel.AddButton(MainMenu.hoodFrame, new Vector2(transform.position.x,transform.position.y), true);
        hoodName = myPanel.AddText(currentHood.name, new Vector2(transform.localPosition.x, transform.localPosition.y-115f), 16);
        hoodName.alignment = TextAnchor.UpperCenter;
        children = new GameObject();
        children.transform.SetParent(transform);
        children.transform.localPosition = Vector3.zero;
        picObj.transform.SetParent(children.transform);
        frameObj.transform.SetParent(children.transform);
        picObj.transform.localPosition = Vector3.zero;
        frameObj.transform.localPosition = Vector3.zero;
        picture = picObj.GetComponent<RawImage>();
        hoodName.transform.SetParent(children.transform);
        frameObj.onPress = GoToHood;
    }
    void GoToHood()
    {
        var startupLoadPanel = Panel.StartupLoading();
        StartCoroutine(Environment.GoToHood(currentHood));
        //Environment.GoToHood(currentHood);
    }
    public void SetNeighborhood(Neighborhood hood)
    {
        if (hood == null)
            children.SetActive(false);
        else
        {
            currentHood = hood;
            picture.texture = hood.thumbnail;
            hoodName.text = hood.name;
            children.SetActive(true);
        }
    }
}
