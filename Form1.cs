using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using SilkroadSecurityApi;
using StormBot.Forms;
using StormBot.Functions;

namespace StormBot
{
    public partial class Form1 : Form
    {
        AddTime frm;
        public System.Threading.Timer t_Zerk;

        //Proxy Program.m_proxy;
        public Form1()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            //Program.m_proxy = Program.proxy;
            //Program.m_proxy = new Proxy();
            //Program.m_proxy.Log += Program.m_proxy_Log;
            Program.m_proxy.Listen();
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern Int32 SendMessage(IntPtr hWnd, int msg, int wParam, [MarshalAs(UnmanagedType.LPWStr)]
string lParam);

        private void Form1_Load(object sender, EventArgs e)
        {
            //Mssql
            Program.mssql_name = Convert.ToString(Program.ini.GetValue("mssql", "address"));
            Program.mssql_acc = Convert.ToString(Program.ini.GetValue("mssql", "username"));
            Program.mssql_pw = Convert.ToString(Program.ini.GetValue("mssql", "password"));
            Program.account_database = Convert.ToString(Program.ini.GetValue("mssql", "account_db"));
            Program.log_database = Convert.ToString(Program.ini.GetValue("mssql", "log_DB"));
            Program.shard_database = Convert.ToString(Program.ini.GetValue("mssql", "shard_db"));
            Program.event_database = Convert.ToString(Program.ini.GetValue("mssql", "event_db"));
            Program.acc_db = new MSSQL(Program.mssql_name, Program.account_database, Program.mssql_acc, Program.mssql_pw, true);
            Program.log_db = new MSSQL(Program.mssql_name, Program.log_database, Program.mssql_acc, Program.mssql_pw, true);
            Program.shard_db = new MSSQL(Program.mssql_name, Program.shard_database, Program.mssql_acc, Program.mssql_pw, true);
            Program.event_db = new MSSQL(Program.mssql_name, Program.event_database, Program.mssql_acc, Program.mssql_pw, true);

            //Events
            toggle_trivia.Checked = Convert.ToBoolean(Program.ini.GetValue("events", "trivia_enable"));
            toggle_lpn.Checked = Convert.ToBoolean(Program.ini.GetValue("events", "lpn_enable"));
            lpn_wait.Text = Convert.ToString(Program.ini.GetValue("events", "lpn_wait"));
            lpn_min.Text = Convert.ToString(Program.ini.GetValue("events", "lpn_min_no"));
            lpn_max.Text = Convert.ToString(Program.ini.GetValue("events", "lpn_max_no"));
            Program.trivia_start = Convert.ToString(Program.ini.GetValue("notices", "trivia_start"));
            Program.trivia_end = Convert.ToString(Program.ini.GetValue("notices", "trivia_end"));
            Program.trivia_win = Convert.ToString(Program.ini.GetValue("notices", "trivia_win"));
            Program.trivia_wrong = Convert.ToString(Program.ini.GetValue("notices", "trivia_wrong"));
            Program.trivia_wrong2 = Convert.ToString(Program.ini.GetValue("notices", "trivia_wrong2"));
            Program.lpn_start_notice = Convert.ToString(Program.ini.GetValue("notices", "lpn_start_notice"));
            Program.lpn_win_notice = Convert.ToString(Program.ini.GetValue("notices", "lpn_win_notice"));
            Program.lpn_nowin_notice = Convert.ToString(Program.ini.GetValue("notices", "lpn_nowin_notice"));

            tbox_zerkmob.Text = Convert.ToString(Program.ini.GetValue("events", "zerkmobid"));
            tbox_zerkadet.Text = Convert.ToString(Program.ini.GetValue("events", "zerkmobadet"));
            tbox_zerksaniye.Text = Convert.ToString(Program.ini.GetValue("events", "zerksaniye"));
            Program.lms_lvllimit = Convert.ToInt32(Program.ini.GetValue("events", "lms_level"));

            s_ip.Text = Convert.ToString(Program.ini.GetValue("clientless", "ip"));
            s_port.Text = Convert.ToString(Program.ini.GetValue("clientless", "port"));
            s_version.Text = Convert.ToString(Program.ini.GetValue("clientless", "version"));
            s_locale.Text = Convert.ToString(Program.ini.GetValue("clientless", "locale"));
            tbUsername.Text = Convert.ToString(Program.ini.GetValue("clientless", "id"));
            tbPassword.Text = Convert.ToString(Program.ini.GetValue("clientless", "pw"));
            tbCaptcha.Text = Convert.ToString(Program.ini.GetValue("clientless", "captcha"));
            Program.chat_log = Convert.ToBoolean(Program.ini.GetValue("clientless", "chat_log"));

            #region Rewards
            //Rewards
            #region TriviaCheckBoxs
            if (Convert.ToBoolean(Program.ini_rewards.GetValue("trivia", "silk_enable")))
            {
                cbox_trivia_silk.Checked = true;
            }
            else
            {
                cbox_trivia_silk.Checked = false;
            }
            if (Convert.ToBoolean(Program.ini_rewards.GetValue("trivia", "gold_enable")))
            {
                cbox_trivia_gold.Checked = true;
            }
            else
            {
                cbox_trivia_gold.Checked = false;
            }
            if (Convert.ToBoolean(Program.ini_rewards.GetValue("trivia", "item_enable")))
            {
                cbox_trivia_item.Checked = true;
            }
            else
            {
                cbox_trivia_item.Checked = false;
            }
            if (Convert.ToBoolean(Program.ini_rewards.GetValue("trivia", "title_enable")))
            {
                cbox_trivia_title.Checked = true;
            }
            else
            {
                cbox_trivia_title.Checked = false;
            }

            #endregion

            #region TriviaTextBoxs
            trivia_silk.Text = Convert.ToString(Program.ini_rewards.GetValue("trivia", "silk"));
            trivia_silkpoint.Text = Convert.ToString(Program.ini_rewards.GetValue("trivia", "silkpoint"));
            trivia_giftsilk.Text = Convert.ToString(Program.ini_rewards.GetValue("trivia", "giftsilk"));

            trivia_itemid.Text = Convert.ToString(Program.ini_rewards.GetValue("trivia", "itemid"));
            trivia_quantity.Text = Convert.ToString(Program.ini_rewards.GetValue("trivia", "quantity"));
            trivia_plus.Text = Convert.ToString(Program.ini_rewards.GetValue("trivia", "plus"));

            trivia_gold.Text = Convert.ToString(Program.ini_rewards.GetValue("trivia", "gold"));

            trivia_title.Text = Convert.ToString(Program.ini_rewards.GetValue("trivia", "title"));
            #endregion

            #region LpnCheckBoxs
            if (Convert.ToBoolean(Program.ini_rewards.GetValue("lpn", "silk_enable")))
            {
                cbox_lpn_silk.Checked = true;
            }
            else
            {
                cbox_lpn_silk.Checked = false;
            }
            if (Convert.ToBoolean(Program.ini_rewards.GetValue("lpn", "gold_enable")))
            {
                cbox_lpn_gold.Checked = true;
            }
            else
            {
                cbox_lpn_gold.Checked = false;
            }
            if (Convert.ToBoolean(Program.ini_rewards.GetValue("lpn", "item_enable")))
            {
                cbox_lpn_item.Checked = true;
            }
            else
            {
                cbox_lpn_item.Checked = false;
            }
            if (Convert.ToBoolean(Program.ini_rewards.GetValue("lpn", "title_enable")))
            {
                cbox_lpn_title.Checked = true;
            }
            else
            {
                cbox_lpn_title.Checked = false;
            }

            #endregion

            #region LpnTextBoxs
            lpn_silk.Text = Convert.ToString(Program.ini_rewards.GetValue("lpn", "silk"));
            lpn_silkpoint.Text = Convert.ToString(Program.ini_rewards.GetValue("lpn", "silkpoint"));
            lpn_giftsilk.Text = Convert.ToString(Program.ini_rewards.GetValue("lpn", "giftsilk"));

            lpn_itemid.Text = Convert.ToString(Program.ini_rewards.GetValue("lpn", "itemid"));
            lpn_quantity.Text = Convert.ToString(Program.ini_rewards.GetValue("lpn", "quantity"));
            lpn_plus.Text = Convert.ToString(Program.ini_rewards.GetValue("lpn", "plus"));

            lpn_gold.Text = Convert.ToString(Program.ini_rewards.GetValue("lpn", "gold"));

            lpn_title.Text = Convert.ToString(Program.ini_rewards.GetValue("lpn", "title"));
            #endregion

            #region SndCheckBoxs
            if (Convert.ToBoolean(Program.ini_rewards.GetValue("snd", "silk_enable")))
            {
                cbox_snd_silk.Checked = true;
            }
            else
            {
                cbox_snd_silk.Checked = false;
            }
            if (Convert.ToBoolean(Program.ini_rewards.GetValue("snd", "gold_enable")))
            {
                cbox_snd_gold.Checked = true;
            }
            else
            {
                cbox_snd_gold.Checked = false;
            }
            if (Convert.ToBoolean(Program.ini_rewards.GetValue("snd", "item_enable")))
            {
                cbox_snd_item.Checked = true;
            }
            else
            {
                cbox_snd_item.Checked = false;
            }
            if (Convert.ToBoolean(Program.ini_rewards.GetValue("snd", "title_enable")))
            {
                cbox_snd_title.Checked = true;
            }
            else
            {
                cbox_snd_title.Checked = false;
            }

            #endregion

            #region SndTextBoxs
            snd_silk.Text = Convert.ToString(Program.ini_rewards.GetValue("snd", "silk"));
            snd_silkpoint.Text = Convert.ToString(Program.ini_rewards.GetValue("snd", "silkpoint"));
            snd_giftsilk.Text = Convert.ToString(Program.ini_rewards.GetValue("snd", "giftsilk"));

            snd_itemid.Text = Convert.ToString(Program.ini_rewards.GetValue("snd", "itemid"));
            snd_quantity.Text = Convert.ToString(Program.ini_rewards.GetValue("snd", "quantity"));
            snd_plus.Text = Convert.ToString(Program.ini_rewards.GetValue("snd", "plus"));

            snd_gold.Text = Convert.ToString(Program.ini_rewards.GetValue("snd", "gold"));

            snd_title.Text = Convert.ToString(Program.ini_rewards.GetValue("snd", "title"));
            #endregion

            #region HnSCheckBoxs
            if (Convert.ToBoolean(Program.ini_rewards.GetValue("hns", "silk_enable")))
            {
                cbox_kayipgm_silk.Checked = true;
            }
            else
            {
                cbox_kayipgm_silk.Checked = false;
            }
            if (Convert.ToBoolean(Program.ini_rewards.GetValue("hns", "gold_enable")))
            {
                cbox_kayipgm_gold.Checked = true;
            }
            else
            {
                cbox_kayipgm_gold.Checked = false;
            }
            if (Convert.ToBoolean(Program.ini_rewards.GetValue("hns", "item_enable")))
            {
                cbox_kayipgm_item.Checked = true;
            }
            else
            {
                cbox_kayipgm_item.Checked = false;
            }
            if (Convert.ToBoolean(Program.ini_rewards.GetValue("hns", "title_enable")))
            {
                cbox_kayipgm_title.Checked = true;
            }
            else
            {
                cbox_kayipgm_title.Checked = false;
            }

            #endregion

            #region HnSTextBoxs
            kayipgm_silk.Text = Convert.ToString(Program.ini_rewards.GetValue("hns", "silk"));
            kayipgm_silkpoint.Text = Convert.ToString(Program.ini_rewards.GetValue("hns", "silkpoint"));
            kayipgm_giftsilk.Text = Convert.ToString(Program.ini_rewards.GetValue("hns", "giftsilk"));

            kayipgm_itemid.Text = Convert.ToString(Program.ini_rewards.GetValue("hns", "itemid"));
            kayipgm_quantity.Text = Convert.ToString(Program.ini_rewards.GetValue("hns", "quantity"));
            kayipgm_plus.Text = Convert.ToString(Program.ini_rewards.GetValue("hns", "plus"));

            kayipgm_gold.Text = Convert.ToString(Program.ini_rewards.GetValue("hns", "gold"));

            kayipgm_title.Text = Convert.ToString(Program.ini_rewards.GetValue("hns", "title"));
            #endregion

            #region GmkillCheckBoxs
            if (Convert.ToBoolean(Program.ini_rewards.GetValue("gmkill", "silk_enable")))
            {
                cbox_gmkill_silk.Checked = true;
            }
            else
            {
                cbox_gmkill_silk.Checked = false;
            }
            if (Convert.ToBoolean(Program.ini_rewards.GetValue("gmkill", "gold_enable")))
            {
                cbox_gmkill_gold.Checked = true;
            }
            else
            {
                cbox_gmkill_gold.Checked = false;
            }
            if (Convert.ToBoolean(Program.ini_rewards.GetValue("gmkill", "item_enable")))
            {
                cbox_gmkill_item.Checked = true;
            }
            else
            {
                cbox_gmkill_item.Checked = false;
            }
            if (Convert.ToBoolean(Program.ini_rewards.GetValue("gmkill", "title_enable")))
            {
                cbox_gmkill_title.Checked = true;
            }
            else
            {
                cbox_gmkill_title.Checked = false;
            }

            #endregion

            #region GmkillTextBoxs
            gmkill_silk.Text = Convert.ToString(Program.ini_rewards.GetValue("gmkill", "silk"));
            gmkill_silkpoint.Text = Convert.ToString(Program.ini_rewards.GetValue("gmkill", "silkpoint"));
            gmkill_giftsilk.Text = Convert.ToString(Program.ini_rewards.GetValue("gmkill", "giftsilk"));

            gmkill_itemid.Text = Convert.ToString(Program.ini_rewards.GetValue("gmkill", "itemid"));
            gmkill_quantity.Text = Convert.ToString(Program.ini_rewards.GetValue("gmkill", "quantity"));
            gmkill_plus.Text = Convert.ToString(Program.ini_rewards.GetValue("gmkill", "plus"));

            gmkill_gold.Text = Convert.ToString(Program.ini_rewards.GetValue("gmkill", "gold"));

            gmkill_title.Text = Convert.ToString(Program.ini_rewards.GetValue("gmkill", "title"));
            #endregion

            #region AlchemyCheckBoxs
            if (Convert.ToBoolean(Program.ini_rewards.GetValue("alchemy", "silk_enable")))
            {
                cbox_alchemy_silk.Checked = true;
            }
            else
            {
                cbox_alchemy_silk.Checked = false;
            }
            if (Convert.ToBoolean(Program.ini_rewards.GetValue("alchemy", "gold_enable")))
            {
                cbox_alchemy_gold.Checked = true;
            }
            else
            {
                cbox_alchemy_gold.Checked = false;
            }
            if (Convert.ToBoolean(Program.ini_rewards.GetValue("alchemy", "item_enable")))
            {
                cbox_alchemy_item.Checked = true;
            }
            else
            {
                cbox_alchemy_item.Checked = false;
            }
            if (Convert.ToBoolean(Program.ini_rewards.GetValue("alchemy", "title_enable")))
            {
                cbox_alchemy_title.Checked = true;
            }
            else
            {
                cbox_alchemy_title.Checked = false;
            }

            #endregion

            #region AlchemyTextBoxs
            alchemy_silk.Text = Convert.ToString(Program.ini_rewards.GetValue("alchemy", "silk"));
            alchemy_silkpoint.Text = Convert.ToString(Program.ini_rewards.GetValue("alchemy", "silkpoint"));
            alchemy_giftsilk.Text = Convert.ToString(Program.ini_rewards.GetValue("alchemy", "giftsilk"));

            alchemy_itemid.Text = Convert.ToString(Program.ini_rewards.GetValue("alchemy", "itemid"));
            alchemy_quantity.Text = Convert.ToString(Program.ini_rewards.GetValue("alchemy", "quantity"));
            alchemy_plus.Text = Convert.ToString(Program.ini_rewards.GetValue("alchemy", "plus"));

            alchemy_gold.Text = Convert.ToString(Program.ini_rewards.GetValue("alchemy", "gold"));

            alchemy_title.Text = Convert.ToString(Program.ini_rewards.GetValue("alchemy", "title"));
            #endregion

            #region LmsCheckBoxs
            if (Convert.ToBoolean(Program.ini_rewards.GetValue("lms", "silk_enable")))
            {
                cbox_lms_silk.Checked = true;
            }
            else
            {
                cbox_lms_silk.Checked = false;
            }
            if (Convert.ToBoolean(Program.ini_rewards.GetValue("lms", "gold_enable")))
            {
                cbox_lms_gold.Checked = true;
            }
            else
            {
                cbox_lms_gold.Checked = false;
            }
            if (Convert.ToBoolean(Program.ini_rewards.GetValue("lms", "item_enable")))
            {
                cbox_lms_item.Checked = true;
            }
            else
            {
                cbox_lms_item.Checked = false;
            }
            if (Convert.ToBoolean(Program.ini_rewards.GetValue("lms", "title_enable")))
            {
                cbox_lms_title.Checked = true;
            }
            else
            {
                cbox_lms_title.Checked = false;
            }

            #endregion

            #region LmsTextBoxs
            lms_silk.Text = Convert.ToString(Program.ini_rewards.GetValue("lms", "silk"));
            lms_silkpoint.Text = Convert.ToString(Program.ini_rewards.GetValue("lms", "silkpoint"));
            lms_giftsilk.Text = Convert.ToString(Program.ini_rewards.GetValue("lms", "giftsilk"));

            lms_itemid.Text = Convert.ToString(Program.ini_rewards.GetValue("lms", "itemid"));
            lms_quantity.Text = Convert.ToString(Program.ini_rewards.GetValue("lms", "quantity"));
            lms_plus.Text = Convert.ToString(Program.ini_rewards.GetValue("lms", "plus"));

            lms_gold.Text = Convert.ToString(Program.ini_rewards.GetValue("lms", "gold"));

            lms_title.Text = Convert.ToString(Program.ini_rewards.GetValue("lms", "title"));
            #endregion

            #region UniqueCheckBoxs
            if (Convert.ToBoolean(Program.ini_rewards.GetValue("unique", "silk_enable")))
            {
                cbox_unique_silk.Checked = true;
            }
            else
            {
                cbox_unique_silk.Checked = false;
            }
            if (Convert.ToBoolean(Program.ini_rewards.GetValue("unique", "gold_enable")))
            {
                cbox_unique_gold.Checked = true;
            }
            else
            {
                cbox_unique_gold.Checked = false;
            }
            if (Convert.ToBoolean(Program.ini_rewards.GetValue("unique", "item_enable")))
            {
                cbox_unique_item.Checked = true;
            }
            else
            {
                cbox_unique_item.Checked = false;
            }
            if (Convert.ToBoolean(Program.ini_rewards.GetValue("unique", "title_enable")))
            {
                cbox_unique_title.Checked = true;
            }
            else
            {
                cbox_unique_title.Checked = false;
            }

            #endregion

            #region UniqueTextBoxs
            unique_silk.Text = Convert.ToString(Program.ini_rewards.GetValue("unique", "silk"));
            unique_silkpoint.Text = Convert.ToString(Program.ini_rewards.GetValue("unique", "silkpoint"));
            unique_giftsilk.Text = Convert.ToString(Program.ini_rewards.GetValue("unique", "giftsilk"));

            unique_itemid.Text = Convert.ToString(Program.ini_rewards.GetValue("unique", "itemid"));
            unique_quantity.Text = Convert.ToString(Program.ini_rewards.GetValue("unique", "quantity"));
            unique_plus.Text = Convert.ToString(Program.ini_rewards.GetValue("unique", "plus"));

            unique_gold.Text = Convert.ToString(Program.ini_rewards.GetValue("unique", "gold"));

            unique_title.Text = Convert.ToString(Program.ini_rewards.GetValue("unique", "title"));
            #endregion

            //End 
            #endregion



            Action<MSSQL, string> lam = (sql, db) =>
            {
                if (sql.isConnected())
                {
                    Logger.LogIt(db + " database", LogType.Başarılı);
                    switch (db)
                    {
                        case "Account":
                            acc_toggle.Checked = true;
                            break;
                        case "Log":
                            log_toggle.Checked = true;
                            break;
                        case "Shard":
                            shard_toggle.Checked = true;
                            break;
                        case "Event":
                            storm_toggle.Checked = true;
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    Logger.LogIt(db + " database", LogType.Hata);
                    acc_toggle.Checked = false;
                    log_toggle.Checked = false;
                    shard_toggle.Checked = false;
                    storm_toggle.Checked = false;
                }
            };



            lam(Program.acc_db, "Account");
            lam(Program.log_db, "Log");
            lam(Program.shard_db, "Shard");
            lam(Program.event_db, "Event");

            TriviaListview();
            TriviaEventTime();
            LpnEventTime();
            SnDEventTime();
            HnSEventTime();
            GmKillEventTime();
            AlchemyEventTime();
            LmsEventTime();
            UniqueEventTime();
            ReadItemData();
            ReadMobData();
            ReadSKillData();
            Globals.InitializeTypes();
            getUniqueSpawn();

            Program.m_proxy.PerformClientless(s_ip.Text, Convert.ToUInt16(s_port.Text), Convert.ToUInt32(s_version.Text),
Convert.ToByte(s_locale.Text));
        }


        private void getUniqueSpawn()
        {
            listView_unique.Items.Clear();
            using (SqlDataReader reader = Program.event_db.ExecuteReader("select UniqueID, Count from _UniqueSpawn"))
            {
                while (reader.Read())
                {
                    ListViewItem item = new ListViewItem(new[] { Convert.ToString(reader["UniqueID"]), Convert.ToString(reader["Count"]) });
                    listView_unique.Items.Add(item);
                    listView_unique.Items[listView_unique.Items.Count - 1].EnsureVisible();
                }

            }
            listView_unique.Items[listView_unique.Items.Count - 1].EnsureVisible();
        }
        public void StartTimers()
        {
            t_Zerk = new System.Threading.Timer(new TimerCallback(zerk_Tick), null, 0, Convert.ToInt32(tbox_zerksaniye.Text) * 1000);
        }

        private void zerk_Tick(object e)
        {
            if (cbox_zerk.Checked && Program.uniquespawn_status == 1)
            {
                Packet packetSpawn = new Packet(0x7010); // Spawn Unique Packet
                packetSpawn.WriteUInt8((byte)6);
                packetSpawn.WriteUInt8((byte)0);
                packetSpawn.WriteUInt16(tbox_zerkmob.Text); // mob ID
                packetSpawn.WriteUInt8((byte)0);
                packetSpawn.WriteUInt8((byte)0);
                packetSpawn.WriteUInt8(tbox_zerkadet.Text); // MOB COUNT
                packetSpawn.WriteUInt8((byte)3);
                Program.m_proxy.m_agentSocket.Send(packetSpawn);
            }
        }

        private void flatClose1_Click(object sender, EventArgs e)
        {
            DialogResult result = new DialogResult();
            result = MessageBox.Show("Çıkış yapmak istediğinizden emin misiniz?", "Uyarı", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
                Application.Exit();
            else
                return;
        }

        private void flatButton2_Click(object sender, EventArgs e)
        {
            Program.m_proxy.PerformClientless(s_ip.Text, Convert.ToUInt16(s_port.Text), Convert.ToUInt32(s_version.Text),
            Convert.ToByte(s_locale.Text));
        }

        private void flatButton4_Click(object sender, EventArgs e)
        {
            if (cbServername.Text != "")
            {
                Program.m_proxy.SendLogin(tbUsername.Text, tbPassword.Text, (Shard)cbServername.SelectedItem);
            }
            else
            {
                Logger.LogIt("Server adını boş bırakmayın.", LogType.Hata);
            }
        }

        private void flatButton6_Click(object sender, EventArgs e)
        {
            Program.m_proxy.SendCharacterSelection((Character)cbKarakter.SelectedItem);
        }

        private void flatButton5_Click(object sender, EventArgs e)
        {
            Program.m_proxy.SendCaptcha(tbCaptcha.Text);
        }

        private void TriviaListview()
        {
            SqlDataReader dr = Program.event_db.ExecuteReader("select * from _questions");

            if (!dr.Read())
            {
                Logger.LogIt("Trivia tablosunda veri bulunamadı.", LogType.Normal);
            }
            else
            {
                do
                {
                    ListViewItem item = new ListViewItem(new[] { Convert.ToString(dr["Question"]), Convert.ToString(dr["Answer"]) });
                    listview_trivia.Items.Add(item);
                } while (dr.Read());
            }
            dr.Close();
        }

        public void TriviaEventTime()
        {
            listview_trivia_time.Items.Clear();
            using (SqlDataReader dr = Program.event_db.ExecuteReader("select * from event_time where Type = 'Trivia'"))
            {
                while (dr.Read())
                {
                    ListViewItem item = new ListViewItem(new[] { Convert.ToString(dr["Day"]), Convert.ToString(dr["Time"]) });
                    listview_trivia_time.Items.Add(item);
                }
            }
        }

        public void LpnEventTime()
        {
            listview_lpn_time.Items.Clear();
            using (SqlDataReader dr = Program.event_db.ExecuteReader("select * from event_time where Type = 'LPN'"))
            {
                while (dr.Read())
                {
                    ListViewItem item = new ListViewItem(new[] { Convert.ToString(dr["Day"]), Convert.ToString(dr["Time"]) });
                    listview_lpn_time.Items.Add(item);
                }
            }
        }

        public void SnDEventTime()
        {
            listView1.Items.Clear();
            using (SqlDataReader dr = Program.event_db.ExecuteReader("select * from event_time where Type = 'SnD'"))
            {
                while (dr.Read())
                {
                    ListViewItem item = new ListViewItem(new[] { Convert.ToString(dr["Day"]), Convert.ToString(dr["Time"]) });
                    listView1.Items.Add(item);
                }
            }
        }

        public void HnSEventTime()
        {
            listView5.Items.Clear();
            using (SqlDataReader dr = Program.event_db.ExecuteReader("select * from event_time where Type = 'HnS'"))
            {
                while (dr.Read())
                {
                    ListViewItem item = new ListViewItem(new[] { Convert.ToString(dr["Day"]), Convert.ToString(dr["Time"]) });
                    listView5.Items.Add(item);
                }
            }
        }

        public void GmKillEventTime()
        {
            listView2.Items.Clear();
            using (SqlDataReader dr = Program.event_db.ExecuteReader("select * from event_time where Type = 'GmKill'"))
            {
                while (dr.Read())
                {
                    ListViewItem item = new ListViewItem(new[] { Convert.ToString(dr["Day"]), Convert.ToString(dr["Time"]) });
                    listView2.Items.Add(item);
                }
            }  
        }

        public void AlchemyEventTime()
        {
            listView3.Items.Clear();
            using (SqlDataReader dr = Program.event_db.ExecuteReader("select * from event_time where Type = 'Alchemy'"))
            {
                while (dr.Read())
                {
                    ListViewItem item = new ListViewItem(new[] { Convert.ToString(dr["Day"]), Convert.ToString(dr["Time"]) });
                    listView3.Items.Add(item);
                }
            }
        }

        public void LmsEventTime()
        {
            listView4.Items.Clear();
            using (SqlDataReader dr = Program.event_db.ExecuteReader("select * from event_time where Type = 'LMS'"))
            {
                while (dr.Read())
                {
                    ListViewItem item = new ListViewItem(new[] { Convert.ToString(dr["Day"]), Convert.ToString(dr["Time"]) });
                    listView4.Items.Add(item);
                }
            }
        }

        public void UniqueEventTime()
        {
            listView6.Items.Clear();
            using (SqlDataReader dr = Program.event_db.ExecuteReader("select * from event_time where Type = 'Unique'"))
            {
                while (dr.Read())
                {
                    ListViewItem item = new ListViewItem(new[] { Convert.ToString(dr["Day"]), Convert.ToString(dr["Time"]) });
                    listView6.Items.Add(item);
                }
            }
        }


        private void ReadItemData()
        {
            uint item_id = 0;
            string item_code;
            string item_name;

            using (SqlDataReader dr = Program.shard_db.ExecuteReader("select ID, CodeName128, ObjName128 from _RefObjCommon Where CodeName128 like 'ITEM_%'"))
            {
                if (!dr.Read())
                {
                    Logger.LogIt("ReadItemData reader sağlanamadı.", LogType.Hata);
                }
                else
                {
                    do
                    {
                        item_id = Convert.ToUInt32(dr["ID"]);
                        item_code = Convert.ToString(dr["CodeName128"]);
                        item_name = Convert.ToString(dr["ObjName128"]);

                        if (item_id != 0 || item_code != string.Empty || item_name != string.Empty)
                        {
                            Items_Info.itemsidlist.Add(item_id);
                            Items_Info.itemstypelist.Add(item_code);
                            Items_Info.itemsnamelist.Add(item_name);

                            Items_Info.itemslevellist.Add(1);
                            Items_Info.items_maxlist.Add(1);
                            Items_Info.itemsdurabilitylist.Add(1);
                        }

                    } while (dr.Read());

                    Logger.LogIt("Item verileri yüklendi.", LogType.Başarılı);
                }
            }
        }

        private void ReadMobData()
        {
            uint mob_id = 0;
            string mob_code;
            string mob_name;

            using (SqlDataReader dr = Program.shard_db.ExecuteReader("select ID,CodeName128,ObjName128 from _RefObjCommon Where CodeName128 NOT like 'ITEM_%'"))
            {
                if (!dr.Read())
                {
                    Logger.LogIt("ReadMobData reader sağlanamadı.", LogType.Hata);
                }
                else
                {
                    do
                    {
                        mob_id = Convert.ToUInt32(dr["ID"]);
                        mob_code = Convert.ToString(dr["CodeName128"]);
                        mob_name = Convert.ToString(dr["ObjName128"]);
                        if (mob_id != 0 || mob_code != string.Empty || mob_name != string.Empty)
                        {
                            Mobs_Info.mobsidlist.Add(mob_id);
                            Mobs_Info.mobstypelist.Add(mob_code);
                            Mobs_Info.mobsnamelist.Add(mob_name);

                            Mobs_Info.mobslevellist.Add(1);
                            Mobs_Info.mobshplist.Add(1);
                            Mobs_Info.mobsifuniquelist.Add("1");
                        }
                    } while (dr.Read());

                    Logger.LogIt("Mob verileri yüklendi.", LogType.Başarılı);
                }
            }
        }

        private void ReadSKillData()
        {
            uint skill_id = 0;
            string skill_code;
            string skill_name;

            using (SqlDataReader dr = Program.shard_db.ExecuteReader("Select ID, Basic_Code, Basic_Name from _RefSkill Where Service = 1"))
            {
                if (!dr.Read())
                {
                    Logger.LogIt("ReadSkillData reader sağlanamadı.", LogType.Hata);
                }
                else
                {
                    do
                    {
                        skill_id = Convert.ToUInt32(dr["ID"]);
                        skill_code = Convert.ToString(dr["Basic_Code"]);
                        skill_name = Convert.ToString(dr["Basic_Name"]);

                        if (skill_id != 0 || skill_code != string.Empty || skill_name != string.Empty)
                        {
                            Skills_Info.skillsidlist.Add(skill_id);
                            Skills_Info.skillstypelist.Add(skill_code);
                            Skills_Info.skillsnamelist.Add(skill_name);
                        }
                    } while (dr.Read());

                    Logger.LogIt("Skill verileri yüklendi.", LogType.Başarılı);
                }
            }
        }

        private void flatCheckBox1_CheckedChanged(object sender)
        {
            if (cbox_trivia_silk.Checked)
            {
                trivia_silk.Enabled = true;
                trivia_giftsilk.Enabled = true;
                trivia_silkpoint.Enabled = true;
            }
            else
            {
                trivia_silk.Enabled = false;
                trivia_giftsilk.Enabled = false;
                trivia_silkpoint.Enabled = false;
            }
        }

        private void flatCheckBox2_CheckedChanged(object sender)
        {
            if (cbox_trivia_gold.Checked)
            {
                trivia_gold.Enabled = true;
            }
            else
            {
                trivia_gold.Enabled = false;
            }
        }

        private void flatCheckBox3_CheckedChanged(object sender)
        {
            if (cbox_trivia_item.Checked)
            {
                trivia_itemid.Enabled = true;
                trivia_quantity.Enabled = true;
                trivia_plus.Enabled = true;
            }
            else
            {
                trivia_itemid.Enabled = false;
                trivia_quantity.Enabled = false;
                trivia_plus.Enabled = false;
            }
        }

        private void flatCheckBox4_CheckedChanged(object sender)
        {
            if (cbox_trivia_title.Checked)
            {
                trivia_title.Enabled = true;
            }
            else
            {
                trivia_title.Enabled = false;
            }
        }

        private void toggle_trivia_CheckedChanged(object sender)
        {
            if (toggle_trivia.Checked)
            {
                Program.ini.SetValue("events", "trivia_enable", "true");
            }
            else
            {
                Program.ini.SetValue("events", "trivia_enable", "false");
            }
        }

        private void ekleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frm = new AddTime();
            frm.Show();
            frm.addComboBox("Trivia");
        }

        private void ekleToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            frm = new AddTime();
            frm.Show();
            frm.addComboBox("LPN");
        }

        private void toggle_lpn_CheckedChanged(object sender)
        {
            if (toggle_lpn.Checked)
            {
                Program.ini.SetValue("events", "lpn_enable", "true");
            }
            else
            {
                Program.ini.SetValue("events", "lpn_enable", "false");
            }
        }

        private void flatButton8_Click(object sender, EventArgs e)
        {
            if (tbox_unique_id.Text != "" && tbox_unique_adet.Text != "")
            {
                Program.event_db.ExecuteCommand("insert into _UniqueSpawn (UniqueID, Count) values ('" + tbox_unique_id.Text + "', '" + tbox_unique_adet.Text + "')");
                getUniqueSpawn();
                /*ListViewItem item = new ListViewItem(new[] { tbox_unique_id.Text, tbox_unique_adet.Text });
                listView_unique.Items.Add(item);*/
                listView_unique.Items[listView_unique.Items.Count - 1].EnsureVisible();
            }
            else
            {
                Logger.LogIt("Boş alan bırakmayın.", LogType.Hata);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Logger.LogIt("test", LogType.Normal);
            if (cbox_zerk.Checked)
            {
                Logger.LogIt("test1", LogType.Normal);
                Packet packetSpawn = new Packet(0x7010); // Spawn Unique Packet
                packetSpawn.WriteUInt8((byte)6);
                packetSpawn.WriteUInt8((byte)0);
                packetSpawn.WriteUInt16(tbox_zerkmob.Text); // mob ID
                packetSpawn.WriteUInt8((byte)0);
                packetSpawn.WriteUInt8((byte)0);
                packetSpawn.WriteUInt8(tbox_zerkadet.Text); // MOB COUNT
                packetSpawn.WriteUInt8((byte)3);
                Program.m_proxy.m_agentSocket.Send(packetSpawn);
            }
        }

        private void ekleToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            frm = new AddTime();
            frm.Show();
            frm.addComboBox("SnD");
        }

        private void ekleToolStripMenuItem4_Click(object sender, EventArgs e)
        {
            frm = new AddTime();
            frm.Show();
            frm.addComboBox("HnS");
        }

        private void ekleToolStripMenuItem5_Click(object sender, EventArgs e)
        {
            frm = new AddTime();
            frm.Show();
            frm.addComboBox("GmKill");
        }

        private void ekleToolStripMenuItem6_Click(object sender, EventArgs e)
        {
            frm = new AddTime();
            frm.Show();
            frm.addComboBox("Alchemy");
        }

        private void ekleToolStripMenuItem7_Click(object sender, EventArgs e)
        {
            frm = new AddTime();
            frm.Show();
            frm.addComboBox("LMS");

        }

        private void ekleToolStripMenuItem8_Click(object sender, EventArgs e)
        {
            frm = new AddTime();
            frm.Show();
            frm.addComboBox("Unique");
        }

        private void silToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string gun = listview_trivia_time.SelectedItems[0].Text;
            string saat = listview_trivia_time.SelectedItems[0].SubItems[1].Text;
            Program.event_db.ExecuteCommand("delete from event_time where Time = '" + saat + "' and Day = '" + gun + "' and Type = 'Trivia'");
            TriviaEventTime();
        }

        private void silToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            string gun = listview_lpn_time.SelectedItems[0].Text;
            string saat = listview_lpn_time.SelectedItems[0].SubItems[1].Text;
            Program.event_db.ExecuteCommand("delete from event_time where Time = '" + saat + "' and Day = '" + gun + "' and Type = 'LPN'");
            LpnEventTime();
        }

        private void silToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            string gun = listView1.SelectedItems[0].Text;
            string saat = listView1.SelectedItems[0].SubItems[1].Text;
            Program.event_db.ExecuteCommand("delete from event_time where Time = '" + saat + "' and Day = '" + gun + "' and Type = 'SnD'");
            SnDEventTime();
        }

        private void silToolStripMenuItem4_Click(object sender, EventArgs e)
        {
            string gun = listView5.SelectedItems[0].Text;
            string saat = listView5.SelectedItems[0].SubItems[1].Text;
            Program.event_db.ExecuteCommand("delete from event_time where Time = '" + saat + "' and Day = '" + gun + "' and Type = 'HnS'");
            HnSEventTime();
        }

        private void silToolStripMenuItem5_Click(object sender, EventArgs e)
        {
            string gun = listView2.SelectedItems[0].Text;
            string saat = listView2.SelectedItems[0].SubItems[1].Text;
            Program.event_db.ExecuteCommand("delete from event_time where Time = '" + saat + "' and Day = '" + gun + "' and Type = 'GmKill'");
            GmKillEventTime();
        }

        private void silToolStripMenuItem6_Click(object sender, EventArgs e)
        {
            string gun = listView3.SelectedItems[0].Text;
            string saat = listView3.SelectedItems[0].SubItems[1].Text;
            Program.event_db.ExecuteCommand("delete from event_time where Time = '" + saat + "' and Day = '" + gun + "' and Type = 'Alchemy'");
            AlchemyEventTime();
        }

        private void silToolStripMenuItem7_Click(object sender, EventArgs e)
        {
            string gun = listView4.SelectedItems[0].Text;
            string saat = listView4.SelectedItems[0].SubItems[1].Text;
            Program.event_db.ExecuteCommand("delete from event_time where Time = '" + saat + "' and Day = '" + gun + "' and Type = 'LMS'");
            LmsEventTime();
        }

        private void silToolStripMenuItem8_Click(object sender, EventArgs e)
        {
            string gun = listView6.SelectedItems[0].Text;
            string saat = listView6.SelectedItems[0].SubItems[1].Text;
            Program.event_db.ExecuteCommand("delete from event_time where Time = '" + saat + "' and Day = '" + gun + "' and Type = 'Unique'");
            UniqueEventTime();
        }
    }
}
