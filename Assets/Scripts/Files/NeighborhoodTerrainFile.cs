using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FSO.Files.Utils;
using System.IO;
using SU2.Utils.Helpers;
using SU2.Utils;
using SU2.Files.Formats.RCOL;
using System.Linq;

public class NHoodDecorationDescription
{
    public string name;
    public string modelName;
    public uint guid;
    public GMDCDataBlock model;
}

public class NHoodDecoration
{
    public Vector3 position;
    public float rotation;
    public NHoodDecorationDescription description;

    public GameObject Place()
    {
        var deco = new GameObject(description.name);
        if (description.model != null)
        {
            foreach (var element in description.model.model.meshes)
            {
                var imposterGroup = new GameObject("Mesh");
                var imposterMeshFilter = imposterGroup.AddComponent<MeshFilter>();
                var imposterMeshRenderer = imposterGroup.AddComponent<MeshRenderer>();
                imposterMeshFilter.sharedMesh = element;
                var mats = new Material[element.subMeshCount];
                for (var i = 0; i < mats.Length; i++)
                    mats[i] = Environment.defaultMaterial;
                imposterMeshRenderer.sharedMaterials = mats;
                imposterGroup.transform.SetParent(deco.transform);
            }
        }
        deco.transform.position = position;
        deco.transform.rotation = Quaternion.Euler(new Vector3(0f,rotation,0f));
        return deco;
    }
}

public class NeighborhoodTerrainFile
{
    public List<NHoodDecoration> decos = new List<NHoodDecoration>();

    private float normalize(float deg)
    {
        float normalizedDeg = deg % 360f;

        if (normalizedDeg <= -180)
            normalizedDeg += 360;
        else if (normalizedDeg > 180)
            normalizedDeg -= 360;

        return normalizedDeg;
    }

    public NeighborhoodTerrainFile(byte[] data, Neighborhood nehood)
    {
        var stream = new MemoryStream(data);
        var io = new IoBuffer(stream);
        io.ByteOrder = ByteOrder.LITTLE_ENDIAN;
        var version = io.ReadUInt32();
        io.Skip(2);
        var treeCount = io.ReadUInt32();
        for(var i=0;i<treeCount;i++)
        {
            io.Skip(38); //ToDo: Load Trees
        }
        io.Skip(2);
        var roadCount = io.ReadUInt32();
        for (var i=0;i<roadCount;i++)
        {
            io.Skip(124); //ToDo: Load Roads
        }
        io.Skip(2);
        var bridgeCount = io.ReadUInt32();
        for (var i=0;i<bridgeCount;i++)
        {
            io.Skip(165); //ToDo: Load Bridges
        }
        io.Skip(2);
        var decorationCount = io.ReadUInt32();
        for (var i=0;i<decorationCount;i++)
        {
            io.Skip(1);
            var yPos = io.ReadFloat();
            var xPos = io.ReadFloat();
            var zPos = io.ReadFloat();
            var BBTopLeftY = io.ReadFloat();
            var BBTopRightX = io.ReadFloat();
            var BBBotLeftY = io.ReadFloat();
            var BBBotRightX = io.ReadFloat();
            io.Skip(1);
            var resourceID = io.ReadUInt32();
            var rotation = io.ReadFloat();
            NHoodDecorationDescription decoResource = null;
            if (Environment.hoodDecoByGUID.ContainsKey(resourceID))
                decoResource = Environment.hoodDecoByGUID[resourceID];
            if (decoResource != null)
            {
                var deco = new NHoodDecoration();
                deco.description = decoResource;
                deco.position = new Vector3(yPos, zPos, xPos);
                deco.rotation = rotation*-1;
                decos.Add(deco);
            }
        }
        io.Dispose();
        stream.Dispose();
    }
}