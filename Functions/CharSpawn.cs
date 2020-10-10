using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SilkroadSecurityApi;

namespace StormBot.Functions
{
    class CharSpawn
    {
        public static List<string> explist = new List<string>();
        public static int Level;
        public static int maxLevel;
        public static ulong exp;
        public static ulong expmax;
        public static ulong Gold;
        public static uint SkillPoints;
        public static uint AvailableStatPoints;
        public static byte Zerk;
        public static uint CurrentHP;
        public static uint CurrentMP;
        public static byte itemscount;
        public static byte inventoryslot;
        public static byte questspending;
        public static ushort questscompleted;
        public static uint ID;
        public static uint AccountID;
        public static int X;
        public static int Y;
        public static string PlayerName;
        public static string guildName;
        public static ushort STR;
        public static ushort INT;
        public static uint MaxHP;
        public static uint MaxMP;
        public static uint some;
        public static uint model;
        public static byte volh;
        public static byte data_loaded = 0;

        public static bool groupspawnIsSpawn = false;
        public static int spawnAmount = 0;
        public static bool groupspawn = false;

        public static void parseSpawn(Packet packet)
        {
            try
            {
                CharStrings.GlobalsTypeSlot.Clear();
                #region Main
                CharSpawn.some = packet.ReadUInt32(); // ServerTime 
                CharSpawn.model = packet.ReadUInt32(); //Model
                CharSpawn.volh = packet.ReadUInt8(); //Volume and Height - scale -
                CharStrings.Level = packet.ReadUInt8(); // CurLevel
                CharStrings.MaxLevel = packet.ReadUInt8(); //MaxLevel
                int maxlvl = (int)CharStrings.Level - 1;
                CharStrings.Exp = packet.ReadUInt64(); // ExpOffset
                packet.ReadUInt32(); //SP bar SExpOffset
                CharStrings.Gold = packet.ReadUInt64(); //RemainGold
                CharStrings.SkillPoints = packet.ReadUInt32(); //RemainSkillPoint
                CharStrings.StatPoints = packet.ReadUInt16(); //RemainStatPoint
                packet.ReadUInt8();//Zerk?!! RemainHwanCount
                packet.ReadUInt32(); // GatheredExpPoint
                CharStrings.CurrentHP = packet.ReadUInt32();
                CharStrings.CurrentMP = packet.ReadUInt32();
                packet.ReadUInt8(); //AutoInverstExp
                packet.ReadUInt8(); //DailyPK
                packet.ReadUInt16(); //TotalPK
                packet.ReadUInt32(); //PKPenaltyPoint
                packet.ReadUInt8(); //HwanLevel
                packet.ReadUInt8(); //FreePVP           //0 = None, 1 = Red, 2 = Gray, 3 = Blue, 4 = White, 5 = Gold

                #endregion
                #region Items
                CharSpawn.inventoryslot = packet.ReadUInt8(); // Inventory.Size
                CharSpawn.itemscount = packet.ReadUInt8();  // Inventory.ItemCount
                for (int y = 0; y < CharSpawn.itemscount; y++)
                {
                    byte slot = packet.ReadUInt8(); //  item.Slot
                    uint RentType = packet.ReadUInt32(); // item.RentType

                    uint item_id = packet.ReadUInt32();
                    int index = Items_Info.itemsidlist.IndexOf(item_id);
                    if (index > -1)
                    {
                        string type = Items_Info.itemstypelist[index];
                        string name = Items_Info.itemsnamelist[index];
                        CharStrings.inventoryslot.Add(slot);
                        CharStrings.inventorytype.Add(type);
                        CharStrings.inventoryid.Add(item_id);

                        if (type.StartsWith("ITEM_CH") || type.StartsWith("ITEM_ROC_CH") || type.StartsWith("ITEM_ROC_EU") || type.StartsWith("ITEM_EU") || type.StartsWith("ITEM_MALL_AVATAR") || type.StartsWith("ITEM_ETC_E060529_GOLDDRAGONFLAG") || type.StartsWith("ITEM_EVENT_CH") || type.StartsWith("ITEM_EVENT_EU") || type.StartsWith("ITEM_EVENT_AVATAR_W_NASRUN") || type.StartsWith("ITEM_EVENT_AVATAR_M_NASRUN"))
                        {
                            byte item_plus = packet.ReadUInt8();
                            packet.ReadUInt64();
                            CharStrings.inventorydurability.Add(packet.ReadUInt32());
                            byte blueamm = packet.ReadUInt8();
                            for (int i = 0; i < blueamm; i++)
                            {
                                packet.ReadUInt32();
                                packet.ReadUInt32();
                            }
                            packet.ReadUInt8(); //Unknwon
                            packet.ReadUInt8(); //Unknwon
                            packet.ReadUInt8(); //Unknwon
                            byte flag1 = packet.ReadUInt8(); // Flag ?
                            if (flag1 == 1)
                            {
                                packet.ReadUInt8(); //Unknown
                                packet.ReadUInt32(); // Unknown ID ? ADV Elexir ID ?
                                packet.ReadUInt32(); // Unknwon Count
                            }
                            CharStrings.inventorycount.Add(1);
                        }
                        else if ((type.StartsWith("ITEM_COS") && type.Contains("SILK")) || (type.StartsWith("ITEM_EVENT_COS") && !type.Contains("_C_")))
                        {
                            // orginial mn 8er De wala Else
                            if (Globals.Types.grabpet_spawn_types.IndexOf(type) != -1 || Globals.Types.attack_spawn_types.IndexOf(type) != -1)
                            {
                                byte flag = packet.ReadUInt8();
                                if (flag == 2 || flag == 3 || flag == 4)
                                {
                                    packet.ReadUInt32(); //Model
                                    packet.ReadAscii();
                                    if (Globals.Types.attack_spawn_types.IndexOf(type) == -1)
                                    {
                                        packet.ReadUInt32();
                                    }
                                    packet.ReadUInt8();
                                }
                                CharStrings.inventorycount.Add(1);
                                CharStrings.inventorydurability.Add(0);

                            }
                            else
                            {
                                byte flag = packet.ReadUInt8();
                                if (flag == 2 || flag == 3 || flag == 4)
                                {
                                    packet.ReadUInt32(); //Model
                                    packet.ReadAscii();
                                    packet.ReadUInt8();
                                    if (Globals.Types.attack_spawn_types.IndexOf(type) == -1)
                                    {
                                        packet.ReadUInt32();
                                    }
                                }
                                CharStrings.inventorycount.Add(1);
                                CharStrings.inventorydurability.Add(0);
                            }
                        }
                        else if (type == "ITEM_ETC_TRANS_MONSTER")
                        {
                            packet.ReadUInt32();
                            CharStrings.inventorycount.Add(1);
                            CharStrings.inventorydurability.Add(0);
                        }
                        else if (type.StartsWith("ITEM_MALL_MAGIC_CUBE")) //
                        {
                            packet.ReadUInt32();
                            CharStrings.inventorycount.Add(1);
                            CharStrings.inventorydurability.Add(0);
                        }
                        else
                        {
                            ushort count = packet.ReadUInt16();
                            if (type.Contains("ITEM_ETC_ARCHEMY_ATTRSTONE")) // || type.Contains("ITEM_ETC_ARCHEMY_MAGICSTONE"))
                            {
                                packet.ReadUInt8();
                            }
                            CharStrings.inventorycount.Add(count);
                            if (type == "ITEM_EVENT_RENT_GLOBAL_CHATTING")
                            {
                                CharStrings.GlobalsTypeSlot.Add(type + "," + count + "," + slot);
                            }
                            CharStrings.inventorydurability.Add(0);
                        }
                    }
                }

                if (CharStrings.GlobalsTypeSlot.Count > 0)
                {
                    foreach (string TTType in CharStrings.GlobalsTypeSlot) // Loop through List with foreach.
                    {
                        Logger.LogIt("Inventory Globals : " + TTType, LogType.Normal);
                    }
                }
                else
                {
                    Logger.LogIt("Warrning : Bot Charachter Dosent Have ITEM_EVENT_RENT_GLOBAL_CHATTING", LogType.Hata);

                }
                #endregion

                #region Avatars
                packet.ReadUInt8(); // AvatarInventory.Size
                int avatarcount = packet.ReadUInt8(); // AvatarInventory.ItemCount
                for (int i = 0; i < avatarcount; i++)
                {
                    packet.ReadUInt8(); //Slot
                    uint AvatarRentType = packet.ReadUInt32(); // item.RentType
                    int index = Items_Info.itemsidlist.IndexOf(AvatarRentType);
                    if (index > -1)
                    {
                        string type = Items_Info.itemstypelist[index];
                    }

                    byte item_plus = packet.ReadUInt8();
                    packet.ReadUInt64();
                    packet.ReadUInt32();
                    byte blueamm = packet.ReadUInt8();
                    for (int a = 0; a < blueamm; a++)
                    {
                        packet.ReadUInt32();
                        packet.ReadUInt32();
                    }
                    packet.ReadUInt32();
                }
                #endregion
                packet.ReadUInt8(); //Avatars End


                packet.ReadUInt8();//
                packet.ReadUInt8();//Unknown
                packet.ReadUInt8();//


                int mastery = packet.ReadUInt8(); // Mastery Start
                while (mastery == 1)
                {
                    packet.ReadUInt32(); // Mastery ID
                    packet.ReadUInt8();  // Mastery LV
                    mastery = packet.ReadUInt8(); // New Mastery Start / List End
                }
                packet.ReadUInt8(); // Mastery END

                uint skilllist = packet.ReadUInt8(); // Skill List Start
                while (skilllist == 1)
                {

                    uint skillid = packet.ReadUInt32(); // Skill ID
                    packet.ReadUInt8();
                    skilllist = packet.ReadUInt8(); // New Skill Start / List End
                }

                #region Skipping Quest Part

                Packet charid = new Packet(3020);
                charid.WriteUInt32(CharStrings.UniqueID);
                charid.Lock();
                byte idpart1 = charid.ReadUInt8();
                byte idpart2 = charid.ReadUInt8();
                byte idpart3 = charid.ReadUInt8();
                byte idpart4 = charid.ReadUInt8();
                while (true)
                {
                    if (packet.ReadUInt8() == idpart1)
                    {
                        if (packet.ReadUInt8() == idpart2)
                        {
                            if (packet.ReadUInt8() == idpart3)
                            {
                                if (packet.ReadUInt8() == idpart4)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
                #endregion

                byte xsec = packet.ReadUInt8();
                byte ysec = packet.ReadUInt8();

                float xcoord = packet.ReadSingle();
                float zcoord = packet.ReadSingle();
                float ycoord = packet.ReadSingle();

                packet.ReadUInt16(); // Position
                int move = packet.ReadUInt8(); // Move ?? Maybie Useless
                packet.ReadUInt8(); // Run
                packet.ReadUInt8();
                packet.ReadUInt16();
                packet.ReadUInt8();
                packet.ReadUInt8(); //DeathFlag
                packet.ReadUInt8(); //Movement Flag
                packet.ReadUInt8(); //Berserker Flag
                //Char.WalkSpeed = packet.ReadUInt32(); //Walking Speed
                //Char.RunSpeed = packet.ReadUInt32(); //Running Speed
                CharStrings.WalkSpeed = packet.ReadSingle() * 1.1f; //Walking Speed
                CharStrings.RunSpeed = packet.ReadSingle() * 1.1f; //Running Speed
                CharStrings.ZerkSpeed = packet.ReadUInt32() * 1.1f; //Berserker Speed
                packet.ReadUInt8();
                CharStrings.PlayerName = packet.ReadAscii();
            }
            catch (Exception ex)
            {
                Logger.LogIt("Error Parsing CharSpawn Packet " + ex.ToString(), LogType.Hata);
            }
        }


        public static void START_ParseChar(Packet packet)
        {
            try
            {
                uint model = packet.ReadUInt32();
                int index = Mobs_Info.mobsidlist.IndexOf(model);

                if (index != -1)
                {
                    if (Mobs_Info.mobstypelist[index].StartsWith("CHAR"))
                    {
                        Thread t = new Thread(() => ParseChar(packet, index));
                        t.Start();
                    }
                }
            }
            catch { }
        }
        public static void ParseChar(Packet packet, int index) // 3015 single spawn
        {
            string name = string.Empty;
            uint UniqueID = 0;
            try
            {
                int trade = 0;
                int stall = 0;
                packet.ReadUInt8(); // Volume/Height scale
                packet.ReadUInt8(); // HwanLevel
                packet.ReadUInt8(); //PVPCape           //0 = None, 1 = Red, 2 = Gray, 3 = Blue, 4 = White, 5 = Orange
                packet.ReadUInt8(); // AutoInverstExp  //1 = Beginner Icon, 2 = Helpful, 3 = Beginner & Helpful
                packet.ReadUInt8(); // Max Slots Inventory.Size
                int items_count = packet.ReadUInt8(); //Inventory.ItemCount
                for (int a = 0; a < items_count; a++)
                {
                    uint itemid = packet.ReadUInt32(); //item.RefItemID

                    int itemindex = Items_Info.itemsidlist.IndexOf(itemid);
                    if (Items_Info.itemstypelist[itemindex].StartsWith("ITEM_CH") || Items_Info.itemstypelist[itemindex].StartsWith("ITEM_EU") || Items_Info.itemstypelist[itemindex].StartsWith("ITEM_FORT") || Items_Info.itemstypelist[itemindex].StartsWith("ITEM_ROC_CH") || Items_Info.itemstypelist[itemindex].StartsWith("ITEM_ROC_EU"))
                    {
                        byte plus = packet.ReadUInt8(); // Item Plus
                    }
                    if (Items_Info.itemstypelist[itemindex].StartsWith("ITEM_EU_M_TRADE") || Items_Info.itemstypelist[itemindex].StartsWith("ITEM_EU_F_TRADE") || Items_Info.itemstypelist[itemindex].StartsWith("ITEM_CH_M_TRADE") || Items_Info.itemstypelist[itemindex].StartsWith("ITEM_CH_W_TRADE"))
                    {
                        trade = 1;
                    }
                }

                packet.ReadUInt8(); // Max Avatars Slot Inventory.Size
                int avatar_count = packet.ReadUInt8(); //Inventory.ItemCount
                for (int a = 0; a < avatar_count; a++)
                {
                    uint avatarid = packet.ReadUInt32(); // item.RefItemID
                    int avatarindex = Items_Info.itemsidlist.IndexOf(avatarid);
                    byte plus = packet.ReadUInt8();// Avatar Plus
                    if (avatarindex == -1)
                    {
                    }
                }

                int mask = packet.ReadUInt8(); //HasMask
                if (mask == 1)
                {
                    uint id = packet.ReadUInt32(); //mask.RefObjID
                    string type = Mobs_Info.mobstypelist[Mobs_Info.mobsidlist.IndexOf(id)];
                    if (type.StartsWith("CHAR"))
                    {
                        packet.ReadUInt8(); //Mask.Scale
                        byte count = packet.ReadUInt8(); //Mask.ItemCount
                        for (int i = 0; i < count; i++)
                        {
                            packet.ReadUInt32(); //item.RefItemID
                        }
                    }
                }

                //uint 
                UniqueID = packet.ReadUInt32(); // Char Unique Spawn ID
                byte xsec = packet.ReadUInt8(); //Position.RegionID
                byte ysec = packet.ReadUInt8();
                float xcoord = packet.ReadSingle();
                packet.ReadSingle();
                float ycoord = packet.ReadSingle();
                packet.ReadUInt16(); // Position

                byte move = packet.ReadUInt8(); // Moving  //  Movement.HasDestination
                packet.ReadUInt8(); // Running  // Movement.Type

                if (move == 1)
                {
                    xsec = packet.ReadUInt8();
                    ysec = packet.ReadUInt8();
                    if (ysec == 0x80)
                    {
                        xcoord = packet.ReadUInt16() - packet.ReadUInt16();
                        packet.ReadUInt16();
                        packet.ReadUInt16();
                        ycoord = packet.ReadUInt16() - packet.ReadUInt16();
                    }
                    else
                    {
                        xcoord = packet.ReadUInt16();
                        packet.ReadUInt16();
                        ycoord = packet.ReadUInt16();
                    }
                }
                else
                {
                    packet.ReadUInt8(); // No Destination
                    packet.ReadUInt16(); ; // Angle
                }

                packet.ReadUInt8(); // Alive
                packet.ReadUInt8(); // Unknown
                packet.ReadUInt8(); // Unknown
                packet.ReadUInt8(); // Unknown

                packet.ReadUInt32(); // Walking speed
                packet.ReadUInt32(); // Running speed
                packet.ReadUInt32(); // Berserk speed

                int active_skills = packet.ReadUInt8(); // Buffs count

                for (int a = 0; a < active_skills; a++)
                {
                    uint skillid = packet.ReadUInt32();
                    int buffindex = Skills_Info.skillsidlist.IndexOf(skillid);
                    string type = Skills_Info.skillstypelist[buffindex];
                    packet.ReadUInt32(); // Temp ID
                    if (type.StartsWith("SKILL_EU_CLERIC_RECOVERYA_GROUP") || type.StartsWith("SKILL_EU_BARD_BATTLAA_GUARD") || type.StartsWith("SKILL_EU_BARD_DANCEA") || type.StartsWith("SKILL_EU_BARD_SPEEDUPA_HITRATE"))
                    {
                        packet.ReadUInt8();
                    }
                }

                name = packet.ReadAscii();
#if DEBUG
                Logger.LogIt("Player Arround Unique ID: " + UniqueID + " Player Name: " + name, LogType.Normal);
#endif
                for (int i = 0; i < CharStrings.CharNameANDuniqueID.Count; i++) // Loop through List with foreach.
                {
                    if (CharStrings.CharNameANDuniqueID[i].StartsWith(name))
                    {
                        int AlreadyIndex = CharStrings.CharNameANDuniqueID.IndexOf(CharStrings.CharNameANDuniqueID[i]);
                        CharStrings.CharNameANDuniqueID.RemoveAt(AlreadyIndex);
                    }
                }
                CharStrings.CharNameANDuniqueID.Add(name + "," + UniqueID.ToString());
            }
            catch (Exception ex)
            {
                Logger.LogIt("Error Spawn Unique ID: " + UniqueID + " Player Name: " + name, LogType.Hata);
                Logger.LogIt(string.Format("Spawn", ex.Message + " Count: " + packet), LogType.Hata);
            }
        }

        public static void begin(Packet packet)
        {
            try
            {
                byte action = packet.ReadUInt8();
                if (action == 1)
                {
                    groupspawnIsSpawn = true;//spawn
                }
                else if (action == 2)
                {
                    groupspawnIsSpawn = false;//despawn
                }
                spawnAmount = (int)packet.ReadUInt16();
                groupspawn = true;
            }
            catch { Logger.LogIt("Error Start Record Amount Of Group Spawn ", LogType.Hata); }
        }

        public static void end(Packet packet)
        {
            groupspawn = false;
        }

        public static void GroupeSpawn(Packet packet)
        {
            try
            {
                #region DetectType
                uint model = packet.ReadUInt32();
                int index = Mobs_Info.mobsidlist.IndexOf(model);
                int itemsindex = Items_Info.itemsidlist.IndexOf(model);
                #endregion

                if (itemsindex != -1)
                {
                    #region ItemsParsing
                    #endregion
                }
                if (index != -1)
                {
                    #region PetsParsing
                    if (Mobs_Info.mobstypelist[index].StartsWith("COS"))
                    { }
                    #endregion
                    #region NPCParsing
                    else if (Mobs_Info.mobstypelist[index].StartsWith("NPC"))
                    { }
                    #endregion
                    #region CharParsing
                    else if (Mobs_Info.mobstypelist[index].StartsWith("CHAR"))
                    {
                        Thread t = new Thread(() => CharSpawn.ParseChar(packet, index));
                        t.Start();
                    }
                    #endregion
                    #region MobsParsing
                    else if (Mobs_Info.mobstypelist[index].StartsWith("MOB"))
                    {
                    }
                    #endregion
                    #region PortalParsing
                    else if (Mobs_Info.mobstypelist[index].Contains("_GATE"))
                    { }
                    #endregion
                    #region OtherParsing
                    else
                    { }
                    #endregion
                }
            }
            catch { Logger.LogIt("Error Parse group Spawn", LogType.Hata); }

        }
    }
}
