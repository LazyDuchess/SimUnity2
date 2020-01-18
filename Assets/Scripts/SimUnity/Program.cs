using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SU2.UI;
using System.IO;

public class Program : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(Init());
    }
    IEnumerator Init()
    {
        //Initialize Initialization
        Environment.InitInit();
        //Load startup files
        Environment.LoadArchives(Environment.config.archives_startup);
        //music
        var ob = new GameObject("Background Music");
        GameObject.DontDestroyOnLoad(ob);
        Environment.bgMusic = ob.AddComponent<Music>();
        Environment.bgMusic.Play();
        //Load mods and main archives
        StartCoroutine(Environment.Init());
        //Startup loading screen
        var startupLoadPanel = Panel.StartupLoading();
        
        while (!Environment.LoadFinished) // Only do the following things after main archives are loaded
            yield return null;

        var hoods_folder = Path.Combine(Environment.config.user_dir, "Neighborhoods"); //Neighborhoods folder
        DirectoryInfo hoodInfo = new DirectoryInfo(hoods_folder); 
        foreach (var dire in hoodInfo.GetDirectories())
        {
            var hd = new Neighborhood(dire.FullName);
            Environment.hoods.Add(hd);   //Load hoods
        }
        startupLoadPanel.Delete(); //Remove loading panel since it's done
        var menu = new GameObject();
        menu.AddComponent<MainMenu>(); //Start main menu!
    }
}
