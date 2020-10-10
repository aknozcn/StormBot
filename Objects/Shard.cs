using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SilkroadSecurityApi;

namespace StormBot
{
    public class Shard
    {
        public Shard(Packet packet)
        {
            m_ID = packet.ReadUInt16();
            m_Name = packet.ReadAscii();
            m_Players = packet.ReadUInt16();
            m_Capacity = packet.ReadUInt16();
            m_Status = (ShardStatus)packet.ReadUInt8();
            packet.ReadUInt8(); //GlobalOperationID
        }

        private ushort m_ID;
        public ushort ID
        {
            get { return m_ID; }
            set { m_ID = value; }
        }

        private string m_Name;
        public string Name
        {
            get { return m_Name; }
            set { m_Name = value; }
        }

        private ushort m_Players;
        public ushort Players
        {
            get { return m_Players; }
            set { m_Players = value; }
        }

        private ushort m_Capacity;
        public ushort Capacity
        {
            get { return m_Capacity; }
            set { m_Capacity = value; }
        }

        public enum ShardStatus : byte
        {
            Online = 0,
            Check = 1
        }
        private ShardStatus m_Status;
        public ShardStatus Status
        {
            get { return m_Status; }
            set { m_Status = value; }
        }

        public override string ToString()
        {
            return m_Name;
        }
    }
}

