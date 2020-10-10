using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StormBot.Forms
{
    public partial class AddTime : Form
    {
        public AddTime()
        {
            InitializeComponent();
        }

        public void addComboBox(string events)
        {
            flatComboBox1.Items.Clear();
            flatComboBox1.Items.Add(events);
            flatComboBox1.Text = events;
            flatComboBox1.Enabled = false;
        }

        private void flatClose2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void flatButton1_Click(object sender, EventArgs e)
        {
            if (flatTextBox1.Text != "" && flatComboBox2.Text != "")
            {
                if (flatComboBox1.SelectedItem.ToString() == "Trivia")
                {
                    if (Program.main.listview_trivia_time.Items.Count > 0)
                    {
                        Logger.LogIt("Trivia için sadece başlangıç saati belirleyebilirsiniz.", LogType.Hata);
                    }
                    else
                    {
                        Program.event_db.ExecuteCommand("insert into event_time (Day, Time, Type) values ('" + flatComboBox2.Text + "', '" + flatTextBox1.Text + "','" + flatComboBox1.Text + "')");
                        Program.main.TriviaEventTime();
                    }
                }

                if (flatComboBox1.SelectedItem.ToString() == "LPN")
                {
                    if (Program.main.listview_lpn_time.Items.Count > 0)
                    {
                        Logger.LogIt("LPN için sadece başlangıç saati belirleyebilirsiniz.", LogType.Hata);
                    }
                    else
                    {
                        Program.event_db.ExecuteCommand("insert into event_time (Day, Time, Type) values ('" + flatComboBox2.Text + "', '" + flatTextBox1.Text + "','" + flatComboBox1.Text + "')");
                        Program.main.LpnEventTime();
                    }
                }

                if (flatComboBox1.SelectedItem.ToString() == "SnD")
                {
                    Program.event_db.ExecuteCommand("insert into event_time (Day, Time, Type) values ('" + flatComboBox2.Text + "', '" + flatTextBox1.Text + "','" + flatComboBox1.Text + "')");
                    Program.main.SnDEventTime();
                }

                if (flatComboBox1.SelectedItem.ToString() == "GmKill")
                {
                    Program.event_db.ExecuteCommand("insert into event_time (Day, Time, Type) values ('" + flatComboBox2.Text + "', '" + flatTextBox1.Text + "','" + flatComboBox1.Text + "')");
                    Program.main.GmKillEventTime();
                }

                if (flatComboBox1.SelectedItem.ToString() == "Alchemy")
                {
                    Program.event_db.ExecuteCommand("insert into event_time (Day, Time, Type) values ('" + flatComboBox2.Text + "', '" + flatTextBox1.Text + "','" + flatComboBox1.Text + "')");
                    Program.main.AlchemyEventTime();
                }

                if (flatComboBox1.SelectedItem.ToString() == "LMS")
                {
                    Program.event_db.ExecuteCommand("insert into event_time (Day, Time, Type) values ('" + flatComboBox2.Text + "', '" + flatTextBox1.Text + "','" + flatComboBox1.Text + "')");
                    Program.main.LmsEventTime();
                }

                if (flatComboBox1.SelectedItem.ToString() == "HnS")
                {
                    Program.event_db.ExecuteCommand("insert into event_time (Day, Time, Type) values ('" + flatComboBox2.Text + "', '" + flatTextBox1.Text + "','" + flatComboBox1.Text + "')");
                    Program.main.HnSEventTime();
                }

                if (flatComboBox1.SelectedItem.ToString() == "Unique")
                {
                    Program.event_db.ExecuteCommand("insert into event_time (Day, Time, Type) values ('" + flatComboBox2.Text + "', '" + flatTextBox1.Text + "','" + flatComboBox1.Text + "')");
                    Program.main.UniqueEventTime();
                }
            }
        }
    }
}
