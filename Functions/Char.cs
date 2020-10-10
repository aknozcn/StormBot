using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SilkroadSecurityApi;

namespace StormBot.Functions
{
    class CharStrings
    {
        public static Packet char_Packet;
        public static byte[] skip_id;
        public static uint UniqueID;
        public static uint AccountID;

        public static string Race;
        public static byte Level;
        public static byte MaxLevel;
        public static ulong Gold;
        public static uint SkillPoints;
        public static ushort StatPoints = 0;
        public static bool Zerk;

        public static float WalkSpeed = 17.6f; //16*1,1
        public static float RunSpeed = 55.0f; //50*1,1
        public static float ZerkSpeed = 110.0f;//-.-

        public static uint PHYdef = 0;
        public static uint MAGdef = 0;
        public static uint hitRate = 0;
        public static uint parryRatio = 0;

        public static uint CurrentHP = 0;
        public static uint CurrentMP = 0;
        public static uint MaximumHP = 0;
        public static uint MaximumMP = 0;
        public static ulong MaxExp = 0;
        public static ulong Exp = 0;
        public static ushort STR = 0;
        public static ushort INT = 0;
        public static byte bad_status = 0;
        public static List<string> explist = new List<string>();

        //public static Items_[] Items;
        //public static Avatars_[] Avatars;



        public static string PlayerName;
        public static List<string> CharNameANDuniqueID = new List<string>();

        public static List<string> GlobalsTypeSlot = new List<string>();

        public static List<byte> inventoryslot = new List<byte>();
        public static List<uint> inventoryid = new List<uint>();
        public static List<string> inventorytype = new List<string>();
        public static List<ushort> inventorycount = new List<ushort>();
        public static List<uint> inventorydurability = new List<uint>();

        public static List<uint> skillid = new List<uint>();
        public static List<string> skillname = new List<string>();
        public static List<string> skilltype = new List<string>();
        public static List<int> skillwait = new List<int>();
        public static List<DateTime> skilllastuse = new List<DateTime>();

    }
    class Items_Info
    {
        public static List<uint> itemsidlist = new List<uint>();
        public static List<string> itemstypelist = new List<string>();
        public static List<string> itemsnamelist = new List<string>();
        public static List<byte> itemslevellist = new List<byte>();
        public static List<ushort> items_maxlist = new List<ushort>();
        public static List<uint> itemsdurabilitylist = new List<uint>();
    }
    class Mobs_Info
    {
        public static List<uint> mobsidlist = new List<uint>();
        public static List<string> mobstypelist = new List<string>();
        public static List<string> mobsnamelist = new List<string>();
        public static List<byte> mobslevellist = new List<byte>();
        public static List<uint> mobshplist = new List<uint>();
        public static List<string> mobsifuniquelist = new List<string>();

    }
    class Skills_Info
    {
        public static List<uint> skillsidlist = new List<uint>();
        public static List<string> skillstypelist = new List<string>();
        public static List<string> skillsnamelist = new List<string>();

        public static List<int> skillsmpreq = new List<int>();
        public static List<int> skillsstatuslist = new List<int>();
        public static List<int> skillscasttimelist = new List<int>();
        public static List<int> skillcooldownlist = new List<int>();
    }
}
