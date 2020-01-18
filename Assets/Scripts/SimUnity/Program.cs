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
        Environment.InitInit();
        Environment.LoadArchives(Environment.config.archives_startup);
        var ob = new GameObject("Background Music");
        GameObject.DontDestroyOnLoad(ob);
        Environment.bgMusic = ob.AddComponent<Music>();
        Environment.bgMusic.Play();
        StartCoroutine(Environment.Init());
        
        var startupLoadPanel = Panel.StartupLoading();
        while (!Environment.LoadFinished)
            yield return null;
        var hoods_folder = Path.Combine(Environment.config.user_dir, "Neighborhoods");
        DirectoryInfo hoodInfo = new DirectoryInfo(hoods_folder);
        foreach (var dire in hoodInfo.GetDirectories())
        {
            var hd = new Neighborhood(dire.FullName);
            Environment.hoods.Add(hd);
        }
        startupLoadPanel.Delete();
        var menu = new GameObject();
        menu.AddComponent<MainMenu>();
    }
}
