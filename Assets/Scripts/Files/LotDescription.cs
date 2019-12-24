using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using FSO.Files.Utils;
using SU2.Files.Formats.RCOL;
using FSO.Files.Formats.DBPF;
using SU2.Utils;
using SU2.Utils.Helpers;

public class LotDescription
{
    public uint lotNumber;
    public string name;
    public Vector3 position;
    public GMDCDataBlock imposterGMDC;
    public DBPFFile package;

    public GameObject placeImposter()
    {
        var impost = new GameObject(name + " Imposter");
        if (imposterGMDC != null)
        {
            foreach (var element in imposterGMDC.model.meshes)
            {
                var imposterGroup = new GameObject("Mesh");
                var imposterMeshFilter = imposterGroup.AddComponent<MeshFilter>();
                var imposterMeshRenderer = imposterGroup.AddComponent<MeshRenderer>();
                imposterMeshFilter.sharedMesh = element;
                var mats = new Material[element.subMeshCount];
                for (var i = 0; i < mats.Length; i++)
                    mats[i] = Environment.defaultMaterial;
                imposterMeshRenderer.sharedMaterials = mats;
                imposterGroup.transform.SetParent(impost.transform);
            }
        }
        impost.transform.position = position;
        return impost;
    }
    void InitializeLot(Neighborhood nhood)
    {
        var lotFile = Path.Combine(nhood.nhoodFolder,"Lots/"+ Path.GetFileName(nhood.nhoodFolder)+"_Lot"+lotNumber.ToString()+".package");
        package = new DBPFFile(lotFile);
        var rcolFile = package.GetItemByFullID(Hash.TGIRHash(0xFF1FB89E, 0x3ADB7D39, 0xAC4F8687, Hash.GroupHash(Path.GetFileNameWithoutExtension(package.fname))));
        if (rcolFile != null)
        {
            imposterGMDC = new RCOLFile(new DBPFReference(rcolFile,package), true).dataBlocks[0] as GMDCDataBlock;
            foreach (var element in imposterGMDC.model.meshes)
                element.RecalculateNormals();
        }
    }
    public LotDescription(byte[] data, Neighborhood nehood)
    {
        var stream = new MemoryStream(data);
        var io = new IoBuffer(stream);
        io.ByteOrder = ByteOrder.LITTLE_ENDIAN;
        var version1 = io.ReadUInt16(); //13, 14=OFB, 18=AL
        var version2 = io.ReadUInt16(); // 6, 7, 8=FT, 11=AL
        var lotWidth = io.ReadUInt32();
        var lotHeight = io.ReadUInt32();
        var lotType = io.ReadByte(); //0=Residential,1=Community,2=Dorm,3=GreekHouse,4=SecretSociety,5=Hotel,6=HiddenVacationLot,7=HiddenHobbyLot,8=ApartmentBase,9=ApartmentSublot,10=HiddenWitchesLot
        var roadBitfield = io.ReadByte();
        var rotation = io.ReadByte(); //0=Left, 1=Top, 2=Right, 3=Bottom
        var flags = io.ReadUInt32();
        var lotNameLength = io.ReadInt32();
        name = io.ReadCString(lotNameLength);
        var lotDescriptionLength = io.ReadInt32();
        var lotDescription = io.ReadCString(lotDescriptionLength);
        var dunnoCount = io.ReadUInt32();
        for(var i=0;i<dunnoCount;i++)
        {
            io.Skip(4);
        }
        if (version2 >= 7)
        {
            var unknownFloat = io.ReadFloat();
        }
        if (version2 >= 8)
        {
            var unknownInt = io.ReadUInt32();
        }
        if (version2 == 11)
        {
            var apartmentCount = io.ReadByte();
            var apartmentRentalPriceHigh = io.ReadUInt32();
            var apartmentRentalPriceLow = io.ReadUInt32();
            var lotClass = io.ReadUInt32();
            var overrideLotClass = io.ReadByte();
        }
        var lotY = io.ReadUInt32();
        var lotX = io.ReadUInt32();
        var lotZ = io.ReadFloat();
        position = new Vector3((float)lotY*World.neighborhoodTerrainSize, lotZ, (float)lotX*World.neighborhoodTerrainSize);
        lotNumber = io.ReadUInt32();
        var lotOrientation = io.ReadByte(); //0=Below road, 1=Left of road, 2=Above road, 3=Right of road
        var textureLength = io.ReadInt32();
        var textureName = io.ReadCString(textureLength);
        var uselessfkinByte = io.ReadByte();
        if (version1 >= 14)
        {
            var businessOwner = io.ReadUInt32(); //Sim Description Instance Number
        }
        if (version2 == 11)
        {
            var apartmentBaseInstance = io.ReadUInt32(); //If the current lot is an apartment sublot (Lot Type = 9), this is the associated apartment base; otherwise, zero
            io.Skip(9); //9 bytes, always zero apparently
            var itemCount = io.ReadUInt32();
            for(var i=0;i<itemCount;i++)
            {
                var apartmentSublotInstance = io.ReadUInt32();
                var occupyingFamily = io.ReadUInt32(); //Family Information Instance Number
                io.Skip(4); //unknown dword
                var roommateSim = io.ReadUInt32(); //Roommate sim description instance number
            }
            itemCount = io.ReadUInt32();
            for(var i=0;i<itemCount;i++)
            {
                io.Skip(4); //unknown dword
            }
        }
        InitializeLot(nehood);
    }
}
