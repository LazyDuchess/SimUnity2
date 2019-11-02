using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SU2.UI;

public class Program : MonoBehaviour
{
    void Start()
    {
        Environment.Init();
        var menu = new GameObject();
        menu.AddComponent<MainMenu>();
        
    }
}
