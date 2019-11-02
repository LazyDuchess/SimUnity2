using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using FSO.Files.Utils;
using System;

namespace SU2.Files.Formats.STR
{
    public class StringSet
    {
        public string value;
        public string description;

        public StringSet(string value, string description)
        {
            this.value = value;
            this.description = description;
        }
    }
    public class STRFile
    {
        public string fileName;
        public Dictionary<byte, List<StringSet>> strings = new Dictionary<byte, List<StringSet>>();
        public string GetString(int id)
        {
            return strings[Environment.language][id].value;
        }
        string readstr(IoBuffer reader)
        {
            var stri = new List<byte>();
            byte byt = 1;
            do
            {
                byt = reader.ReadByte();
                stri.Add(byt);
            }
            while (byt != 0);
            return System.Text.Encoding.UTF8.GetString(stri.ToArray());
        }
        public STRFile(byte[] bytes)
        {
            var stream = new MemoryStream(bytes);
            var reader = IoBuffer.FromStream(stream, ByteOrder.LITTLE_ENDIAN);
            fileName = readstr(reader);
            reader.Seek(SeekOrigin.Begin, 66);
            var stringSets = reader.ReadUInt16();
            for (var i = 0; i < stringSets; i++)
            {
                //Iobuffer's Read variable length pascal string messes up here for some reason?
                var languageCode = reader.ReadByte();
                var value = readstr(reader);
                var desc = readstr(reader);
                if (!strings.ContainsKey(languageCode))
                    strings[languageCode] = new List<StringSet>();
                strings[languageCode].Add(new StringSet(value, desc));
            }
            stream.Dispose();
            reader.Dispose();
        }
    }
}