using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using FSO.Files.Utils;
using System;
using SU2.Utils.Helpers;

public class TerrainGeometryFile
{
    public bool hasWater = false;
    public static float shoreDistance = 40f;
    public TerrainType tType;
    public Mesh terrainMesh;
    public Mesh waterMesh;
    public int xSize;
    public int ySize;
    public float waterElevation;
    public string terrainType;
    public Dictionary<Tuple<int,int>, float> terrainElevation = new Dictionary<Tuple<int,int>, float>();


    Mesh CreateFlatPlane(int segments, float size)
    {
        var m = new Mesh();

        int hCount2 = segments + 1;
        int vCount2 = segments + 1;
        int numTriangles = segments * segments * 6;
        int numVertices = hCount2 * vCount2;

        Vector3[] vertices = new Vector3[numVertices];
        Vector2[] uvs = new Vector2[numVertices];
        
        int[] triangles = new int[numTriangles];
        Vector4[] tangents = new Vector4[numVertices];
        Vector4 tangent = new Vector4(1f, 0f, 0f, -1f);

        int index = 0;
        float uvFactorX = 1.0f;
        float uvFactorY = 1.0f;
        float scaleX = size;
        float scaleY = size;
        var anchorOffset = new Vector2(-size / 2.0f, -size / 2.0f);
        for (float y = 0.0f; y < vCount2; y++)
        {
            for (float x = 0.0f; x < hCount2; x++)
            {
                var height = 0f;
                vertices[index] = new Vector3(x * size * World.neighborhoodTerrainSize, height, y * size * World.neighborhoodTerrainSize);
                tangents[index] = tangent;
                uvs[index++] = new Vector2(x * uvFactorX, y * uvFactorY);
            }
        }

        index = 0;
        for (int y = 0; y < segments; y++)
        {
            for (int x = 0; x < segments; x++)
            {
                triangles[index] = (y * hCount2) + x;
                triangles[index + 1] = ((y + 1) * hCount2) + x;
                triangles[index + 2] = (y * hCount2) + x + 1;

                triangles[index + 3] = ((y + 1) * hCount2) + x;
                triangles[index + 4] = ((y + 1) * hCount2) + x + 1;
                triangles[index + 5] = (y * hCount2) + x + 1;
                index += 6;
            }
        }

        m.vertices = vertices;
        m.uv = uvs;
        m.triangles = triangles;
        m.tangents = tangents;
        m.RecalculateNormals();
        return m;
    }

    Mesh CreatePlane(int segments, float size, Dictionary<Tuple<int, int>, float> elevation)
    {
        var m = new Mesh();

        int hCount2 = segments + 1;
        int vCount2 = segments + 1;
        
        int numTriangles = segments * segments * 6;
        int numVertices = hCount2 * vCount2;

        Color[] colors = new Color[numVertices];
        Vector3[] vertices = new Vector3[numVertices];
        Vector2[] uvs = new Vector2[numVertices];
        int[] triangles = new int[numTriangles];
        Vector4[] tangents = new Vector4[numVertices];
        Vector4 tangent = new Vector4(1f, 0f, 0f, -1f);
        List<Vector3> waterPoints = new List<Vector3>();
        int index = 0;
        float uvFactorX = 0.1f;
        float uvFactorY = 0.1f;
        float scaleX = 0.1f * segments;
        float scaleY = 0.1f * segments;
        var anchorOffset = new Vector2(-size / 2.0f, -size / 2.0f);
        for (float y = 0.0f; y < vCount2; y++)
        {
            for (float x = 0.0f; x < hCount2; x++)
            {
                var height = elevation[new Tuple<int,int>((int)y,(int)x)];

                //var col = Color.black;
                if (height <= waterElevation)
                {
                    waterPoints.Add(new Vector3(x * World.neighborhoodTerrainSize, height, y * World.neighborhoodTerrainSize));
                    hasWater = true;
                }
                 //   col = Color.red;
                vertices[index] = new Vector3(x * World.neighborhoodTerrainSize, height, y * World.neighborhoodTerrainSize);
                tangents[index] = tangent;
                colors[index] = Color.black;
                uvs[index++] = new Vector2(x * uvFactorX, y * uvFactorY);
            }
        }
        if (hasWater)
        {
            for (var i = 0; i < vertices.Length; i++)
            {
                var col = colors[i];
                foreach (var element in waterPoints)
                {
                    var dist = Vector3.Distance(element, vertices[i]);
                    if (dist <= shoreDistance)
                    {
                        col = Color.Lerp(Color.red, col, dist / shoreDistance);
                    }
                }
                colors[i] = col;
            }
        }
        index = 0;
        for (int y = 0; y < segments; y++)
        {
            for (int x = 0; x < segments; x++)
            {
                triangles[index] = (y * hCount2) + x;
                triangles[index + 1] = ((y + 1) * hCount2) + x;
                triangles[index + 2] = (y * hCount2) + x + 1;

                triangles[index + 3] = ((y + 1) * hCount2) + x;
                triangles[index + 4] = ((y + 1) * hCount2) + x + 1;
                triangles[index + 5] = (y * hCount2) + x + 1;
                index += 6;
            }
        }

        m.vertices = vertices;
        m.uv = uvs;
        m.triangles = triangles;
        m.tangents = tangents;
        m.colors = colors;
        m.RecalculateNormals();
        return m;
    }

    public TerrainGeometryFile(byte[] file)
    {
        var stream = new MemoryStream(file);
        var reader = IoBuffer.FromStream(stream, ByteOrder.LITTLE_ENDIAN);
        var blockID = reader.ReadUInt32();
        var versionNumber = reader.ReadUInt32();
        xSize = reader.ReadInt32();
        ySize = reader.ReadInt32();
        waterElevation = reader.ReadFloat();
        var terrainTypeLength = reader.ReadUInt32();
        terrainType = reader.ReadCString((int)terrainTypeLength);
        tType = Neighborhood.TerrainTypes[terrainType];
        tType.Load();
        Debug.Log(terrainType);
        var blockID2 = reader.ReadUInt32();
        var versionNumber2 = reader.ReadUInt32();
        var sectionTypeLength = reader.ReadInt32();
        var sectionType = reader.ReadCString(sectionTypeLength);
        var xRange = reader.ReadInt32();
        var yRange = reader.ReadInt32();
        for(var i=0;i<xRange;i++)
        {
            for(var j=0;j<yRange;j++)
            {
                var height = reader.ReadFloat();
                terrainElevation[new Tuple<int, int>(i, j)] = height;
                //terrainElevation.Add(height);
            }
        }
        var blockID3 = reader.ReadUInt32();
        var versionNumber3 = reader.ReadUInt32();
        sectionTypeLength = reader.ReadInt32();
        sectionType = reader.ReadCString(sectionTypeLength);
        xRange = reader.ReadInt32();
        yRange = reader.ReadInt32();
        for(var i=0;i<xRange;i++)
        {
            for(var j=0;j<yRange;j++)
            {
                var uselessByte = reader.ReadByte();
            }
        }
        reader.Dispose();
        stream.Dispose();
        terrainMesh = CreatePlane(xSize, xSize, terrainElevation);
        waterMesh = CreateFlatPlane(1, xSize);
    }
}
