using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StormBot
{
    public enum LogType
    {
        Normal,
        Başarılı,
        Hata,
        Query,
        Global,
        PM,
        Notice,
    }

    class Logger
    {
        object obj = new object();
        public static void LogIt(string msg, LogType lvl)
        {

            StringBuilder sBuilder = new StringBuilder();
            sBuilder.AppendFormat("[{0}] -> {1} \r\n", DateTime.Now, msg);

            var item = new ListViewItem(new[] {lvl.ToString(), msg, DateTime.Now.ToShortDateString() + " - " + DateTime.Now.ToShortTimeString()});
            item.ForeColor = getColor(lvl);
            Program.main.log.Items.Add(item);
            Program.main.log.Items[Program.main.log.Items.Count - 1].EnsureVisible();

    
                try
                {
                    using (StreamWriter writer = File.AppendText("log.txt"))
                    {
                        writer.Write(sBuilder.ToString());
                        writer.Close();
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogIt("StreamWriter failed", LogType.Hata);
                }
           


        }

        static Color getColor(LogType lvl)
        {
            switch (lvl)
            {
                case LogType.Normal:
                    return Color.Gray;
                case LogType.Başarılı:
                    return Color.Green;
                case LogType.Hata:
                    return Color.Red;
                case LogType.Query:
                    return Color.MediumSlateBlue;
                case LogType.Global:
                    return Color.Goldenrod;
                case LogType.PM:
                    return  Color.DarkTurquoise;
                case LogType.Notice:
                    return Color.DeepPink;
                default:
                    return Color.Gray;
            }
        }
    }
}
