using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using FSO.Files.Utils;
using System.Xml;
using System;
using System.Text;

namespace SU2.Files.Formats.CPF
{
    public enum DataType : uint
    {
        UInt = 0xEB61E4F7,
        String = 0x0B8BEA18,
        Float = 0xABC78708,
        Boolean = 0xCBA908E1,
        Int2 = 0x0C264712,
    }
    public class CPFEntry
    {
        public string name;
        public int Integer;
        public uint Unsigned;
        public float Float;
        public bool Boolean;
        public string String;
        public DataType dataType;

        public CPFEntry(string name, DataType dataType, uint Unsigned)
        {
            this.name = name;
            this.dataType = dataType;
            this.Unsigned = Unsigned;
        }

        public CPFEntry(string name, DataType dataType, int Integer)
        {
            this.name = name;
            this.dataType = dataType;
            this.Integer = Integer;
        }

        public CPFEntry(string name, DataType dataType, string String)
        {
            this.name = name;
            this.dataType = dataType;
            this.String = String;
        }

        public CPFEntry(string name, DataType dataType, float Float)
        {
            this.name = name;
            this.dataType = dataType;
            this.Float = Float;
        }

        public CPFEntry(string name, DataType dataType, bool Boolean)
        {
            this.name = name;
            this.dataType = dataType;
            this.Boolean = Boolean;
        }
    }
    public class CPFFile
    {
        public static Dictionary<string, DataType> StringToDataType = new Dictionary<string, DataType>()
        {
            { "0XEB61E4F7",DataType.UInt},
            { "0X0B8BEA18",DataType.String},
            { "0XABC78708",DataType.Float},
            { "0XCBA908E1",DataType.Boolean},
            { "0X0C264712",DataType.Int2}
        };

        public Dictionary<string, CPFEntry> entries = new Dictionary<string, CPFEntry>();

        void ReadAsXML(byte[] file)
        {
            var doc = new XmlDocument();
            doc.Load(new MemoryStream(file));
            foreach (XmlNode node in doc.DocumentElement.ChildNodes)
            {
                var key = node.Attributes["key"].InnerText;
                var type = StringToDataType[node.Attributes["type"].InnerText.ToUpper()];
                switch (type) {
                    case DataType.UInt:
                        var entri2 = new CPFEntry(key, type, UInt32.Parse(node.InnerText));
                        entries[key] = entri2;
                        break;
                    case DataType.String:
                        var entri3 = new CPFEntry(key, type, node.InnerText);
                        entries[key] = entri3;
                        break;
                    case DataType.Float:
                        var entri4 = new CPFEntry(key, type, float.Parse(node.InnerText));
                        entries[key] = entri4;
                        break;
                    case DataType.Boolean:
                        var boolval = false;
                        if (node.InnerText != "false")
                        boolval = true;
                        var entri = new CPFEntry(key, type, boolval);
                        entries[key] = entri;
                        break;
                    case DataType.Int2:
                        var entri5 = new CPFEntry(key, type, int.Parse(node.InnerText));
                        entries[key] = entri5;
                        break;
                }
            }
        }
        public CPFFile(byte[] file)
        {
            if (Encoding.UTF8.GetChars(file)[0] == '<')
            {
                ReadAsXML(file);
                return;
            }
            var stream = new MemoryStream(file);
            var io = new IoBuffer(stream);
            io.ByteOrder = ByteOrder.LITTLE_ENDIAN;
            var typeID = io.ReadUInt32();
            var version = io.ReadUInt16();
            var numberOfItems = io.ReadUInt32();
            for (var i = 0; i < numberOfItems; i++)
            {
                var dataType = (DataType)io.ReadUInt32();
                var fieldNameLength = io.ReadInt32();
                var fieldName = io.ReadCString(fieldNameLength);
                if (fieldName[0] != '"')
                {
                    switch (dataType)
                    {
                        case DataType.UInt:
                            var data = new CPFEntry(fieldName, dataType, io.ReadUInt32());
                            entries.Add(fieldName, data);
                            break;
                        case DataType.Int2:
                            var data5 = new CPFEntry(fieldName, dataType, io.ReadInt32());
                            entries.Add(fieldName, data5);
                            break;
                        case DataType.String:
                            var dataLength = io.ReadInt32();
                            var data2 = new CPFEntry(fieldName, dataType, io.ReadCString(dataLength));
                            entries.Add(fieldName, data2);
                            break;
                        case DataType.Float:
                            var data3 = new CPFEntry(fieldName, dataType, io.ReadFloat());
                            entries.Add(fieldName, data3);
                            break;
                        case DataType.Boolean:
                            var boole = false;
                            if (io.ReadByte() == (byte)1)
                                boole = true;
                            var data4 = new CPFEntry(fieldName, dataType, boole);
                            entries.Add(fieldName, data4);
                            break;
                    }
                }
            }
        }

        public float GetFloat(string entry)
        {
            if (entries.ContainsKey(entry))
                return entries[entry].Float;
            return 0f;
        }

        public int GetInt(string entry)
        {
            if (entries.ContainsKey(entry))
                return entries[entry].Integer;
            return 0;
        }

        public uint GetUInt(string entry)
        {
            if (entries.ContainsKey(entry))
                return entries[entry].Unsigned;
            return 0;
        }

        public bool GetBool(string entry)
        {
            if (entries.ContainsKey(entry))
                return entries[entry].Boolean;
            return false;
        }

        public string GetString(string entry)
        {
            if (entries.ContainsKey(entry))
                return entries[entry].String;
            return "";
        }
    }
}
