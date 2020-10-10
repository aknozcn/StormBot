using SilkroadSecurityApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StormBot.Functions
{
    class ExcParse
    {
        public static void EXC_parse(Packet packet)
        {
            try
            {
                byte ExchangeType = packet.ReadUInt8();
                uint ExchangerUniqueID = packet.ReadUInt32();
                string ExchangerName;
                if (ExchangeType == 1)
                {
                    foreach (string Char in CharStrings.CharNameANDuniqueID) // Loop through List with foreach.
                    {
                        //Console.WriteLine(Char);
                        string[] txt = Char.Split(',');
                        ExchangerName = txt[0];
                        uint UUID = Convert.ToUInt32(txt[1]);
                        if (ExchangerUniqueID == UUID)
                        {
                            if (Program.kayipgm_status == 1)
                            {
                                if (Program.main.cbox_kayipgm_silk.Checked)
                                {
                                    if (Program.main.kayipgm_silk.Text != "" || Program.main.kayipgm_silkpoint.Text != "" || Program.main.kayipgm_giftsilk.Text != "")
                                    {
                                        Program.rewards.GiveSilk(ExchangerName, Convert.ToInt32(Program.main.kayipgm_silk.Text), Convert.ToInt32(Program.main.kayipgm_giftsilk.Text), Convert.ToInt32(Program.main.kayipgm_silkpoint.Text));
                                    }
                                }

                                if (Program.main.cbox_kayipgm_item.Checked)
                                {
                                    if (Program.main.kayipgm_itemid.Text != "" || Program.main.kayipgm_quantity.Text != "" || Program.main.kayipgm_plus.Text != "")
                                    {
                                        Program.rewards.GiveItem(ExchangerName, Convert.ToInt32(Program.main.kayipgm_itemid.Text), Convert.ToInt32(Program.main.kayipgm_quantity.Text), Convert.ToInt32(Program.main.kayipgm_plus.Text));
                                    }
                                }

                                if (Program.main.cbox_kayipgm_gold.Checked)
                                {
                                    if (Program.main.kayipgm_gold.Text != "")
                                    {
                                        Program.rewards.GiveGold(ExchangerName, Convert.ToInt32(Program.main.kayipgm_gold.Text));
                                    }
                                }

                                if (Program.main.cbox_kayipgm_title.Checked)
                                {
                                    if (Program.main.kayipgm_title.Text != "")
                                    {
                                        Program.rewards.GiveTitle(ExchangerName, Convert.ToInt32(Program.main.kayipgm_title.Text));
                                    }
                                }
                                String WinNotic = Program.kayipgm_win.Replace("%name%", ExchangerName);
                                Program.m_proxy.SendNotice(WinNotic);
                                Program.kayipgm_status = 0;
                                Program.HaveEvent = false;
                                Program.m_proxy.Gotown();
                            }
                        }
                    }
                }
            }
            catch
            {
                //Console.WriteLine("Error parse Exchange");
            }
        }
    }
}
