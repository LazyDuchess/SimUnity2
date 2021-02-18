using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SU2.UI;
using SU2.Utils;
using SU2.Files.Formats.STR;
using System.IO;
using SU2.Files.Formats.RCOL;

public class MainMenu : MonoBehaviour
{
    public static List<int> TopLeftSprite = new List<int>()
    {
        { Hash.TGIRHash(0xCCC30175, 0x00000000, 0x856DDBAC, 0x499DB772) },
        { Hash.TGIRHash(0xCCC30176, 0x00000000, 0x856DDBAC, 0x499DB772) },
        { Hash.TGIRHash(0xCCC30177, 0x00000000, 0x856DDBAC, 0x499DB772) }
    };
    public static List<int> BotRightSprite = new List<int>()
    {
        { Hash.TGIRHash(0xCCC30172, 0x00000000, 0x856DDBAC, 0x499DB772) },
        { Hash.TGIRHash(0xCCC30171, 0x00000000, 0x856DDBAC, 0x499DB772) },
        { Hash.TGIRHash(0xCCC30170, 0x00000000, 0x856DDBAC, 0x499DB772) }
    };
    public static Texture2D[] hoodFrame;
    public List<HoodButton> hoodFrames = new List<HoodButton>();
    public int currentPage = 0;
    public SButton previousHButton;
    public SButton nextHButton;
    public void NextPage()
    {
        currentPage += 3;
        for(var i=0;i<hoodFrames.Count;i++)
        {
            var fram = hoodFrames[i];
            if (i + currentPage >= Environment.hoods.Count)
                fram.SetNeighborhood(null);
            else
                fram.SetNeighborhood(Environment.hoods[i + currentPage]);
        }
        CalculateButtons();
    }
    public void PrevPage()
    {
        currentPage -= 3;
        for (var i = 0; i < hoodFrames.Count; i++)
        {
            var fram = hoodFrames[i];
            if (i + currentPage >= Environment.hoods.Count)
                fram.SetNeighborhood(null);
            else
                fram.SetNeighborhood(Environment.hoods[i + currentPage]);
        }
        CalculateButtons();
    }
    void CalculateButtons()
    {
        nextHButton.unlocked = true;
        previousHButton.unlocked = true;
        var maxPage = (Mathf.CeilToInt(Environment.hoods.Count / 3)-1)*3;
        if (currentPage > maxPage)
            nextHButton.unlocked = false;
        if (currentPage == 0)
            previousHButton.unlocked = false;
    }
    // Start is called before the first frame update
    void Start()
    {
        
        hoodFrame = Environment.LoadAnimatedTexture(Hash.TGIRHash(0xCCC30000, 0x00000000, 0x856DDBAC, 0x499DB772),4);
        var MenuStrings1 = new STRFile(Environment.GetAsset(Hash.TGIRHash(0x00000087, 0x00000000, 0x53545223, Hash.GroupHash("Neighborhood"))));
        var MenuStrings2 = new STRFile(Environment.GetAsset(Hash.TGIRHash(0x00000086, 0x00000000, 0x53545223, Hash.GroupHash("Neighborhood"))));
        Panel.mainPanel = new Panel();
        var menuBackground = Environment.LoadUITexture(Hash.TGIRHash(0xCCC9AF70, 0x00000000, 0x856DDBAC, 0x499DB772));
        //var menuBackground = Environment.LoadUITexture(new SU2.Utils.GroupEntryRef(0xCCC9AF70, 0x00000000, 0x856DDBAC, 0x499DB772));
        Panel.mainPanel.MakePlainBackground(new Color(0.007843138f, 0.1647059f, 0.2784314f));
        Panel.mainPanel.AddImage(menuBackground,Vector2.zero);
        var hoodChooser = Environment.LoadUITexture(Hash.TGIRHash(0xCCC30135, 0x00000000, 0x856DDBAC, 0x499DB772));
        Panel.mainPanel.AddImage(hoodChooser, Vector2.zero);
        Panel.mainPanel.AddImage(Environment.LoadUITexture(Hash.TGIRHash(0xCCC30138, 0x00000000, 0x856DDBAC, 0x499DB772)), new Vector2(-3.5f, -253.5f));
        var tleft = TopLeftSprite[Random.Range(0, TopLeftSprite.Count)];
        var bright = BotRightSprite[Random.Range(0, BotRightSprite.Count)];
        Panel.mainPanel.AddImage(Environment.LoadUITexture(tleft), new Vector2(-208.1f,194.3f));
        Panel.mainPanel.AddImage(Environment.LoadUITexture(bright), new Vector2(213.6f, -125.5f));
        var exitButton = Environment.LoadAnimatedTexture(Hash.TGIRHash(0xCCC30095, 0x00000000, 0x856DDBAC, 0x499DB772),4);
        Panel.mainPanel.AddImage(Environment.LoadUITexture(Hash.TGIRHash(0xCCC29090, 0x00000000, 0x856DDBAC, 0x499DB772)), new Vector2(-5f, -250.5f));
        var exitButt = Panel.mainPanel.AddButton(exitButton, new Vector2(-5f,-250.5f),true,MenuStrings2.GetString(2));
        exitButt.onPress += Environment.QuitGame;
        Panel.mainPanel.AddImage(Environment.LoadUITexture(Hash.TGIRHash(0xCCC30080, 0x00000000, 0x856DDBAC, 0x499DB772)), new Vector2(0f, 226.4f));
        var spinBob = Environment.LoadAnimatedTexture(Hash.TGIRHash(0xCCC30200, 0x00000000, 0x856DDBAC, 0x499DB772));
        Panel.mainPanel.AddImage(spinBob[0], new Vector2(-4.6f, 285f));
        
        //File.WriteAllBytes(Environment.config.output, stre);
        var nhoodText = Panel.mainPanel.AddText(MenuStrings1.GetString(0),new Vector2(0f,112f),16);
        nhoodText.alignment = TextAnchor.MiddleCenter;
        nhoodText.fontSize = 20;
        nhoodText.color = new Color(0.6313726f, 0.6666667f, 0.9529412f);
        var nextButton = Environment.LoadAnimatedTexture(Hash.TGIRHash(0xCCC30115, 0x00000000, 0x856DDBAC, 0x499DB772),4);
        nextHButton = Panel.mainPanel.AddButton(nextButton, new Vector2(288f, 51f), true, "");
        var prevButton = Environment.LoadAnimatedTexture(Hash.TGIRHash(0xCCC30110, 0x00000000, 0x856DDBAC, 0x499DB772),4);
        previousHButton = Panel.mainPanel.AddButton(prevButton, new Vector2(-288f, 51f), true, "");
        nextHButton.onPress += NextPage;
        previousHButton.onPress += PrevPage;
        CalculateButtons();
        var xOff = 175f;
        for (var i = 0; i < 3; i++)
        {
            var hh = Panel.mainPanel.CreateHoodFramePanel(new Vector2(-xOff+(xOff*i),29.3f), Environment.hoods[i]);
            hoodFrames.Add(hh);
        }
        
    }
}
