using StormBot.Events;
using StormBot.Forms;
using StormBot.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StormBot
{
    static class Program
    {

        public static Ini ini = new Ini(Environment.CurrentDirectory + "\\settings\\config.ini");
        public static Ini ini_rewards = new Ini(Environment.CurrentDirectory + "\\settings\\rewards.ini");

        public static Form1 main;
        public static Rewards rewards = new Rewards();
        public static Proxy m_proxy = new Proxy();

        public static UniqueSpawnParse u_parsing = new UniqueSpawnParse();

        //public static
        public static MSSQL acc_db;
        public static MSSQL log_db;
        public static MSSQL shard_db;
        public static MSSQL event_db;

        //Mssql ini Settings
        public static string mssql_name;
        public static string mssql_acc;
        public static string mssql_pw;
        public static string account_database;
        public static string log_database;
        public static string shard_database;
        public static string event_database;

        //Events
        public static bool HaveEvent = false;
        public static bool AlreadyStarted = false;
        public static bool trivia_event = false;
        public static List<string> TriviaList = new List<string>();


        //LPN
        public static int lpn_status = 0;
        public static int lpn_check_ptno = 0;
        public static int lpn_target_number = 0;
        public static int snd_status = 0;
        public static int snd_eventtime = 0;
        public static int gmkill_status = 0;
        public static int snd_mobid = 0;
        public static int lms_status = 0;
        public static int lms_regtime = 1; // dakika
        public static int lms_matchtime = 5; //dakika
        public static int lms_lvllimit = 90;
        public static int kayipgm_status = 0;
        public static int kayipgm_endtime = 5; // dakika
        public static List<string> lms_playerlist = new List<string>();
        public static List<string> lms_recalledplayers = new List<string>();
        public static uint mobid = 0u;
        public static bool ilk = false;
        public static int MobSira = 0;
        public static int mobsay = 0;
        public static int uniquespawn_status = 0;
        public static bool chat_log = false;

        //Notices
        public static string lpn_start_notice = "[Event Lucky Party Number] Event başladı. Şanslı parti no: %number%";
        public static string lpn_win_notice = "[Event Lucky Party Number] Kazanan: %charname%";
        public static string lpn_nowin_notice = "[Event Lucky Party Number] parti bulunamadığı için sonlandı.";

        //Trivia
        public static string trivia_start = "[Event Trivia] Trivia event başladı. Storm nicke pm atınız";
        public static string trivia_end = "[Event Trivia] Trivia event bitti";
        public static string trivia_win = "[Event Trivia] Kazanan: %charname%";
        public static string trivia_wrong = "[Event Trivia] Yanlış cevap!";
        public static string trivia_wrong2 = "[Event Trivia] Bu cevap daha önceden verildi. Cevap hakkınız bitmiştir.";

        //SnD
        public static string snd_firstnotice = "[Search And Destroy Event] Event başladı. %placename% bölgesinde ki Unique bul ve öldür!";
        public static string snd_wait_notice = "[Search And Destroy Event] Event %min% dakika sürecektir.";
        public static string snd_nowin_notice = "[Search And Destroy Event] Kimse öldüremediği için bitmiştir.";
        public static string snd_win_notice = "[Search And Destroy] %player% kazandı!";

        //GmKill
        public static string gmkill_firstnotice = "[GM Killer Event] 1 dakika sonra %placename% bölgesinde başlalayacaktır.";
        public static string gmkill_running = "[GM Killer Event] %placename% bölgesine git ve GM öldür!";
        public static string gmkill_end = "[GM Killer Event] Kimse öldüremediği için sona erdi.";
        public static string gmkill_finish = "[GM Killer Event] Bitmiştir.";
        public static string gmkill_win = "[GM Killer Event] %charname% kazanmıştır.";

        //Alchemy
        public static string alchemy_startnotice = "[Alchemy Event] Jangan South bölgesinde başladı!";
        public static string alchemy_r1notice = "[Alchemy Event] 1. round itemlari droplandı.";
        public static string alchemy_r2notice = "[Alchemy Event] 2. round itemlari droplandı.";
        public static string alchemy_r3notice = "[Alchemy Event] 3. round itemlari droplandı.";
        public static string alchemy_waitnotice = "[Alchemy Event] Event %min% dakika sürecektir. Itemi en yüksek artıya yükselten oyuncu kazanacaktır.";
        public static string alchemy_finishnotice = "[Alchemy Event] Alchemy eventi +%plus% basarak %charname% kazanmıştır.";
        public static string alchemy_win = "[Alchemy Event] Alchemy eventi +%plus% basarak %charname% kazanmıştır.";
        public static string alchemy_nowin = "[Alchemy Event] Kimse kazanamadığı için bitmiştir.";

        //LMS
        public static string lms_firstnotice = "[Last Man Standing Event] 1 dakika sonra başlayacaktır. Job suit takmayın ve %plus% level üzeri olmalısınız!";
        public static string lms_regglobal = "[Last Man Standing Event] Evente katılmak için /reg yazıp gönderiniz.";
        public static string lms_closeglobal = "[Last Man Standing Event] Kayıt kapandı.";
        public static string lms_startnotice = "[Last Man Standing Event] Sarı Cape takıp PvP yapmanız için 40 saniyeniz var!, yoksa şehire geri döneceksiniz.";
        public static string lms_winnernotice = "[Last Man Standing Event] Tebrikler %playername% eventi kazandı.";
        public static string lms_nowin = "[Last Man Standing Event] Event bitti, kazanan olmadı!";


        //HnS
        public static string kayipgm_startnotice = "[Hide And Seek Event] Başladı! [ %placename% ] bölgesindeyim ilk exchange gönderen kazanır!";
        public static string kayipgm_timenotice = "[Hide And Seek Event] Event bitmeden beni bul!";
        public static string kayipgm_win = "[Hide And Seek Event] Tebrikler %name% eventi kazandın!";
        public static string kayipgm_nowin = "[Hide And Seek Event] Kimse bulamadığı için event bitmiştir.";


        //UniqueEvent
        public static string uniqueevent_firstnotice = "[Unique Event] %placename% alanında başlıyor!";
        public static string uniqueevent_end = "[Unique Event] Bitmiştir!";


        public static string getBetween(string strSource, string strStart, string strEnd)
        {
            int Start, End;
            if (strSource.Contains(strStart) && strSource.Contains(strEnd))
            {
                Start = strSource.IndexOf(strStart, 0) + strStart.Length;
                End = strSource.IndexOf(strEnd, Start);
                return strSource.Substring(Start, End - Start);
            }
            else
            {
                return "";
            }
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            main = new Form1();
            Application.Run(main);
        }
    }
}
