using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SU2.Utils;
using FSO.Files.Formats.DBPF;
using SU2.Files.Formats.RCOL;

public class NeighborhoodScreenManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Environment.currentNhood.Spawn();
        //RCOLFile.GetModel("ldwashingmachine").Spawn();
        /*
        var desertTXTR = (new RCOLFile(Environment.GetAsset(Hash.TGIRHash(0xFFEB2F8B, 0x4691724B, 0x1C4A276C, 0x1C0532FA))).dataBlocks[0] as TXTRDataBlock);
        var Meme = new TerrainGeometryFile(Environment.currentNhood.package.GetItemByFullID(Hash.TGIRHash(0x00000000, 0x00000000, 0xABCB5DA4, Environment.currentNhood.package.groupID))).terrainMesh;
        var gamo = new GameObject();
        var filt = gamo.AddComponent<MeshFilter>();
        filt.sharedMesh = Meme;
        var rend = gamo.AddComponent<MeshRenderer>();
        rend.sharedMaterial = new Material(Environment.defaultMaterial);
        rend.sharedMaterial.mainTexture = desertTXTR.textures[desertTXTR.textures.Length-1];
        */
        // DEBUG STUFF FOR SIZE COMPARISONN
        /*
        var modelPack = new DBPFFile(Environment.config.model_dir);
        var modelFile = modelPack.GetItemByID(new EntryRef(Environment.config.model_instance, Environment.config.model_instance2, 0xAC4F8687));
        var modelRCOL = new RCOLFile(modelFile);
        var gmdcData = modelRCOL.dataBlocks[0] as GMDCDataBlock;
        var obbi = new GameObject();
        var obbiSkin = obbi.AddComponent<SkinnedMeshRenderer>();
        obbiSkin.sharedMesh = gmdcData.model.meshes[0];
        obbiSkin.sharedMaterial = Environment.defaultMaterial;*/
        // -------------------------------
    }
}
