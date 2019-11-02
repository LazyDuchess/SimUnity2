using System.Collections;
using System;
using System.Collections.Generic;
using FSO.Files.Formats.DBPF;
using FSO.Files.Utils;
using System.IO;
using SU2.Utils;

namespace SU2.Files.Formats.DIR
{
    /// <summary>
    /// Contains a list of all compressed resources in a DBPF package and their uncompressed sizes.
    /// </summary>
    public class DIRFile
    {
        //private Dictionary<EntryRef, DIREntry> m_EntryByID = new Dictionary<EntryRef, DIREntry>();
        private Dictionary<int, DIREntry> m_EntryByFullID = new Dictionary<int, DIREntry>();

        private IoBuffer reader;

        public DIRFile()
        {
        }
        /*
        public DIREntry GetEntryByID(EntryRef ID)
        {
            if (m_EntryByID.ContainsKey(ID))
                return m_EntryByID[ID];
            else
                return null;
        }*/
        public static void Read(DBPFFile package, byte[] file)
        {
            var gGroupID = Hash.GroupHash(Path.GetFileNameWithoutExtension(package.fname));
            var stream = new MemoryStream(file);
            var reader = IoBuffer.FromStream(stream, ByteOrder.LITTLE_ENDIAN);
            //var count = 0;
            while (stream.Position < file.Length)
            {
                var TypeID = reader.ReadUInt32();
                var GroupID = reader.ReadUInt32();
                if (GroupID == 0xFFFFFFFF && package.fname != "")
                    GroupID = gGroupID;
                var InstanceID = reader.ReadUInt32();
                uint  InstanceID2 = 0x00000000;
                if (package.IndexMinorVersion >= 2)
                    InstanceID2 = reader.ReadUInt32();
                var idEntry2 = Hash.TGIRHash(InstanceID, InstanceID2, TypeID, GroupID);
                package.GetEntryByFullID(idEntry2).uncompressedSize = reader.ReadUInt32();
                /*
                if (!m_EntryByFullID.ContainsKey(idEntry2))
                    m_EntryByFullID.Add(idEntry2, entry);
                entry.UncompressedFileSize = reader.ReadUInt32();
                count += 1;*/
            }
            reader.Dispose();
            stream.Dispose();
        }
        public DIREntry GetEntryByID(int tgir)
        {
            if (m_EntryByFullID.ContainsKey(tgir))
                return m_EntryByFullID[tgir];
            else
                return null;
        }
        public DIRFile(DBPFFile file, byte[] bytes)
        {
            var stream = new MemoryStream(bytes);
            reader = IoBuffer.FromStream(stream, ByteOrder.LITTLE_ENDIAN);
            var count = 0;
            while (stream.Position < bytes.Length)
            {
                var entry = new DIREntry();
                
                entry.TypeID = reader.ReadUInt32();
                entry.GroupID = reader.ReadUInt32();
                if (entry.GroupID == 0xFFFFFFFF && file.fname != "")
                    entry.GroupID = Hash.GroupHash(Path.GetFileNameWithoutExtension(file.fname));
                entry.InstanceID = reader.ReadUInt32();
                entry.InstanceID2 = 0x0;
                
                if (file.IndexMinorVersion >= 2)
                    entry.InstanceID2 = reader.ReadUInt32();
                //m_EntryByID.Add((((ulong)entry.InstanceID) << 32) + (ulong)entry.InstanceID2 + (ulong)entry.TypeID, entry);
                /*
                var idEntry = new EntryRef(entry.InstanceID, entry.InstanceID2, entry.TypeID);
                if (!m_EntryByID.ContainsKey(idEntry))
                    m_EntryByID.Add(idEntry, entry);*/
                var idEntry2 = Hash.TGIRHash(entry.InstanceID, entry.InstanceID2, entry.TypeID, entry.GroupID);
                if (!m_EntryByFullID.ContainsKey(idEntry2))
                    m_EntryByFullID.Add(idEntry2, entry);
                entry.UncompressedFileSize = reader.ReadUInt32();
                count += 1;
            }
            reader.Dispose();
            stream.Dispose();
        }
    }
}