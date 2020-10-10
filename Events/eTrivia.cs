using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StormBot.Events
{
    public class eTrivia
    {
        public static async void TriviaSystem()
        {

            while (true)
            {
                string[] read = Program.event_db.getSingleArray("exec _getQuestion 1");
                {
                    string Question = read[0];
                    Program.m_proxy.SendNotice(Program.trivia_start); 
                    await Task.Delay(1500);
                    Program.m_proxy.SendNotice("Soru: " + Question + "");
                    Program.trivia_event = true;
                    if (Program.trivia_event)
                    {
                        new Thread(Sec).Start();
                    }
                }

                //Trivia Timer
                await Task.Delay((int)TimeSpan.FromMinutes(Convert.ToInt32(Program.main.txtTriviaMinute.Text)).TotalMilliseconds);

            }
        }

        public static async void Sec()
        {
            await Task.Delay(60000);
            if (Program.trivia_event)
            {
                Program.trivia_event = false;
                Program.event_db.ExecuteCommand("exec _getQuestion 0");
                Program.m_proxy.SendNotice(Program.trivia_end);
                Program.TriviaList.Clear();
            }
        }
    }
}
