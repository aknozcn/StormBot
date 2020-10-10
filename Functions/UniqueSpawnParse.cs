using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SilkroadSecurityApi;

namespace StormBot.Functions
{
    class UniqueSpawnParse
    {
        public void unique_parse(Packet packet)
        {
            try
            {
                uint SpawnFunc = packet.ReadUInt16();
                if (SpawnFunc.ToString("X4") == "0C05") // Unique Appear
                {
                    uint MOBID = packet.ReadUInt32();

                }
                else if (SpawnFunc.ToString("X4") == "0C06") // Unique Dead
                {
                    uint MOBID = packet.ReadUInt32();
                    string PTcharname = packet.ReadAscii();

                    int i = Convert.ToInt32(MOBID);
                    if (Program.uniquespawn_status == 1)
                    {
                        Program.mobid = MOBID;
                        Program.m_proxy.UniqueSpawnEvent();
                        Program.event_db.ExecuteCommand("exec _UniqueEventRewards " + MOBID + ", '" + PTcharname + "'");
                    }

                    if ((Program.snd_mobid == i) && (Program.snd_status == 1)) // Event
                    {
                        string winnotice = Program.snd_win_notice.Replace("%player%", PTcharname);
                        Program.m_proxy.SendNotice(winnotice);

                        if (Program.main.cbox_snd_silk.Checked)
                        {
                            if (Program.main.snd_silk.Text != "" || Program.main.snd_silkpoint.Text != "" || Program.main.snd_giftsilk.Text != "")
                            {
                                Program.rewards.GiveSilk(PTcharname, Convert.ToInt32(Program.main.snd_silk.Text), Convert.ToInt32(Program.main.snd_giftsilk.Text), Convert.ToInt32(Program.main.snd_silkpoint.Text));
                            }
                        }

                        if (Program.main.cbox_snd_item.Checked)
                        {
                            if (Program.main.snd_itemid.Text != "" || Program.main.snd_quantity.Text != "" || Program.main.snd_plus.Text != "")
                            {
                                Program.rewards.GiveItem(PTcharname, Convert.ToInt32(Program.main.snd_itemid.Text), Convert.ToInt32(Program.main.snd_quantity.Text), Convert.ToInt32(Program.main.snd_plus.Text));
                            }
                        }

                        if (Program.main.cbox_snd_gold.Checked)
                        {
                            if (Program.main.snd_gold.Text != "")
                            {
                                Program.rewards.GiveGold(PTcharname, Convert.ToInt32(Program.main.snd_gold.Text));
                            }
                        }

                        if (Program.main.cbox_snd_title.Checked)
                        {
                            if (Program.main.snd_title.Text != "")
                            {
                                Program.rewards.GiveTitle(PTcharname, Convert.ToInt32(Program.main.snd_title.Text));
                            }
                        }
                        Logger.LogIt("SnD Ödülleri verildi.", LogType.Normal);
                        Program.snd_mobid = 0;
                        Program.snd_status = 0;
                        Program.m_proxy.Gotown();
                        Program.HaveEvent = false;
                    }
                }
            }

            catch
            {

            }

        }
    }
}
