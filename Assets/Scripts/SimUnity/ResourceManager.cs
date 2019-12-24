using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SU2.Files.Formats.RCOL;
using FSO.Files.Formats.DBPF;

public class CachedTexture : CachedAsset
{
    public TXTRDataBlock asset = null;

    public CachedTexture(TXTRDataBlock asset, string key)
    {
        this.asset = asset;
        this.key = key;
    }
}

public class CachedModel : CachedAsset
{
    public ModelWrapper asset = null;

    public CachedModel(ModelWrapper asset, string key)
    {
        this.asset = asset;
        this.key = key;
    }
}
public abstract class CachedAsset
{
    public string key;
    public int users = 0;

    public virtual void Dereference()
    {
        users -= 1;
        if (users <= 0)
            OnFree();
    }

    public virtual void Free()
    {
        users = 0;
        OnFree();
    }

    public virtual void OnFree()
    {
        if (ResourceManager.CachedAssetsByName.ContainsKey(key))
            ResourceManager.CachedAssetsByName.Remove(key);
    }
}
public static class ResourceManager
{
    public static Dictionary<string, CachedAsset> CachedAssetsByName = new Dictionary<string, CachedAsset>();
    //public static Dictionary<string, CachedTexture> CachedTexturesByName = new Dictionary<string, CachedTexture>();
    //public static Dictionary<string, CachedModel> CachedModelsByName = new Dictionary<string, CachedModel>();
    public static CachedModel GetModel(string name, bool addUser = true)
    {
        if (CachedAssetsByName.ContainsKey(name))
        {
            var cach = CachedAssetsByName[name];
            if (addUser)
                cach.users += 1;
            return cach as CachedModel;
        }
        else
        {
            var modelget = RCOLFile.GetModel(name);
            var cachmod = new CachedModel(modelget, name);
            modelget.referencedModel = cachmod;
            if (addUser)
                cachmod.users = 1;
            CachedAssetsByName[name] = cachmod;
            return cachmod;
        }
    }
    public static CachedTexture GetTexture(string name, bool addUser = true)
    {
        if (CachedAssetsByName.ContainsKey(name))
        {
            var cach = CachedAssetsByName[name];
            if (addUser)
                cach.users += 1;
            return cach as CachedTexture;
        }
        else
        {
            var reference = new RCOLFile(Environment.GetReference(name));
            var cachmat = new CachedTexture(reference.dataBlocks[0] as TXTRDataBlock, name);
            if (addUser)
                cachmat.users = 1;
            CachedAssetsByName[name] = cachmat;
            return cachmat;
        }
    }
    public static void CleanUp()
    {
        var toDelete = new List<string>();
        foreach(var element in CachedAssetsByName)
        {
            if (element.Value.users <= 0)
            {
                toDelete.Add(element.Key);
            }
        }
        foreach(var element in toDelete)
        {
            CachedAssetsByName.Remove(element);
        }
    }
}
