using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Security.Cryptography;

namespace SU2.Utils
{
    public static class Hash
    {
        public static uint GroupHash(string name)
        {
            name = name.Trim().ToLower();
            long crc = 0x00B704CE;
            int i;
            char[] octets = name.ToCharArray();
            for (int index = 0; index < octets.Length; index++)
            {
                crc ^= octets[index] << 16;
                for (i = 0; i < 8; i++)
                {
                    crc <<= 1;
                    if ((crc & 0x1000000) != 0)
                        crc ^= 0x01864CFB;
                }
            }
            return (uint)((crc & 0x00ffffff) | 0x7f000000);
        }
        public static int TGIHash(uint instanceID, uint type, uint group)
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                hash = hash * 23 + instanceID.GetHashCode();
                hash = hash * 23 + type.GetHashCode();
                hash = hash * 23 + group.GetHashCode();
                return hash;
            }
        }
        public static int TGIRHash(uint instanceID, uint instanceID2, uint type, uint group)
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                hash = hash * 23 + instanceID.GetHashCode();
                hash = hash * 23 + instanceID2.GetHashCode();
                hash = hash * 23 + type.GetHashCode();
                hash = hash * 23 + group.GetHashCode();
                return hash;
            }
        }
    }
    /*
    public class EntryRef : IEquatable<EntryRef>
    {
        readonly uint instanceID;
        readonly uint instanceID2;
        readonly uint type;

        public EntryRef(uint InstanceID, uint InstanceID2, uint Type)
        {
            instanceID = InstanceID;
            instanceID2 = InstanceID2;
            type = Type;
        }

        public bool Equals(EntryRef refe)
        {
            return refe.GetHashCode() == GetHashCode();
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                hash = hash * 23 + instanceID.GetHashCode();
                hash = hash * 23 + instanceID2.GetHashCode();
                hash = hash * 23 + type.GetHashCode();
                return hash;
            }
        }
    }

    public class GroupEntryRef : IEquatable<GroupEntryRef>
    {
        readonly uint instanceID;
        readonly uint instanceID2;
        readonly uint type;
        readonly uint group;

        public GroupEntryRef(uint InstanceID, uint InstanceID2, uint Type, uint Group)
        {
            instanceID = InstanceID;
            instanceID2 = InstanceID2;
            type = Type;
            group = Group;
        }

        public bool Equals(GroupEntryRef refe)
        {
            return refe.GetHashCode() == GetHashCode();
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                hash = hash * 23 + instanceID.GetHashCode();
                hash = hash * 23 + instanceID2.GetHashCode();
                hash = hash * 23 + type.GetHashCode();
                hash = hash * 23 + group.GetHashCode();
                return hash;
            }
        }
    }*/
}