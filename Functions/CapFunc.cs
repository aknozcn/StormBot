using SilkroadSecurityApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StormBot.Functions
{
    class CapFunc
    {
        public static void Cap_Parse(Packet packet)
        {
            try
            {
                String CapperName = string.Empty;
                uint UUID = 0;
                packet.ReadUInt8();
                uint CapperUID = packet.ReadUInt32();
                byte TypeState = packet.ReadUInt8();  // 0=Cap CLose | 1= RedCap | 2=BlackCap | 3=BlueCap | 4=WHiteCap | 5=YellowCap
              
                foreach (string Char in CharStrings.CharNameANDuniqueID) // Loop through List with foreach.
                {
                    string[] txt = Char.Split(',');
                    CapperName = txt[0];
                    UUID = Convert.ToUInt32(txt[1]);
                    if (CapperUID == UUID)
                    {

                        if ((Program.lms_status == 1) && (Program.lms_recalledplayers.IndexOf(CapperName) != -1))
                        {
                            switch (TypeState)
                            {
                                case 0:
                                    Program.m_proxy.SendPM(CapperName, "Maç başlamadan önce cape çıkaramazsın.");
                                    Program.m_proxy.PlayertoTown(CapperName);
                                    Program.m_proxy.LMS_REMOVE_CHEATER(CapperName);
                                    Program.event_db.ExecuteCommand("Delete From [EventBot].[dbo].[_LMSPlayers] Where CharName like '" + CapperName + "'");
                                    break;
                                case 1:
                                    Program.m_proxy.SendPM(CapperName, "Sadece sarı cape!");
                                    Program.m_proxy.PlayertoTown(CapperName);
                                    Program.m_proxy.LMS_REMOVE_CHEATER(CapperName);
                                    Program.event_db.ExecuteCommand("Delete From [EventBot].[dbo].[_LMSPlayers] Where CharName like '" + CapperName + "'");
                                    break;
                                case 2:
                                    Program.m_proxy.SendPM(CapperName, "Sadece sarı cape!");
                                    Program.m_proxy.PlayertoTown(CapperName);
                                    Program.m_proxy.LMS_REMOVE_CHEATER(CapperName);
                                    Program.event_db.ExecuteCommand("Delete From [EventBot].[dbo].[_LMSPlayers] Where CharName like '" + CapperName + "'");
                                    break;
                                case 3:
                                    Program.m_proxy.SendPM(CapperName, "Sadece sarı cape!");
                                    Program.m_proxy.PlayertoTown(CapperName);
                                    Program.m_proxy.LMS_REMOVE_CHEATER(CapperName);
                                    Program.event_db.ExecuteCommand("Delete From [EventBot].[dbo].[_LMSPlayers] Where CharName like '" + CapperName + "'");
                                    break;
                                case 4:
                                    Program.m_proxy.SendPM(CapperName, "Sadece sarı cape!");
                                    Program.m_proxy.PlayertoTown(CapperName);
                                    Program.m_proxy.LMS_REMOVE_CHEATER(CapperName);
                                    Program.event_db.ExecuteCommand("Delete From [EventBot].[dbo].[_LMSPlayers] Where CharName like '" + CapperName + "'");
                                    break;
                                case 5:
                                    Program.m_proxy.SendPM(CapperName, "Onlar seni öldürmeden sen onları öldür!");
                                    Program.event_db.ExecuteCommand("INSERT INTO [EventBot].[dbo].[_LMSPlayers] (Service,CharName) VALUES (1,'" + CapperName + "')");
                                    break;
                            }
                        }
                    }
                }
            }
            catch
            {
                //  Console.WriteLine("Parsing Cap Error");
            }
        }

    }
}
