using System.Collections;
using System;
using System.Collections.Generic;
using FSO.Files.Formats.DBPF;
using FSO.Files.Utils;
using System.IO;
using UnityEngine;

namespace SU2.Files.Formats.RCOL
{
    public class ModelWrapper
    {
        public CachedAsset referencedModel;
        public GMDCDataBlock GMDCData;
        public Dictionary<string, cMaterialDefinitionDataBlock> groupMaterialDefs = new Dictionary<string, cMaterialDefinitionDataBlock>();
       // public Dictionary<string, Material> groupMaterials = new Dictionary<string, Material>();
        public GameObject Spawn()
        {
            var modelGameObject = new GameObject("Model");
            modelGameObject.AddComponent<ReferencingObject>().references.Add(referencedModel);
            var currentOne = 0;
            var i = 0;
            while(currentOne<GMDCData.Groups.Count)
            {
                var thisGroup = new GameObject(GMDCData.Groups[currentOne].groupName);
                var filter = thisGroup.AddComponent<MeshFilter>();
                filter.sharedMesh = GMDCData.model.meshes[i];
                var rendur = thisGroup.AddComponent<MeshRenderer>();
                var mattyList = new Material[filter.sharedMesh.subMeshCount];
                var referenceList = new List<CachedAsset>();
                for (var n=0;n<GMDCData.model.meshes[i].subMeshCount;n++)
                {
                    /*
                    var groupName = GMDCData.Groups[currentOne].groupName;
                    if (groupMaterialDefs.ContainsKey(groupName))
                    {
                        var matDef = groupMaterialDefs[groupName];
                    }
                    */
                    var nMat = new Material(Environment.defaultMaterial);
                    nMat.name = GMDCData.Groups[currentOne].groupName;
                    if (groupMaterialDefs.ContainsKey(GMDCData.Groups[currentOne].groupName))
                    {
                        var matDef = groupMaterialDefs[GMDCData.Groups[currentOne].groupName];
                        if (matDef.properties.ContainsKey("stdMatBaseTextureName"))
                        {
                            var texName = matDef.properties["stdMatBaseTextureName"].ToLower() + "_txtr";
                            var assete = ResourceManager.GetTexture(texName);
                            nMat.mainTexture = assete.asset.getTexture();
                            referenceList.Add(assete);
                            /*
                            var assetCheck = Environment.GetReference(matDef.properties["stdMatBaseTextureName"].ToLower() + "_txtr");
                            if (assetCheck != null)
                                nMat.mainTexture = (new RCOLFile(assetCheck).dataBlocks[0] as TXTRDataBlock).getTexture();*/
                        }
                        if (matDef.properties.ContainsKey("reflectivity"))
                        {
                            nMat.SetFloat("_Reflectivity",float.Parse(matDef.properties["reflectivity"])*0.25f);
                            nMat.SetFloat("_ReflectionBlur", (-float.Parse(matDef.properties["reflectivity"])+1f)*2f);
                        }
                        if (matDef.properties.ContainsKey("reflectionBlur"))
                        {
                            nMat.SetFloat("_ReflectionBlur", float.Parse(matDef.properties["reflectionBlur"]));
                        }
                        if (matDef.properties.ContainsKey("stdMatNormalMapTextureName"))
                        {
                            var texName = matDef.properties["stdMatNormalMapTextureName"].ToLower() + "_txtr";
                            var assete = ResourceManager.GetTexture(texName);
                            var assetTex = assete.asset.getTexture();
                            nMat.SetTexture("_BumpMap", assetTex);
                            /*
                            var asseto = (new RCOLFile(Environment.GetReference(matDef.properties["stdMatNormalMapTextureName"].ToLower() + "_txtr")).dataBlocks[0] as TXTRDataBlock).getHighestLOD();
                            nMat.SetTexture("_BumpMap", asseto);*/
                            if (assetTex.format != TextureFormat.R8)
                                nMat.SetFloat("_BumpOrNormal", 1f);
                            referenceList.Add(assete);
                        }
                        if (nMat.name.ToLower().Contains("shadow"))
                        {
                            nMat.SetFloat("_Cutout", 2.0f);
                        }
                    }
                    mattyList[n] = nMat;
                    currentOne += 1;
                }
                thisGroup.AddComponent<ReferencingObject>().references = referenceList;
                rendur.sharedMaterials = mattyList;
                thisGroup.transform.SetParent(modelGameObject.transform);
                i += 1;
            }
            return modelGameObject;
        }
    }
    //http://simswiki.info/wiki.php?title=E519C933
    public class cMaterialDefinitionDataBlock : DataBlock
    {
        public string MaterialType;
        public string MaterialDescription;
        public Dictionary<string, string> properties = new Dictionary<string, string>();
        public List<string> textures = new List<string>();
    }

    public class cMaterialDefinitionStream : IDataBlockStream
    {
        public DataBlock Read(byte[] bytes, IoBuffer reader, RCOLFile owner)
        {
            var thisData = new cMaterialDefinitionDataBlock();
            thisData.BlockName = "cMaterialDefinition";
            thisData.BlockRCOLID = reader.ReadUInt32();
            thisData.BlockVersion = reader.ReadUInt32();

            var csgRef = reader.ReadVariableLengthPascalString();
            var csg = RCOLFile.streams[csgRef].Read(bytes, reader, owner);

            thisData.MaterialDescription = reader.ReadVariableLengthPascalString();
            thisData.MaterialType = reader.ReadVariableLengthPascalString();

            var propCount = reader.ReadUInt32();
            for(var i=0;i<propCount;i++)
            {
                var propName = reader.ReadVariableLengthPascalString();
                var propValue = reader.ReadVariableLengthPascalString();
                thisData.properties[propName] = propValue;
            }
            if (thisData.BlockVersion > 8)
            {
                var textureCount = reader.ReadUInt32();
                for(var i=0;i<textureCount;i++)
                {
                    thisData.textures.Add(reader.ReadVariableLengthPascalString());
                }
            }
            return thisData;
        }
    }

    public class cGeometryNodeDataBlock : DataBlock
    {
        public string targetName = "";
    }
    public class cGeometryNodeStream : IDataBlockStream
    {
        public DataBlock Read(byte[] bytes, IoBuffer reader, RCOLFile owner)
        {
            var thisData = new cGeometryNodeDataBlock();
            thisData.BlockName = "cGeometryNode";
            thisData.BlockRCOLID = reader.ReadUInt32();
            thisData.BlockVersion = reader.ReadUInt32();
            var graphNode = reader.ReadVariableLengthPascalString();
            var graphNodeDataBlock = RCOLFile.streams[graphNode].Read(bytes, reader, owner);
            var csG = reader.ReadVariableLengthPascalString();
            var csgDataBlock = RCOLFile.streams[csG].Read(bytes, reader, owner);
            thisData.targetName = csgDataBlock.BlockName.Substring(0,csgDataBlock.BlockName.Length-4)+"gmdc";
            if (thisData.BlockVersion == 11)
            {
                var forcedRelocation = reader.ReadUInt16(); //2=yes, 512=no - GMND Redirects to geometry
            }
            if (thisData.BlockVersion == 11 || thisData.BlockVersion == 12)
            {
                var assistedGeometry = reader.ReadUInt16(); //1=yes, 256=no - GMND Contains geometry
                var unknownByte = reader.ReadByte();
            }
            var count = reader.ReadUInt32();
            for(var i=0;i<count;i++)
                { 
                var blockID = reader.ReadVariableLengthPascalString();
                if (!RCOLFile.streams.ContainsKey(blockID))
                {
                    return thisData;
                }
                var blocked = RCOLFile.streams[blockID].Read(bytes, reader, owner);
            }
            return thisData;
        }
    }
    public class cShapeDataBlock : DataBlock
    {
        public string GMNDFileName;
        public Dictionary<string, string> groupMaterials = new Dictionary<string, string>();
    }
    public class cShapeStream : IDataBlockStream
    {
        public DataBlock Read(byte[] bytes, IoBuffer reader, RCOLFile owner)
        {
            var thisData = new cShapeDataBlock();
            thisData.BlockName = "cShape";
            thisData.BlockRCOLID = reader.ReadUInt32();
            thisData.BlockVersion = reader.ReadUInt32();
            var csg = reader.ReadVariableLengthPascalString();
            var csgBlock = RCOLFile.streams["cSGResource"].Read(bytes, reader, owner);
            var cReferent = reader.ReadVariableLengthPascalString();
            var referentBlockID = reader.ReadUInt32();
            var referentVersion = reader.ReadUInt32();
            var objGraph = reader.ReadVariableLengthPascalString();
            var objGraphDataBlock = RCOLFile.streams[objGraph].Read(bytes, reader, owner);
            if (thisData.BlockVersion > 6)
            {
                var lodCount = reader.ReadUInt32();
                for(var i=0;i<lodCount;i++)
                {
                    var lodValue = reader.ReadUInt32();
                }
            }
            if (thisData.BlockVersion == 7 || thisData.BlockVersion == 6)
            {
                var lodCount2 = reader.ReadUInt32();
                for(var i=0;i<lodCount2;i++)
                {
                    var lodType = reader.ReadUInt32();
                    var enabled = reader.ReadByte();
                    var useAGMNDEmbeddedSubmesh = reader.ReadByte();
                    var headerLinkIndex = reader.ReadUInt32();
                }
            }
            
            
            
            if (thisData.BlockVersion == 8)
            {
                var lodCount3 = reader.ReadUInt32();
                for (var i=0;i<lodCount3;i++)
                {
                    var lodType2 = reader.ReadUInt32();
                }
            }
            var enabled2 = reader.ReadByte();
            var gmndFilename = reader.ReadVariableLengthPascalString();
            thisData.GMNDFileName = gmndFilename;
            var matCount = reader.ReadUInt32();
            for (var i=0;i<matCount;i++)
            {
                var group = reader.ReadVariableLengthPascalString();
                var materialDefinition = reader.ReadVariableLengthPascalString();
                thisData.groupMaterials[group] = materialDefinition;
                reader.Skip(9);
            }
            return thisData;
        }
    }

    public class cTransformDataBlock : DataBlock
    {
        public ReferenceDataBlock cObjectGraphNode;
    }
    public class cTransformNodeStream : IDataBlockStream
    {
        public DataBlock Read(byte[] bytes, IoBuffer reader, RCOLFile owner)
        {
            var thisData = new cTransformDataBlock();
            thisData.BlockName = "cTransformNode";
            thisData.BlockRCOLID = reader.ReadUInt32();
            thisData.BlockVersion = reader.ReadUInt32();

            var compositionName = reader.ReadVariableLengthPascalString();
            var compID = reader.ReadUInt32();
            var compVers = reader.ReadUInt32();

            var objGraphName = reader.ReadVariableLengthPascalString();
            thisData.cObjectGraphNode = RCOLFile.streams["cObjectGraphNode"].Read(bytes, reader, owner) as ReferenceDataBlock;

            var count = reader.ReadUInt32();
            for (var i=0;i<count;i++)
            {
                var enabled = reader.ReadByte();
                var dependsOnChildNode = reader.ReadByte();
                var childNodeIndex = reader.ReadUInt32();
            }

            var xTransform = reader.ReadFloat();
            var yTransform = reader.ReadFloat();
            var zTransform = reader.ReadFloat();

            var xQuat = reader.ReadFloat();
            var yQuat = reader.ReadFloat();
            var zQuat = reader.ReadFloat();
            var wQuat = reader.ReadFloat();

            var assignedSubset = reader.ReadInt32(); //[as an int] [7fffffff - enumerated to "No Assignment"]
            return thisData;
        }
    }
    public class cShapeRefDataBlock : ReferenceDataBlock
    {
        public List<uint> links = new List<uint>();
        public cTransformDataBlock cTransformNode;
    }
    public class ReferenceDataBlock : DataBlock
    {
        public List<string> references = new List<string>();
    }
    public class cShapeRefNodeStream : IDataBlockStream
    {
        public DataBlock Read(byte[] bytes, IoBuffer reader, RCOLFile owner)
        {
            var thisData = new cShapeRefDataBlock();
            thisData.BlockName = "cShapeRefNode";
            thisData.BlockRCOLID = reader.ReadUInt32();
            thisData.BlockVersion = reader.ReadUInt32();
            var renderableNodeString = reader.ReadVariableLengthPascalString();
            var renderableRCOLID = reader.ReadUInt32();
            var renderableVersion = reader.ReadUInt32();
            var boundedNodeString = reader.ReadVariableLengthPascalString();
            var boundedRCOLID = reader.ReadUInt32();
            var boundedVersion = reader.ReadUInt32();
            var transformNodeName = reader.ReadVariableLengthPascalString();
            thisData.cTransformNode = RCOLFile.streams["cTransformNode"].Read(bytes, reader, owner) as cTransformDataBlock;
            var unknown1 = reader.ReadUInt16();
            var unknown2 = reader.ReadUInt32();
            var practical = reader.ReadVariableLengthPascalString();
            var unknown1a = reader.ReadUInt32();
            var unknown2a = reader.ReadByte();
            var shapeLinkCount = reader.ReadUInt32();
            for(var i=0;i<shapeLinkCount;i++)
            {
                var shapeEnabled = reader.ReadByte();
                var shapeDependant = reader.ReadByte();
                var shapeFileLinkIndex = reader.ReadUInt32();
                thisData.links.Add(shapeFileLinkIndex);
            }
            var unknown1b = reader.ReadUInt32(); //Always 0 or 16
            var count = reader.ReadUInt32();
            for (var i=0;i<count;i++)
            {
                var unknown1c = reader.ReadUInt32();
            }
            if (thisData.BlockVersion == 21)
            {
                for (var i=0;i<count;i++)
                {
                    var blendName = reader.ReadVariableLengthPascalString();
                }
            }
            var count2 = reader.ReadUInt32();
            for(var i=0; i<count2;i++)
            {
                var unknown1d = reader.ReadByte();
            }
            var unknown1e = reader.ReadUInt32();
            return thisData;
        }
    }

    public class cDataListExtensionStream : IDataBlockStream
    {
        public DataBlock Read(byte[] bytes, IoBuffer reader, RCOLFile owner)
        {
            var thisData = new DataBlock();
            thisData.BlockName = "cDataListExtension";
            thisData.BlockRCOLID = reader.ReadUInt32();
            thisData.BlockVersion = reader.ReadUInt32();
            var exten = reader.ReadVariableLengthPascalString();
            var classID = reader.ReadUInt32();
            var version = reader.ReadUInt32();
            Recursive(reader, thisData, version);
            return thisData;
        }
        void Recursive(IoBuffer reader, DataBlock block, uint version)
        {
            var extensionType = reader.ReadByte();
            var varname2 = reader.ReadVariableLengthPascalString();
            /*if (extensionType < 0x07)
            {*/
            switch (extensionType)
                {
                    case 2:
                    //Uint32
                    
                    reader.ReadUInt32();
                        break;
                    case 3: 
                        //Float
                        reader.ReadFloat();
                        break;
                    case 5:
                    //Transform
                    var xTrans = reader.ReadFloat();
                        var yTrans = reader.ReadFloat();
                        var zTrans = reader.ReadFloat();
                        break;
                    case 6:
                        //String
                        //var stringKey = reader.ReadVariableLengthPascalString();
                        var stringValue = reader.ReadVariableLengthPascalString();
                        break;
                    case 7:
                    //Array
                    /*
                    Debug.Log("Reading array");
                    Recursive(reader, block, version);*/
                    //var varname = reader.ReadVariableLengthPascalString();
                    /*
                    var varname = reader.ReadVariableLengthPascalString();
                    Debug.Log(varname);*/
                    var extensionCount = reader.ReadUInt32();
                    for (var i = 0; i < extensionCount; i++)
                    {
                        Recursive(reader, block, version);
                    }
                    break;
                    case 8:
                        //Quaternion
                        var quatX = reader.ReadFloat();
                        var quatY = reader.ReadFloat();
                        var quatZ = reader.ReadFloat();
                        var quatW = reader.ReadFloat();
                        break;
                    case 9:
                        //Arbitrary data
                        var dataLength = reader.ReadUInt32();
                        reader.ReadBytes(dataLength);
                        break;
                }
               
                /*
                var siz = 16;
                if ((extensionType != 0x03) || (block.BlockVersion == 4))
                    siz += 15;
                if ((extensionType <= 0x03) && (version == 3))
                {
                    if (block.BlockVersion == 5)
                        siz = 31;
                    else
                        siz = 15;
                }
                if ((extensionType <= 0x03) && block.BlockVersion == 4)
                    siz = 31;
                if (extensionType == 6) //This a string
                {
                    var strengKey = reader.ReadVariableLengthPascalString();
                    var strengValue = reader.ReadVariableLengthPascalString();
                }
                else
                {
                    Debug.Log("Final size: " + siz.ToString());
                    var allData = reader.ReadBytes(siz);
                }*/
                /*
            }
            else
            {
                var varname = reader.ReadVariableLengthPascalString();
                Debug.Log(varname);
                var extensionCount = reader.ReadUInt32();
                for(var i=0;i<extensionCount;i++)
                {
                    Recursive(reader, block, version);
                }
            }*/
        }
    }

    public class cObjectGraphNodeStream : IDataBlockStream
    {
        public DataBlock Read(byte[] bytes, IoBuffer reader, RCOLFile owner)
        {
            var thisData = new ReferenceDataBlock();
            thisData.BlockName = "cObjectGraphNode";
            thisData.BlockRCOLID = reader.ReadUInt32();
            thisData.BlockVersion = reader.ReadUInt32();
            var numberOfExtensions = reader.ReadUInt32();
            for(var i=0;i<numberOfExtensions;i++)
            {
                var enabled = reader.ReadByte();
                var dependsOnAnotherRCOL = reader.ReadByte();
                var indexofcDataListExtension = reader.ReadUInt32();
                /*
                if (dependsOnAnotherRCOL != (byte)0)
                    Debug.Log("DEPENDS ON ANOTHER!");*/
            }
            if (thisData.BlockVersion == 4)
            {
                var fileName = reader.ReadVariableLengthPascalString();
                thisData.references.Add(fileName);
            }
            return thisData;
        }
    }

    public class cCompositionTreeNodeStream : IDataBlockStream
    {
        public DataBlock Read(byte[] bytes, IoBuffer reader, RCOLFile owner)
        {
            var thisData = new DataBlock();
            thisData.BlockName = "cCompositionTreeNode";
            thisData.BlockRCOLID = reader.ReadUInt32();
            thisData.BlockVersion = reader.ReadUInt32();
            return thisData;
        }
    }

    public class cResourceNodeDataBlock : DataBlock
    {
        public DataBlock cSGResource;
    }
   
    public class cResourceNodeStream : IDataBlockStream
    {
        public DataBlock Read(byte[] bytes, IoBuffer reader, RCOLFile owner)
        {
            var thisData = new cResourceNodeDataBlock();
            thisData.BlockName = "cResourceNode";
            thisData.BlockRCOLID = reader.ReadUInt32();
            thisData.BlockVersion = reader.ReadUInt32();
            var typecode = reader.ReadByte();
            if (typecode == 1)
            {
                var cSG = reader.ReadVariableLengthPascalString();
                thisData.cSGResource = RCOLFile.streams["cSGResource"].Read(bytes, reader, owner);
                cSG = reader.ReadVariableLengthPascalString();
                RCOLFile.streams["cCompositionTreeNode"].Read(bytes, reader, owner);
                cSG = reader.ReadVariableLengthPascalString();
                RCOLFile.streams["cObjectGraphNode"].Read(bytes, reader, owner);

                var chainCount = reader.ReadUInt32();
                for (var i=0;i<chainCount;i++)
                {
                    var chainEnabled = reader.ReadByte();
                    var chainDependent = reader.ReadByte();
                    var chainLocation = reader.ReadUInt32();
                }

                var isASubnodeCRES = reader.ReadByte();
                var purposeType = reader.ReadUInt32();
            }
            else if (typecode == 0)
            {
                var oGraph = reader.ReadVariableLengthPascalString();
                RCOLFile.streams["cObjectGraphNode"].Read(bytes, reader, owner);

                var enabled = reader.ReadByte();
                var isSubnode = reader.ReadByte();
                var objectBlockLink = reader.ReadUInt32();
                var blockObjectCount = reader.ReadUInt32();
            }
            return thisData;
        }
    }
    public class DataBlock
    {
        public string BlockName;
        public uint BlockRCOLID;
        public uint BlockVersion;
    }
    public interface IDataBlockStream
    {
        DataBlock Read(byte[] bytes, IoBuffer reader, RCOLFile owner);
    }
    #region GMDCElements
    public class GMDCModel
    {
        public List<GMDCTransform> transforms = new List<GMDCTransform>();
        public List<GMDCBlendGroup> blendGroups = new List<GMDCBlendGroup>();
        public List<Vector3> boundingMeshVertices = new List<Vector3>();
        public List<int> boundingMeshFaces = new List<int>();
        public Mesh boundingMesh = null;
        public List<Mesh> meshes = new List<Mesh>();
    }
    public class GMDCBlendGroup
    {
        public string blendGroupName;
        public string blendGroupElementName;
    }
    public class GMDCTransform
    {
        public Quaternion rotation;
        public Vector3 transformation;
    }
    public class GMDCLink
    {
        public List<ushort> Indices = new List<ushort>();
        public List<uint> vertices = new List<uint>();
        public List<uint> normals = new List<uint>();
        public List<uint> uvs = new List<uint>();
    }
    public class GMDCElement
    {
        public uint elementIdentity;
        public uint blockFormat;
        public uint setFormat;
        public uint identityRepitition;
        public List<GMDCElementSet> sets = new List<GMDCElementSet>();
    }
    public class GMDCElementSet
    {
        public List<GMDCElementBlock> blocks = new List<GMDCElementBlock>();
    }
    public class GMDCElementBlock
    {
        public uint integer;
        public float floatvalue;
    }
    public class GMDCGroup
    {
        public string groupName;
        public uint linkIndex;
        public uint primType;
        public uint opacity;
        public List<int> Triangles = new List<int>();
        public List<int> Subset = new List<int>();
    }
    public class GMDCSubset
    {
        public List<Vector3> vertices = new List<Vector3>();
        public List<int> faces = new List<int>();
    }
    #endregion
    #region LIFODataBlock
    public class LIFODataBlock : DataBlock
    {
        public DataBlock cSGResource;
        public Texture2D texture;
    }
    #endregion
    #region TXTRDataBlock
    public class TXTRDataBlock : DataBlock
    {
        public DataBlock cSGResource;
        public Texture2D mipmappedTexture;
        public Texture2D[] textures;

        public Texture2D getHighestLOD()
        {
            return textures[textures.Length - 1];
        }

        public Texture2D getTexture()
        {
            if (mipmappedTexture != null)
                return mipmappedTexture;
            var form = TextureFormat.ARGB32;
            var highMip = getHighestLOD();
            if (highMip == null)
                return new Texture2D(2, 2);
            if (highMip.format == TextureFormat.R8)
                form = TextureFormat.R8;
            mipmappedTexture = new Texture2D(highMip.width, highMip.height, form, textures.Length, false);
            Array.Reverse(textures);
            for(var i=0;i<textures.Length-1;i++)
            {
                mipmappedTexture.SetPixels(textures[i].GetPixels(), i);
            }
            mipmappedTexture.Apply();
            Array.Reverse(textures);
            return mipmappedTexture;
        }
    }
    #endregion
    #region GMDCDataBlock
    public class GMDCDataBlock : DataBlock
    {
        public List<GMDCGroup> Groups = new List<GMDCGroup>();
        public List<GMDCElement> Elements = new List<GMDCElement>();
        public List<GMDCLink> Links = new List<GMDCLink>();
        public List<GMDCSubset> subsets = new List<GMDCSubset>();
        public GMDCModel model;
        public DataBlock cSGResource;
    }
    #endregion
    #region ImageData
    public class cImageDataStream : IDataBlockStream
    {
        public DataBlock Read(byte[] bytes, IoBuffer reader, RCOLFile owner)
        {
            var thisData = new TXTRDataBlock();
            thisData.BlockName = "cImageData";
            reader.Skip(4);
            var modifier = reader.ReadUInt32();
            var cSG = reader.ReadVariableLengthPascalString();
            thisData.cSGResource = RCOLFile.streams["cSGResource"].Read(bytes, reader, owner);
            var width = reader.ReadUInt32();
            var height = reader.ReadUInt32();
            var formatCode = reader.ReadUInt32();
            var mips = reader.ReadUInt32();
            var unk = reader.ReadUInt32();
            var loopCount = reader.ReadUInt32();
            thisData.textures = new Texture2D[mips];
            reader.Skip(4);
            if (modifier == 9)
                reader.Skip(1);
            for (var i=0;i<loopCount;i++)
            {
                var innerLoopCount = mips;
                if (modifier == 9)
                    innerLoopCount = reader.ReadUInt32();
                for (var j = 0; j < innerLoopCount; j++)
                {
                    var dataType = reader.ReadByte();
                    if (dataType == 0)
                    {
                        //var dataSiz = reader.ReadUInt32();
                        var w = width;
                        var h = height;
                        for(var k=0;k<innerLoopCount-j-1;k++)
                        {
                            w /= 2;
                            h /= 2;
                        }
                        var tix = RCOLFile.ReadLIFO(reader, "", (int)w, (int)h);
                        tix.Apply();
                        thisData.textures[j] = tix;
                    }
                    else
                    {
                        /*
                        var stringLength = reader.ReadByte();
                        var lifoName = reader.ReadCString(stringLength);*/
                        var lifoName = reader.ReadVariableLengthPascalString();
                        var lifoAsset = Environment.GetReference(lifoName);
                        if (lifoAsset == null)
                            return thisData;
                        var asset = new RCOLFile(lifoAsset);
                        thisData.textures[j] = (asset.dataBlocks[0] as LIFODataBlock).texture;
                    }
                }
                switch(modifier)
                {
                    case 7:
                        reader.Skip(4);
                        break;
                    case 9:
                        reader.Skip(4);
                        break;
                }
            }
            return thisData;
        }

    }
    #endregion
    #region LIFO
    public class cLevelInfoStream : IDataBlockStream
    {
        public DataBlock Read(byte[] bytes, IoBuffer reader, RCOLFile owner)
        {
            var thisData = new LIFODataBlock();
            thisData.BlockName = "cLevelInfo";
            thisData.BlockRCOLID = reader.ReadUInt32();
            thisData.BlockVersion = reader.ReadUInt32();
            var cSG = reader.ReadVariableLengthPascalString();
            thisData.cSGResource = RCOLFile.streams["cSGResource"].Read(bytes, reader, owner);
            var width = reader.ReadInt32();
            var height = reader.ReadInt32();
            var zLevel = reader.ReadUInt32();
            /*
            var dataLength = reader.ReadUInt32();
            var txFormat = TextureFormat.DXT1;
            reader.Mark();
            var twoBytes = reader.ReadInt16();
            var twelveBytes1 = reader.ReadInt64();
            var twelveBytes2 = reader.ReadInt32();
            reader.SeekFromMark(0);
            if (dataLength == 4 * width * height)
                txFormat = TextureFormat.ARGB32;
            else if (dataLength == 3 * width * height)
                txFormat = TextureFormat.RGB24;
            else if (dataLength == width * height)
            {
                if (thisData.cSGResource.BlockName.Contains("bump"))
                    txFormat = TextureFormat.R8;
                else if (twoBytes != 0 && twelveBytes1 == 0 && twelveBytes2 == 0)
                    txFormat = TextureFormat.DXT5;
                else
                    txFormat = TextureFormat.DXT5;
            }
            else
                txFormat = TextureFormat.DXT1;
            thisData.texture = new Texture2D(width, height, txFormat, false);
            thisData.texture.LoadRawTextureData(reader.ReadBytes(dataLength));*/
            thisData.texture = RCOLFile.ReadLIFO(reader, thisData.cSGResource.BlockName, width, height);
            thisData.texture.Apply();
            return thisData;
        }
    }
    #endregion
    #region cSGResource
    public class cSGResourceStream : IDataBlockStream
    {
        public DataBlock Read(byte[] bytes, IoBuffer reader, RCOLFile owner)
        {
            var thisData = new DataBlock();
            thisData.BlockRCOLID = reader.ReadUInt32();
            thisData.BlockVersion = reader.ReadUInt32();
            //Parsing the filename into the block name instead of cSGResource cause we already know it's a cSGResource... will change if this happens not to be the case
            thisData.BlockName = reader.ReadVariableLengthPascalString();
            return thisData;
        }
    }
    #endregion
    #region GMDC
    public class GMDCStream : IDataBlockStream
    {
        public DataBlock Read(byte[] bytes, IoBuffer reader, RCOLFile owner)
        {
            var thisData = new GMDCDataBlock();
            thisData.BlockName = "cGeometryDataContainer";
            thisData.BlockRCOLID = reader.ReadUInt32();
            thisData.BlockVersion = reader.ReadUInt32();
            var cSG = reader.ReadVariableLengthPascalString();
            thisData.cSGResource = RCOLFile.streams["cSGResource"].Read(bytes, reader, owner);
            #region Elements
            var elementCount = reader.ReadUInt32();
            for (var i = 0; i < elementCount; i++)
            {
                var eleme = new GMDCElement();
                thisData.Elements.Add(eleme);
                var referenceArraySize = reader.ReadUInt32();
                eleme.elementIdentity = reader.ReadUInt32();
                eleme.identityRepitition = reader.ReadUInt32();
                var blockFormat = reader.ReadUInt32();
                var setFormat = reader.ReadUInt32();
                var blockSize = reader.ReadUInt32();
                eleme.blockFormat = blockFormat;
                eleme.setFormat = setFormat;
                uint setLength = 1;
                switch (blockFormat)
                {
                    case 0x00:
                    case 0x04:
                        setLength = 1;
                        break;
                    case 0x01:
                        setLength = 2;
                        break;
                    case 0x02:
                        setLength = 3;
                        break;
                }
                int listLength = (int)blockSize / (int)setLength / 4;
                for (int j = 0; j < listLength; j++)
                {
                    var eleblock = new GMDCElementSet();
                    eleme.sets.Add(eleblock);
                    for (int n = 0; n < setLength; n++)
                    {
                        var blocc = new GMDCElementBlock();
                        eleblock.blocks.Add(blocc);
                        if (blockFormat == 0x04)
                        {
                            // Read a 32bit integer
                            var oy = reader.ReadUInt32();
                            blocc.integer = oy;
                        }
                        else
                        {
                            // Read a float
                            var flo = reader.ReadFloat();
                            blocc.floatvalue = flo;
                        }
                    }
                }
                
                var count = reader.ReadUInt32();
                for (int j = 0; j < count; j++)
                {
                    reader.ReadUInt16();
                }

            }
            #endregion
            #region Linkages
            var count2 = reader.ReadUInt32();
            for (var i = 0; i < count2; i++)
            {
                var link = new GMDCLink();
                thisData.Links.Add(link);
                var indexCount = reader.ReadUInt32();
                for (var j = 0; j < indexCount; j++)
                {
                    var indexValue = reader.ReadUInt16();
                    link.Indices.Add(indexValue);
                }

                var referencedArraySize = reader.ReadUInt32();
                var activeElements = reader.ReadUInt32();

                var submodelVertexCount = reader.ReadUInt32();
                for (var j = 0; j < submodelVertexCount; j++)
                {
                    var subsetIndexValue = reader.ReadUInt16();
                    link.vertices.Add(subsetIndexValue);
                }

                var submodelNormalsCount = reader.ReadUInt32();
                for (var j = 0; j < submodelNormalsCount; j++)
                {
                    var normalsIndexValue = reader.ReadUInt16();
                    link.normals.Add(normalsIndexValue);
                }

                var submodelUVCount = reader.ReadUInt32();
                for (var j = 0; j < submodelUVCount; j++)
                {
                    var UVIndexValue = reader.ReadUInt16();
                    link.uvs.Add(UVIndexValue);
                }
            }
            #endregion
            #region Groups
            var countg = reader.ReadUInt32();
            for (var i = 0; i < countg; i++)
            {
                var group = new GMDCGroup();
                thisData.Groups.Add(group);
                group.primType = reader.ReadUInt32();
                group.linkIndex = reader.ReadUInt32();
                group.groupName = reader.ReadVariableLengthPascalString();
                var faceCount = reader.ReadUInt32();
                for (var j = 0; j < faceCount; j++)
                {
                    var vertReference = reader.ReadUInt16();
                    group.Triangles.Insert(0,(int)vertReference);
                }
                group.opacity = reader.ReadUInt32();

                var subsetCount = reader.ReadUInt32();
                for (var n = 0; n < subsetCount; n++)
                {
                    var modelSubsetReference = reader.ReadUInt16();
                    group.Subset.Add((int)modelSubsetReference);
                }
            }
            #endregion
            #region Model
            var mod = new GMDCModel();
            thisData.model = mod;
            var transformBlockCount = reader.ReadUInt32();
            for (var i = 0; i < transformBlockCount; i++)
            {
                var quatX = reader.ReadFloat();
                var quatY = reader.ReadFloat();
                var quatZ = reader.ReadFloat();
                var quatW = reader.ReadFloat();

                var transformX = reader.ReadFloat();
                var transformY = reader.ReadFloat();
                var transformZ = reader.ReadFloat();
                var trans = new GMDCTransform();
                mod.transforms.Add(trans);
                trans.rotation = new Quaternion(quatX, quatY, quatZ, quatW);
                trans.transformation = new Vector3(transformX, transformY, transformZ);
            }
            var namePairsCount = reader.ReadUInt32();
            for (var i = 0; i < namePairsCount; i++)
            {
                var bShape = new GMDCBlendGroup();
                mod.blendGroups.Add(bShape);
                bShape.blendGroupName = reader.ReadVariableLengthPascalString();
                bShape.blendGroupElementName = reader.ReadVariableLengthPascalString();
            }
            var vertexCount = reader.ReadUInt32();
            if (vertexCount > 0)
            {
                mod.boundingMesh = new Mesh();
                var faceCount = reader.ReadUInt32();
                for (var i = 0; i < vertexCount; i++)
                {
                    var vertX = reader.ReadFloat();
                    var vertY = reader.ReadFloat();
                    var vertZ = reader.ReadFloat();
                    mod.boundingMeshVertices.Add(new Vector3(vertX, vertY, vertZ));
                }
                for (var j = 0; j < faceCount; j++)
                {
                    var faceVert = reader.ReadUInt16();
                    mod.boundingMeshFaces.Add((int)faceVert);
                }
                mod.boundingMesh.SetVertices(mod.boundingMeshVertices);
                mod.boundingMesh.SetTriangles(mod.boundingMeshFaces, 0);
            }
            #endregion
            #region Subset
            var subsCount = reader.ReadUInt32();
            for (var i = 0; i < subsCount; i++)
            {
                var soob = new GMDCSubset();
                thisData.subsets.Add(soob);
                var vertCount = reader.ReadUInt32();
                if (vertCount > 0)
                {
                    var faceCount = reader.ReadUInt32();
                    for (var j = 0; j < vertCount; j++)
                    {
                        var verX = reader.ReadFloat();
                        var verY = reader.ReadFloat();
                        var verZ = reader.ReadFloat();
                        soob.vertices.Add(new Vector3(verX, verY, verZ));
                    }
                    for (var j = 0; j < faceCount; j++)
                    {
                        var value = reader.ReadUInt16();
                        soob.faces.Add(value);
                    }
                }
            }
            #endregion
            for (var i = 0; i < thisData.Links.Count; i++)
            {
                var actualMesh = new Mesh();

                mod.meshes.Add(actualMesh);
                var linkIdentity = thisData.Groups;
                var subGroups = new List<GMDCGroup>();
                var morphDeltas = new List<GMDCElement>();
                GMDCElement morphMap = null;
                foreach (var element in thisData.Groups)
                {
                    if (element.linkIndex == i)
                        subGroups.Add(element);
                }
                actualMesh.subMeshCount = subGroups.Count;
                var verts = new List<Vector3>();
                var normals = new List<Vector3>();
                var uvs = new List<Vector2>();
                var hasVerts = false;
                var hasNormals = false;
                var hasUVS = false;
                foreach (var element in thisData.Links[i].Indices)
                {
                    if (thisData.Elements[element].elementIdentity == 0xDCF2CFDC)
                    {
                        morphMap = thisData.Elements[element];
                    }
                    if (thisData.Elements[element].elementIdentity == 0x5CF2CFE1)
                    {
                        morphDeltas.Add(thisData.Elements[element]);
                    }
                    if (thisData.Elements[element].elementIdentity == 0x5B830781 && !hasVerts)
                    {
                        
                        foreach (var ellie2 in thisData.Elements[element].sets)
                        {
                            var vec = new Vector3(ellie2.blocks[0].floatvalue, ellie2.blocks[2].floatvalue, ellie2.blocks[1].floatvalue);
                            verts.Add(vec);
                        }
                        hasVerts = true;
                        actualMesh.SetVertices(verts);

                        
                    }
                    if (thisData.Elements[element].elementIdentity == 0x3B83078B && !hasNormals)
                    {
                        
                        foreach (var ellie2 in thisData.Elements[element].sets)
                        {
                            var vec = new Vector3(ellie2.blocks[0].floatvalue, ellie2.blocks[2].floatvalue, ellie2.blocks[1].floatvalue);
                            normals.Add(vec);
                        }
                        hasNormals = true;
                        //actualMesh.SetNormals(normals);
                    }
                    if (thisData.Elements[element].elementIdentity == 0xBB8307AB && !hasUVS)
                    {
                        
                        foreach (var ellie2 in thisData.Elements[element].sets)
                        {
                            var vec = new Vector2(ellie2.blocks[0].floatvalue, ellie2.blocks[1].floatvalue);
                            uvs.Add(vec);
                        }
                        hasUVS = true;
                        //actualMesh.SetUVs(0, uvs);
                    }
                }
                /*
                        var iter = 0;
                        for (var j = 0; j < thisData.model.blendGroups.Count; j++)
                        {
                            var name = thisData.model.blendGroups[j].blendGroupName + ", " + thisData.model.blendGroups[j].blendGroupElementName;
                            var deltas = new Vector3[verts.Count];
                            if (name == ", ")
                            {
                                continue;
                            }

                            foreach (var ind in thisData.Links[i].Indices)
                            {
                                if (thisData.Elements[ind].elementIdentity == 0x5CF2CFE1 && thisData.Elements[ind].identityRepitition == iter)
                                {

                                    var morphDeltaElement = thisData.Elements[ind];
                                    Debug.Log("(" + ind.ToString() + ")Mesh has " + actualMesh.vertexCount.ToString() + " vertices, morph has " + morphDeltaElement.sets.Count.ToString());
                                    for (var n = 0; n < morphDeltaElement.sets.Count; n++)
                                    {
                                        deltas[n] = new Vector3(morphDeltaElement.sets[n].blocks[0].floatvalue, morphDeltaElement.sets[n].blocks[1].floatvalue, morphDeltaElement.sets[n].blocks[2].floatvalue);
                                    }
                                    break;
                                }
                            }
                            iter += 1;
                            actualMesh.AddBlendShapeFrame(name, 100f, deltas, new Vector3[deltas.Length], new Vector3[deltas.Length]);
                        }*/
                var iter = 0;
                for (var j = 0; j < thisData.model.blendGroups.Count; j++)
                {
                    var name = thisData.model.blendGroups[j].blendGroupName + ", " + thisData.model.blendGroups[j].blendGroupElementName;
                    var deltas = new Vector3[verts.Count];
                    if (name == ", ")
                        continue;
                    for (var n = 0; n < morphMap.sets.Count; n++)
                    {
                        byte[] intBytes = BitConverter.GetBytes(morphMap.sets[n].blocks[0].integer);
                        for (var o = 0; o < morphDeltas.Count; o++)
                        {
                            var morph = intBytes[o];
                            if (morph == j)
                            {
                                var morphDelta = morphDeltas[o];
                                deltas[n] = new Vector3(morphDelta.sets[n].blocks[0].floatvalue, morphDelta.sets[n].blocks[2].floatvalue, morphDelta.sets[n].blocks[1].floatvalue);
                            }
                        }
                    }
                    actualMesh.AddBlendShapeFrame(name, 100f, deltas, new Vector3[deltas.Length], new Vector3[deltas.Length]);
                    iter += 1;
                }
                if (hasNormals)
                {
                    actualMesh.SetNormals(normals);
                }
                if (hasUVS)
                {
                    actualMesh.SetUVs(0, uvs);
                }
                for (var j = 0; j < subGroups.Count; j++)
                {
                    //var m_Indices = subGroups[j].Triangles.ToArray();
                    /*
                    Debug.Log(subGroups[j].groupName);
                    if (!subGroups[j].groupName.Contains("wall") && owner.imposter)
                    {
                        Array.Reverse(m_Indices);
                        m_Indices = new int[(subGroups[j].Triangles.Count * 2 - 2) * 3];
                        int n = 0;
                        for (int k = 0; k < subGroups[j].Triangles.Count * 2 - 3; k += 2, n++)
                        {
                            m_Indices[k * 3] = n * 2;
                            m_Indices[k * 3 + 1] = n * 2 + 1;
                            m_Indices[k * 3 + 2] = n * 2 + 2;

                            m_Indices[k * 3 + 3] = n * 2 + 1;
                            m_Indices[k * 3 + 4] = n * 2 + 3;
                            m_Indices[k * 3 + 5] = n * 2 + 2;

                        }
                    }*/
                    actualMesh.SetTriangles(subGroups[j].Triangles, j);
                    /*
                    for (var n = 0;n < subGroups[j].Triangles.Count; n++)
                    {
                        subGroups[j].Triangles[n * 3] = j * 2;
                        subGroups[j].Triangles[n * 3 + 1] = j * 2 + 1;
                        subGroups[j].Triangles[n * 3 + 2] = j * 2 + 2;

                        subGroups[j].Triangles[n * 3 + 3] = j * 2 + 1;
                        subGroups[j].Triangles[n * 3 + 4] = j * 2 + 3;
                        subGroups[j].Triangles[n * 3 + 5] = j * 2 + 2;
                    }*/
                    //actualMesh.SetTriangles(m_Indices, j);
                }
                
                /*
                foreach(var element in thisData.Elements)
                {
                    if (element.elementIdentity == 0x5CF2CFE1)
                    {
                        iter += 1;
                        var vertis = new Vector3[verts.Count];
                        for(var j=0;j<element.sets.Count;j++)
                        {
                            vertis[j] = new Vector3(element.sets[j].blocks[0].floatvalue, element.sets[j].blocks[1].floatvalue, element.sets[j].blocks[2].floatvalue);
                        }
                        actualMesh.AddBlendShapeFrame("yeet"+iter.ToString(), 100f, vertis, new Vector3[vertis.Length], new Vector3[vertis.Length]);
                    }
                }*/
                
                
                /*
                for (var j = 0; j < thisData.Links[i].Indices.Count; j++)
                {
                    var element = thisData.Links[i].Indices[j];
                    if (thisData.Elements[element].elementIdentity == 0x5CF2CFE1)
                    {
                        var morVerts = new Vector3[verts.Count];
                        foreach (var ellie2 in thisData.Elements[element].sets)
                        {
                            var vec = new Vector3(ellie2.blocks[0].floatvalue, ellie2.blocks[1].floatvalue, ellie2.blocks[2].floatvalue);
                            morVerts.Add(vec);
                        }
                        Debug.Log("Adding blend");
                        actualMesh.AddBlendShapeFrame(thisData.model.blendGroups[j].blendGroupElementName, 100f, morVerts.ToArray(), new Vector3[morVerts.Count], new Vector3[morVerts.Count]);
                    }
                }*/
                //actualMesh.RecalculateBounds();
            }
            return thisData;
        }
    }
    #endregion
    public enum ReferenceType { TGIR, TGI};
    public class RCOLFile
    {
        public ReferenceType referenceType = ReferenceType.TGIR;
        public List<int> references = new List<int>();
        public bool imposter = false;
        public static List<uint> RCOLTypeIDs = new List<uint>()
        {
            0xAC4F8687,
            0x7BA3838C,
            0x49596978,
            0xE519C933,
            0xFC6EB1F7,
            0x1C4A276C
        };
        public static Dictionary<string, IDataBlockStream> streams = new Dictionary<string, IDataBlockStream>()
        {
            { "cSGResource", new cSGResourceStream() },
            { "cGeometryDataContainer", new GMDCStream() },
            { "cLevelInfo", new cLevelInfoStream() },
            { "cImageData", new cImageDataStream() },
            { "cCompositionTreeNode", new cCompositionTreeNodeStream() },
            { "cObjectGraphNode", new cObjectGraphNodeStream() },
            { "cResourceNode", new cResourceNodeStream() },
            { "cDataListExtension", new cDataListExtensionStream() },
            { "cShapeRefNode", new cShapeRefNodeStream() },
            { "cTransformNode", new cTransformNodeStream() },
            { "cShape", new cShapeStream() },
            { "cGeometryNode", new cGeometryNodeStream() },
            { "cMaterialDefinition", new cMaterialDefinitionStream() }
        };
        public List<DataBlock> dataBlocks = new List<DataBlock>();
        private IoBuffer reader;
        public static ModelWrapper GetModel(string name)
        {
            //culmination of hours of fucking endless typing
            var finalMod = new ModelWrapper();
            var cresFile = name + "_cres";
            var assetCres = Environment.GetReference(cresFile.ToLower());
            if (assetCres == null)
                return finalMod;
            var testCres = new RCOLFile(assetCres);
            foreach(var element in testCres.dataBlocks)
            {
                if (element.BlockName == "cShapeRefNode")
                {
                    var shapeRefBlock = element as cShapeRefDataBlock;
                    //shapeRefBlock.cTransformNode.cObjectGraphNode.references.Add("root_rot");
                    foreach (var shapeLink in shapeRefBlock.links)
                    {
                        DBPFReference shape;
                        if (testCres.referenceType == ReferenceType.TGIR)
                            shape = Environment.GetReference(testCres.references[(int)shapeLink]);
                        else
                            shape = Environment.GetReferenceTGI(testCres.references[(int)shapeLink]);
                        if (shape != null)
                        {
                            var shapeAsset = new RCOLFile(shape);
                            var shapeBlock = shapeAsset.dataBlocks[0] as cShapeDataBlock;
                            foreach (var element3 in shapeBlock.groupMaterials)
                            {
                                var assut = Environment.GetReference(element3.Value.ToLower() + "_txmt");
                                if (assut != null)
                                {
                                    finalMod.groupMaterialDefs[element3.Key] = (new RCOLFile(assut).dataBlocks[0] as cMaterialDefinitionDataBlock);
                                }
                            }
                            var gmndFile = shapeBlock.GMNDFileName;
                            var gmndAsset = new RCOLFile(Environment.GetReference(gmndFile.ToLower()));
                            var finalGMDC = (gmndAsset.dataBlocks[0] as cGeometryNodeDataBlock).targetName;
                            var demGMDC = new RCOLFile(Environment.GetReference(finalGMDC.ToLower())).dataBlocks[0] as GMDCDataBlock;

                            finalMod.GMDCData = demGMDC;
                            return finalMod;
                            /*
                            var finalName = name + "_" + shape + "_shpe";
                            Debug.Log(finalName);
                            var shapeCheck = Environment.GetReference(finalName.ToLower());*/
                            /*
                            if (shapeCheck != null)
                            {
                                var shapeAsset = new RCOLFile(shapeCheck);
                                var shapeBlock = shapeAsset.dataBlocks[0] as cShapeDataBlock;
                                foreach (var element3 in shapeBlock.groupMaterials)
                                {
                                    var assut = Environment.GetReference(element3.Value.ToLower() + "_txmt");
                                    Debug.Log("Requesting texture " + element3.Value.ToLower() + "_txmt" + " for " + element3.Key);
                                    if (assut != null)
                                    {
                                        finalMod.groupMaterialDefs[element3.Key] = (new RCOLFile(assut).dataBlocks[0] as cMaterialDefinitionDataBlock);
                                    }
                                }
                                var gmndFile = shapeBlock.GMNDFileName;
                                Debug.Log("GMND is " + gmndFile);
                                var gmndAsset = new RCOLFile(Environment.GetReference(gmndFile.ToLower()));
                                var finalShit = (gmndAsset.dataBlocks[0] as cGeometryNodeDataBlock).targetName;
                                Debug.Log("Final gmdc is " + finalShit);
                                var demGMDC = new RCOLFile(Environment.GetReference(finalShit.ToLower())).dataBlocks[0] as GMDCDataBlock;

                                finalMod.GMDCData = demGMDC;
                                return finalMod;
                            }*/
                        }
                    }
                }
            }
            return null;
        }
        public static Texture2D ReadLIFO(IoBuffer reader, string filename, int width, int height)
        {
            /*
            var width = reader.ReadInt32();
            var height = reader.ReadInt32();
            var zLevel = reader.ReadUInt32();*/
            var dataLength = reader.ReadUInt32();
            var txFormat = TextureFormat.DXT1;
            reader.Mark();
            var twoBytes = reader.ReadInt16();
            var twelveBytes1 = reader.ReadInt64();
            var twelveBytes2 = reader.ReadInt32();
            reader.SeekFromMark(0);
            if (dataLength == 4 * width * height)
                txFormat = TextureFormat.ARGB32;
            else if (dataLength == 3 * width * height)
                txFormat = TextureFormat.RGB24;
            else if (dataLength == width * height)
            {
                if (filename.Contains("bump"))
                    txFormat = TextureFormat.R8;
                else if (twoBytes != 0 && twelveBytes1 == 0 && twelveBytes2 == 0)
                    txFormat = TextureFormat.DXT5;
                else
                    txFormat = TextureFormat.DXT5;
            }
            else
                txFormat = TextureFormat.DXT1;
            var texture = new Texture2D(width, height, txFormat, false);
            try
            {
                texture.LoadRawTextureData(reader.ReadBytes(dataLength));
            }
            catch {};
            return texture;
        }
        public static string GetGMNDName(byte[] bytes)
        {
            var stream = new MemoryStream(bytes);
            var io = IoBuffer.FromStream(stream, ByteOrder.LITTLE_ENDIAN);
            var vMark = io.ReadUInt32();
            if (vMark != 0xFFFF0001)
            {
                io.Seek(SeekOrigin.Begin, 0);
            }
            var fileLinks = io.ReadUInt32();
            var skipCount = 16;
            if (vMark != 0xFFFF0001)
                skipCount = 12;
            io.Skip(fileLinks * skipCount);
            var items = io.ReadUInt32();
            io.Skip(4 * items);
            var blockName = io.ReadVariableLengthPascalString();
            //var boop = RCOLFile.streams[blockName].Read(bytes, io, null);
            //var cSG = io.ReadVariableLengthPascalString();
            var eBlockRCOLID = io.ReadUInt32();
            var eBlockVersion = io.ReadUInt32();
            blockName = io.ReadVariableLengthPascalString();
            RCOLFile.streams[blockName].Read(bytes, io, null);
            var cesg = io.ReadVariableLengthPascalString();
            eBlockRCOLID = io.ReadUInt32();
            eBlockVersion = io.ReadUInt32();
            var retName = io.ReadVariableLengthPascalString();
            io.Dispose();
            stream.Dispose();
            return retName;
        }
        public static string GetCRESName(byte[] bytes)
        {
            var stream = new MemoryStream(bytes);
            var io = IoBuffer.FromStream(stream, ByteOrder.LITTLE_ENDIAN);
            var vMark = io.ReadUInt32();
            if (vMark != 0xFFFF0001)
            {
                io.Seek(SeekOrigin.Begin, 0);
            }
            var fileLinks = io.ReadUInt32();
            var skipCount = 16;
            if (vMark != 0xFFFF0001)
                skipCount = 12;
            io.Skip(fileLinks * skipCount);
            var items = io.ReadUInt32();
            io.Skip(4 * items);
            var blockName = io.ReadVariableLengthPascalString();
            var BlockRCOLID = io.ReadUInt32();
            var BlockVersion = io.ReadUInt32();
            io.Skip(1);

            var cesg = io.ReadVariableLengthPascalString();
            var datte = streams[cesg].Read(bytes, io, null);
            //Debug.Log(datte.BlockName);
            io.Dispose();
            stream.Dispose();
            return datte.BlockName;
        }
        public static string GetName(byte[] bytes)
        {
            var stream = new MemoryStream(bytes);
            var io = IoBuffer.FromStream(stream, ByteOrder.LITTLE_ENDIAN);
            var vMark = io.ReadUInt32();
            if (vMark != 0xFFFF0001)
            {
                io.Seek(SeekOrigin.Begin, 0);
            }
            var fileLinks = io.ReadUInt32();
            var skipCount = 16;
            if (vMark != 0xFFFF0001)
                skipCount = 12;
            io.Skip(fileLinks * skipCount);
            if (io.Stream.Position >= io.Stream.Length)
                return "";
            var items = io.ReadUInt32();
            io.Skip(4 * items);
            var blockName = io.ReadVariableLengthPascalString();
            var BlockRCOLID = io.ReadUInt32();
            var BlockVersion = io.ReadUInt32();
            var cSG = io.ReadVariableLengthPascalString();
            BlockRCOLID = io.ReadUInt32();
            BlockVersion = io.ReadUInt32();
            var retName = io.ReadVariableLengthPascalString();
            io.Dispose();
            stream.Dispose();
            //Debug.Log(retName);
            return retName;
            /*
            var dataBlock = streams[blockName].Read(bytes, reader);
            dataBlocks.Add(dataBlock);*/
            /*
            for (var i = 0; i < fileLinks; i++)
            {
                var groupID = reader.ReadUInt32();
                var instanceID = reader.ReadUInt32();
                uint resourceID = 0x0;
                if (vMark == 0xFFFF0001)
                    resourceID = reader.ReadUInt32();
                var typeID = reader.ReadUInt32();
            }*/
        }
        public RCOLFile(DBPFReference reference, bool imposterTest = false)
        {
            imposter = imposterTest;
            var stream = new MemoryStream(reference.fileBytes);
            reader = IoBuffer.FromStream(stream, ByteOrder.LITTLE_ENDIAN);
            var vMark = reader.ReadUInt32();
            if (vMark != 0xFFFF0001)
            {
                referenceType = ReferenceType.TGI;
                reader.Seek(SeekOrigin.Begin, 0);
            }
            var fileLinks = reader.ReadUInt32(); //THESE ARE THE mfing REFERENCES
            for(var i=0;i<fileLinks;i++)
            {
                var groupID = reader.ReadUInt32();
                if (groupID == 0xFFFFFFFF)
                    groupID = reference.file.groupID;
                var instanceID = reader.ReadUInt32();
                uint resourceID = 0x0;
                if (referenceType == ReferenceType.TGIR)
                    resourceID = reader.ReadUInt32();
                var typeID = reader.ReadUInt32();
                var entryID = Utils.Hash.TGIHash(instanceID,typeID,groupID);
                if (referenceType == ReferenceType.TGIR)
                    entryID = Utils.Hash.TGIRHash(instanceID, resourceID, typeID, groupID);
                references.Add(entryID);
            }
            var items = reader.ReadUInt32();
            for (var i = 0; i < items; i++)
            {
                var rcolID = reader.ReadUInt32();
            }
            for (var i = 0;i < items; i++)
            {
                var blockName = reader.ReadVariableLengthPascalString();
                var dataBlock = streams[blockName].Read(reference.fileBytes, reader, this);
                dataBlocks.Add(dataBlock);
                //Read data blocks
            }
            reader.Dispose();
            stream.Dispose();
        }
    }
}