using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SilkroadSecurityApi;

namespace StormBot
{
    public class Character
    {
        private string m_Name;

        public string Name
        {
            get { return m_Name; }
            set { m_Name = value; }
        }

        private byte m_Level;

        public byte Level
        {
            get { return m_Level; }
            set { m_Level = value; }
        }



        public Character(Packet packet)
        {
            packet.ReadUInt32(); //Model
            m_Name = packet.ReadAscii();
            packet.ReadUInt8(); //Volume
            m_Level = packet.ReadUInt8(); //Level
            packet.ReadUInt64(); //EXP
            packet.ReadUInt16(); //STR
            packet.ReadUInt16(); //INT
            packet.ReadUInt16(); //STAT
            packet.ReadUInt32(); //HP
            packet.ReadUInt32(); //MP
            var restoreFlag = packet.ReadBool();
            if (restoreFlag)
                packet.ReadUInt32(); //Delete Time

            packet.ReadUInt8(); //Something
            packet.ReadUInt8(); //with
            packet.ReadUInt8(); //Guild,Trader etc...

            var itemCount = packet.ReadUInt8();
            for (int i = 0; i < itemCount; i++)
            {
                packet.ReadUInt32();
                packet.ReadUInt8();
            }

            var avatarCount = packet.ReadUInt8();
            for (int i = 0; i < avatarCount; i++)
            {
                packet.ReadUInt32();
                packet.ReadUInt8();
            }
        }

        public override string ToString()
        {
            return string.Format("{0} - [Lv. {1}]", m_Name, m_Level);
        }
    }
}

//02 //Type
//01 //Sucess
//02 //Amount of Characters

//73 07 00 00 //Model
//06 00 //NameLenght
//44 61 78 74 65 72 //Name
//22 //Volume
//01 //Level
//00 00 00 00 00 00 00 00
//14 00 //STR
//14 00 //INT
//00 00 //Stat Points
//C8 00 00 00 //HP
//C8 00 00 00 //MP
//00 //Delete FLAG- if dword
//00 00 //?
//00 //?
//00 //ItemCount - Items from slot 0 to 8 for each item: item_id && item_plus
//00 //AvatarCount -  for each avatar_item: item_id && item_plus


