using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SilkroadSecurityApi;

namespace StormBot
{
    class Globals
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


        public struct Types_
        {
            public List<string> grab_types;
            public List<string> grabpet_spawn_types;
            public List<string> attack_types;
            public List<string> attack_spawn_types;
        }
        public static Types_ Types = new Types_();

        public static void InitializeTypes()
        {
            Types.grab_types = new List<string>();
            //Grab Pets
            Types.grab_types.Add("COS_P_SPOT_RABBIT");
            Types.grab_types.Add("COS_P_RABBIT");
            Types.grab_types.Add("COS_P_GGLIDER");
            Types.grab_types.Add("COS_P_MYOWON");
            Types.grab_types.Add("COS_P_SEOWON");
            Types.grab_types.Add("COS_P_RACCOONDOG");
            Types.grab_types.Add("COS_P_CAT");
            Types.grab_types.Add("COS_P_BROWNIE");
            Types.grab_types.Add("COS_P_PINKPIG");
            Types.grab_types.Add("COS_P_GOLDPIG");
            Types.grab_types.Add("COS_P_FOX");
            //Grab Pets

            Types.attack_types = new List<string>();
            //Attack Pets
            Types.attack_types.Add("COS_P_BEAR");
            Types.attack_types.Add("COS_P_FOX");
            Types.attack_types.Add("COS_P_PENGUIN");
            Types.attack_types.Add("COS_P_WOLF_WHITE_SMALL");
            Types.attack_types.Add("COS_P_WOLF_WHITE");
            Types.attack_types.Add("COS_P_WOLF");
            //Attack Pets

            //Attack Pets Item
            Types.attack_spawn_types = new List<string>();
            Types.attack_spawn_types.Add("ITEM_COS_P_FOX_SCROLL");
            Types.attack_spawn_types.Add("ITEM_COS_P_BEAR_SCROLL");
            Types.attack_spawn_types.Add("ITEM_COS_P_FLUTE");
            Types.attack_spawn_types.Add("ITEM_COS_P_FLUTE_SILK");
            Types.attack_spawn_types.Add("ITEM_COS_P_FLUTE_WHITE");
            Types.attack_spawn_types.Add("ITEM_COS_P_FLUTE_WHITE_SMALL");
            Types.attack_spawn_types.Add("ITEM_COS_P_PENGUIN_SCROLL");
            //Attack Pets Item

            //Grab Pets Item
            Types.grabpet_spawn_types = new List<string>();
            Types.grabpet_spawn_types.Add("ITEM_COS_P_SPOT_RABBIT_SCROLL");
            Types.grabpet_spawn_types.Add("ITEM_COS_P_RABBIT_SCROLL");
            Types.grabpet_spawn_types.Add("ITEM_COS_P_RABBIT_SCROLL_SILK");
            Types.grabpet_spawn_types.Add("ITEM_COS_P_GGLIDER_SCROLL");
            Types.grabpet_spawn_types.Add("ITEM_COS_P_MYOWON_SCROLL");
            Types.grabpet_spawn_types.Add("ITEM_COS_P_SEOWON_SCROLL");
            Types.grabpet_spawn_types.Add("ITEM_COS_P_RACCOONDOG_SCROLL");
            Types.grabpet_spawn_types.Add("ITEM_COS_P_BROWNIE_SCROLL");
            Types.grabpet_spawn_types.Add("ITEM_COS_P_CAT_SCROLL");
            Types.grabpet_spawn_types.Add("ITEM_COS_P_PINKPIG_SCROLL");
            Types.grabpet_spawn_types.Add("ITEM_COS_P_GOLDPIG_SCROLL");
            Types.grabpet_spawn_types.Add("ITEM_COS_P_GOLDPIG_SCROLL_SILK");
            //Grab Pets Item
        }
    }
}
