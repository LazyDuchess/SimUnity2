using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FSO.Files.Formats.DBPF;
using System.IO;
using SU2.Files.Formats.STR;
using SU2.Utils;
using SU2.Files.Formats.RCOL;

public class TerrainType
{
    public static Material TerrainMaterial = Resources.Load<Material>("TerrainMaterial");
    public static Material WaterMaterial = Resources.Load<Material>("WaterMaterial");
    string terrainTexture;
    string shoreTexture;
    string cliffTexture;
    string roughnessTexture;
    public TXTRDataBlock terrainTexture2D;
    public TXTRDataBlock shoreTexture2D;
    public TXTRDataBlock cliffTexture2D;
    public TXTRDataBlock roughnessTexture2D;
    public Material terrainMaterial;

    public TerrainType(string terrainTexture, string shoreTexture, string cliffTexture, string roughnessTexture)
    {
        this.terrainTexture = terrainTexture;
        this.shoreTexture = shoreTexture;
        this.cliffTexture = cliffTexture;
        this.roughnessTexture = roughnessTexture;
    }
    public void Load()
    {
        if (terrainTexture2D != null)
            return;
        terrainTexture2D = new RCOLFile(Environment.GetReference(terrainTexture)).dataBlocks[0] as TXTRDataBlock;
        shoreTexture2D = new RCOLFile(Environment.GetReference(shoreTexture)).dataBlocks[0] as TXTRDataBlock;
        cliffTexture2D = new RCOLFile(Environment.GetReference(cliffTexture)).dataBlocks[0] as TXTRDataBlock;
        roughnessTexture2D = new RCOLFile(Environment.GetReference(roughnessTexture)).dataBlocks[0] as TXTRDataBlock;
        terrainMaterial = new Material(TerrainMaterial);
        terrainMaterial.mainTexture = terrainTexture2D.getTexture();
        terrainMaterial.SetTexture("_CliffTex", cliffTexture2D.getTexture());
        terrainMaterial.SetTexture("_ShoreTex", shoreTexture2D.getTexture());
        terrainMaterial.SetTexture("_Roughness", roughnessTexture2D.getTexture());
    }
}
public class NeighborhoodData
{
    public Material terrainMaterial;
    public List<NHoodDecoration> deco = new List<NHoodDecoration>();
    public List<LotDescription> lots = new List<LotDescription>();
    public TerrainGeometryFile terrain;
    public Neighborhood owner;
    public NeighborhoodTerrainFile terrainFile;

    public NeighborhoodData(Neighborhood hood)
    {
        owner = hood;
        var desertTXTR = (new RCOLFile(Environment.GetReference(Hash.TGIRHash(0xFFEB2F8B, 0x4691724B, 0x1C4A276C, 0x1C0532FA))).dataBlocks[0] as TXTRDataBlock);
        terrain = new TerrainGeometryFile(hood.package.GetItemByFullID(Hash.TGIRHash(0x00000000, 0x00000000, 0xABCB5DA4, hood.package.groupID)));
        terrainMaterial = new Material(Environment.defaultMaterial);
        terrainMaterial.mainTexture = desertTXTR.textures[desertTXTR.textures.Length - 1];
        var lotse = hood.package.GetItemsByType(0x0BF999E7);
        if (lotse != null)
        {
            foreach (var element in lotse)
            {
                lots.Add(new LotDescription(element.Value, this.owner));
            }
        }
        var terrainBytes = hood.package.GetItemsByType(0xABD0DC63);
        if (terrainBytes != null)
        {
            terrainFile = new NeighborhoodTerrainFile(terrainBytes[0].Value,owner);
        }
    }
}
public class Neighborhood
{
    public static Dictionary<string, TerrainType> TerrainTypes = new Dictionary<string, TerrainType>()
    {
        {"Temperate", new TerrainType("nh-temperate-wet-00_txtr","terrain-beach_txtr","nh-test-cliff_txtr","nh-temperate-drydry-00_txtr") },
        {"Dirt", new TerrainType("dirt-rough_txtr","terrain-beach_txtr","nh-test-cliff_txtr","dirt-rough_txtr") },
        {"Desert", new TerrainType("desert-smooth_txtr","terrain-beach_txtr","nh-test-cliff_txtr","desert-rough_txtr") },
        {"Concrete", new TerrainType("concrete-smooth_txtr","terrain-beach_txtr","nh-test-cliff_txtr", "concrete-smooth_txtr") }
    };
    public static Texture2D unknownHoodPicture;
    public Texture2D thumbnail;
    public Texture2D largePic;
    public string name = "Unknown/Corrupt";
    public string desc = "This neighborhood is either corrupted or it's a hidden/tutorial hood";
    public DBPFFile package;
    public NeighborhoodData data = null;
    public string nhoodFolder;

    public void InitializeNHoodData()
    {
        if (data != null)
            return;
        data = new NeighborhoodData(this);
    }

    public void Spawn()
    {
        if (data == null)
        {
            throw new System.Exception("Trying to spawn a Neighborhood without having initialized its NeighborhoodData!");
        }
        var NeighborhoodObject = new GameObject(name);
        var TerrainGameObject = new GameObject("Terrain");
        if (data.terrain.hasWater)
        {
            var WaterGameObject = new GameObject("Water");
            var waterFilter = WaterGameObject.AddComponent<MeshFilter>();
            var waterRenderer = WaterGameObject.AddComponent<MeshRenderer>();
            waterFilter.sharedMesh = data.terrain.waterMesh;
            WaterGameObject.transform.position = new Vector3(0f, data.terrain.waterElevation, 0f);
            waterRenderer.sharedMaterial = TerrainType.WaterMaterial;
            var rendTex = new RenderTexture(512, 512, 24);
            waterRenderer.sharedMaterial.SetTexture("_Reflection", rendTex);
            var reflectionCam = new GameObject();
            var refCamComp = reflectionCam.AddComponent<Camera>();
            var planRef = WaterGameObject.AddComponent<Reflection>();
            planRef.reflectionCamera = refCamComp;
            refCamComp.targetTexture = rendTex;
        }
        var TerrainFilter = TerrainGameObject.AddComponent<MeshFilter>();
        TerrainFilter.sharedMesh = data.terrain.terrainMesh;
        var TerrainRenderer = TerrainGameObject.AddComponent<MeshRenderer>();
        TerrainRenderer.sharedMaterial = data.terrain.tType.terrainMaterial;
        TerrainGameObject.transform.SetParent(NeighborhoodObject.transform);
        foreach(var element in data.lots)
        {
            var lot = element.placeImposter();
            lot.transform.SetParent(NeighborhoodObject.transform);
        }
        foreach(var element in data.terrainFile.decos)
        {
            var dec = element.Place();
            dec.transform.SetParent(NeighborhoodObject.transform);
        }
    }

    public Neighborhood(string folder)
    {
        nhoodFolder = Path.GetFullPath(folder).TrimEnd(Path.DirectorySeparatorChar);
        var packagePath = Path.Combine(folder, Path.GetFileName(nhoodFolder) + "_Neighborhood.package");
        package = new DBPFFile(packagePath);
        var stringsFile = package.GetItemByFullID(Hash.TGIRHash(0x00000001, 0x00000000, 0x43545353, package.groupID));
        if (stringsFile != null)
        {
            var ds = new STRFile(stringsFile);
            name = ds.GetString(0);
            desc = ds.GetString(1);
        }
        var pictureFile = Path.Combine(folder, Path.GetFileName(nhoodFolder) + "_Neighborhood.png");
        var hoodTexture = Environment.LoadUITexture(Hash.TGIRHash(0xCCC30155, 0x00000000, 0x856DDBAC, 0x499DB772));
        if (File.Exists(pictureFile))
            hoodTexture = Environment.LoadUITexture(File.ReadAllBytes(pictureFile));
        thumbnail = hoodTexture;
        largePic = hoodTexture;
    }
}
