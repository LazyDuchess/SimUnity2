using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class Config
{
    public string package_dir;
    public string model_dir;
    public string output;
    public int model_id;
    public uint model_instance;
    public uint model_instance2;

    public string game_dir;
    public string user_dir;
    public string su_dir;
    public string lang;
    public bool enable_mods = false;

    public float ui_size;

    //archives is deprecated
    public List<string> archives;
    public List<string> dlc;

    public List<string> archives_main;
    public List<string> archives_nhood;
}
