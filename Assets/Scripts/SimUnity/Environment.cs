﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using FSO.Files.Formats.DBPF;
using SU2.Utils;
using System;
using System.Text;
using UnityEngine.SceneManagement;
using FSO.Files.XA;
using SU2.Files.Formats.CPF;
using SU2.Files.Formats.RCOL;
using System.Threading;

public static class Environment
{
    public static bool LoadFinished = false;
    public static Material defaultMaterial = Resources.Load<Material>("DefaultMaterial");
    public static Neighborhood currentNhood = null;
    public static List<Neighborhood> hoods = new List<Neighborhood>();
    public static byte language = 1;
    public static Music bgMusic;
    public static Config config;
    public static DBPFFile UIPackage;
    public static Dictionary<int, DBPFEntry> entryByID = new Dictionary<int, DBPFEntry>();
    public static Dictionary<string, DBPFEntry> entryByName = new Dictionary<string, DBPFEntry>();
    public static Dictionary<int, DBPFEntry> entryByFullID = new Dictionary<int, DBPFEntry>();
    public static Dictionary<uint, List<DBPFEntry>> entryByType = new Dictionary<uint, List<DBPFEntry>>();
    public static List<NHoodDecorationDescription> hoodDeco = new List<NHoodDecorationDescription>();
    public static Dictionary<uint, NHoodDecorationDescription> hoodDecoByGUID = new Dictionary<uint, NHoodDecorationDescription>();
    public static Dictionary<string, byte> LanguageNames = new Dictionary<string, byte>()
    {
        { "English", 1},
        { "English (UK)", 2},
        { "French", 3 },
        { "German", 4},
        { "Italian", 5},
        { "Spanish", 6},
        { "Dutch", 7},
        { "Danish", 8},
        { "Swedish", 9},
        { "Norwegian", 10},
        { "Finnish", 11},
        { "Hebrew", 12},
        { "Russian", 13},
        { "Portuguese", 14},
        { "Japanese", 15},
        { "Polish", 16},
        { "Traditional Chinese", 17},
        { "Simplified Chinese", 18},
        { "Thai", 19},
        { "Korean", 20},
        { "Czech", 26}
    };
    public static void InitInit()
    {
        var SplashStation = new Station("", true);
        SplashStation.songs.Add(new Song(Hash.TGIRHash(0xFFA87950, 0x5BA1DCC7, 0x2026960B, 0xFF606C7A)));

        var NHoodStation = new Station("nhood", false);
        //BG
        NHoodStation.songs.Add(new Song(Hash.TGIRHash(0xFFA78F62, 0x09CCDF11, 0x2026960B, 0xFFC4D24B)));
        //Uni
        NHoodStation.songs.Add(new Song(Hash.TGIRHash(0xFF347A54, 0x22ED6EBC, 0x2026960B, 0xFFC4D24B)));
        //NightLife
        NHoodStation.songs.Add(new Song(Hash.TGIRHash(0xFF60DF7E, 0xF9FCFBD1, 0x2026960B, 0xFFC4D24B)));
        NHoodStation.songs.Add(new Song(Hash.TGIRHash(0xFF6B1F36, 0x63048DC1, 0x2026960B, 0xFFC4D24B)));
        NHoodStation.songs.Add(new Song(Hash.TGIRHash(0xFFA059A0, 0x045D17A7, 0x2026960B, 0xFFC4D24B)));
        //OFB
        NHoodStation.songs.Add(new Song(Hash.TGIRHash(0xFFF7762C, 0x41471062, 0x2026960B, 0xFFC4D24B)));

        Music.soundtrack.Add(SplashStation);
        Music.soundtrack.Add(NHoodStation);


        var dir = new DirectoryInfo(Application.dataPath).Parent.FullName;
        config = JsonUtility.FromJson<Config>(File.ReadAllText(Path.Combine(dir, "config.json")));
        if (LanguageNames.ContainsKey(config.lang))
            language = LanguageNames[config.lang];
    }
    public static IEnumerator Init()
    {
        LoadFinished = false;
        Thread thread = new Thread(new ThreadStart(ThreadedInit));
        thread.Start();
        while (!LoadFinished)
            yield return null;
        /*
        var hoods_folder = Path.Combine(config.user_dir, "Neighborhoods");
        DirectoryInfo hoodInfo = new DirectoryInfo(hoods_folder);
        foreach (var dire in hoodInfo.GetDirectories())
        {
            var hd = new Neighborhood(dire.FullName);
            hoods.Add(hd);
        }*/
    }

    public static List<KeyValuePair<uint, byte[]>> GetItemsByType(uint Type)
    {

        var result = new List<KeyValuePair<uint, byte[]>>();
        if (!entryByType.ContainsKey(Type))
            return null;
        var entries = entryByType[Type];
        for (int i = 0; i < entries.Count; i++)
        {
            result.Add(new KeyValuePair<uint, byte[]>(entries[i].InstanceID, entries[i].file.GetEntry(entries[i])));
        }
        return result;
    }
    public static void ThreadedLoadNHoodData()
    {
        LoadArchives(config.archives_nhood);
        var hoodDecos = GetItemsByType(0x6D619378);
        foreach (var element in hoodDecos)
        {
            var decoDesc = new NHoodDecorationDescription();
            var cpfFile = new CPFFile(element.Value);
            decoDesc.name = cpfFile.GetString("name");
            decoDesc.modelName = cpfFile.GetString("modelname");
            var guid = cpfFile.GetUInt("guid");
            decoDesc.guid = guid;
            hoodDeco.Add(decoDesc);
            hoodDecoByGUID[guid] = decoDesc;
        }
        LoadFinished = true;
    }
    public static void LoadNHoodData()
    {
        LoadFinished = false;
        Thread thread = new Thread(new ThreadStart(ThreadedLoadNHoodData));
        thread.Start();
    }
    public static IEnumerator GoToHood(Neighborhood hood)
    {
        LoadNHoodData();
        bgMusic.SetStation(MusicCategory.Nhood);
        while (!LoadFinished)
            yield return null;
        currentNhood = hood;
        currentNhood.InitializeNHoodData();
        SceneManager.LoadScene(1);
    }
    public static void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    public static XAFile GetAudio(byte[] audio)
    {
        var xafil = new XAFile(audio);
        if (xafil.XA)
            xafil.DecompressFile();
        return xafil;
    }
    public static void LoadArchives(List<string> archives, bool setLoadFinishedToTrue = false)
    {
        foreach (var element1 in config.dlc)
        {
            foreach (var element in archives)
            {
                var fil = Path.Combine(config.game_dir, element1, element);
                if (File.Exists(fil))
                {
                    DBPFFile.LoadResource(fil);
                }
                else
                {
                    if (Directory.Exists(fil))
                    {
                        DirectoryInfo folderInfo = new DirectoryInfo(fil);
                        foreach (var file in folderInfo.GetFiles("*.package"))
                        {
                            DBPFFile.LoadResource(file.FullName);
                        }
                    }
                }
            }
        }
        if (setLoadFinishedToTrue)
            LoadFinished = true;
    }
    public static void ThreadedInit()
    {
        if (config.enable_mods)
        {
            var mods_folder = Path.Combine(config.user_dir, "Downloads");
            if (Directory.Exists(mods_folder))
            {
                DirectoryInfo modInfo = new DirectoryInfo(mods_folder);
                foreach (var file in modInfo.GetFiles("*.package",SearchOption.AllDirectories))
                {
                    DBPFFile.LoadResource(file.FullName, true);
                }
            }
        }
        LoadArchives(config.archives_main);
        LoadFinished = true;
    }
    

    public static DBPFReference GetReference(string name)
    {
        if (entryByName.ContainsKey(name))
        {
            var entr = entryByName[name];
            return new DBPFReference(entr.file.GetItemByName(name),entr.file);
        }
        return null;
    }

    public static DBPFReference GetReference(int tgir)
    {
        if (entryByFullID.ContainsKey(tgir))
        {
            var entr = entryByFullID[tgir];
            return new DBPFReference(entr.file.GetEntry(entr),entr.file);
        }
        return null;
    }

    public static DBPFReference GetReferenceTGI(int tgi)
    {
        if (entryByID.ContainsKey(tgi))
        {
            var entr = entryByFullID[tgi];
            return new DBPFReference(entr.file.GetEntry(entr),entr.file);
        }
        return null;
    }

    public static byte[] GetAsset(string name)
    {
        if (entryByName.ContainsKey(name))
        {
            var entr = entryByName[name];
            return entr.file.GetItemByName(name);
        }
        return null;
    }

    public static byte[] GetAsset(int tgir)
    {
        if (entryByFullID.ContainsKey(tgir))
        {
            var entr = entryByFullID[tgir];
            return entr.file.GetEntry(entr);
        }
        return null;
    }

    public static byte[] GetAssetTGI(int tgi)
    {
        if (entryByID.ContainsKey(tgi))
        {
            var entr = entryByID[tgi];
            return entr.file.GetEntry(entr);
        }
        return null;
    }

    public static DBPFEntry GetEntry(int tgir)
    {
        if (entryByFullID.ContainsKey(tgir))
        {
            return entryByFullID[tgir];
        }
        return null;
    }
    public static DBPFEntry GetEntryTGI(int tgi)
    {
        if (entryByID.ContainsKey(tgi))
        {
            return entryByID[tgi];
        }
        return null;
    }
    
    /*
    public static byte[] GetAsset(GroupEntryRef refe)
    {
        if (entryByFullID.ContainsKey(refe))
        {
            return entryByFullID[refe].file.GetEntry(entryByFullID[refe]);
        }
        return null;
    }*/
    public static string GetPackage(string packageName)
    {
        return Path.Combine(config.game_dir, "Double Deluxe/Base/TSData/Res", packageName);
    }
    public static Texture2D[] LoadAnimatedTexture(int tgir, int frames = 0)
    {
        var tex = LoadUITexture(tgir);
        var amount = tex.width / tex.height;
        if (frames != 0)
            amount = frames;
        var width = tex.width / amount;
        var pp = new Texture2D[amount];
        for(var i=0;i<amount;i++)
        {
            pp[i] = new Texture2D(width, tex.height, TextureFormat.ARGB32, false, false);
            for(var n=0;n<width;n++)
            {
                for(var j=0;j<tex.height;j++)
                {
                    pp[i].SetPixel(n, j, tex.GetPixel(n + (width * i), j));
                }
            }
            pp[i].Apply();
            pp[i].filterMode = FilterMode.Point;
        }
        return pp;
    }
    public static Texture2D LoadUITexture(int tgir)
    {
        var file = GetAsset(tgir);
        return LoadUITexture(file);
    }
    /*
    public static Texture2D LoadUITexture(int tgir)
    {
        var file = GetAsset(tgir);
        return LoadUITexture(file);
    }*/
    public static Texture2D LoadTexture(byte[] file, bool mips)
    {
        var pngCheck = Encoding.UTF8.GetString(file, 1, 3);
        var jpgCheck = Encoding.UTF8.GetString(file, 6, 4);
        if (pngCheck != "PNG" && jpgCheck != "JFIF")
        {
            var tex = new Paloma.TargaImage(file).bmpTargaImage;
            tex.filterMode = FilterMode.Point;
            return tex;
        }
        Texture2D fTex = new Texture2D(1, 1, TextureFormat.ARGB32, mips);
        fTex.filterMode = FilterMode.Point;
        fTex.LoadImage(file);
        return fTex;
    }
    public static Texture2D LoadUITexture(byte[] file)
    {
        var uiTex = LoadTexture(file, false);
        uiTex.filterMode = FilterMode.Point;
        //var result = uiTex.LoadImage(file);
        return uiTex;
    }
}
