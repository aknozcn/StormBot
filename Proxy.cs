using StormBot.Events;
using StormBot.Functions;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using SilkroadSecurityApi;
namespace StormBot
{
    public class Proxy
    {

        public bool CharSpawned = false;
        public Random rdm = new Random();
        public delegate void LogEventHandler(string text);

        public event LogEventHandler Log;


        #region Gateway Fields

        private Server m_gatewaySocket;

        private string m_GatewayIP;

        public string GatewayIP
        {
            get { return m_GatewayIP; }
            set { m_GatewayIP = value; }
        }

        private ushort m_GatewayPort;

        public ushort GatewayPort
        {
            get { return m_GatewayPort; }
            set { m_GatewayPort = value; }
        }

        private bool m_GatewayConnected;

        public bool GatewayConnected
        {
            get { return m_GatewayConnected; }
            set { m_GatewayConnected = value; }
        }

        #endregion

        #region Agent Fields

        public Server m_agentSocket;

        private string m_AgentIP;

        public string AgentIP
        {
            get { return m_AgentIP; }
            set { m_AgentIP = value; }
        }

        private ushort m_AgentPort;

        public ushort AgentPort
        {
            get { return m_AgentPort; }
            set { m_AgentPort = value; }
        }

        private bool m_AgentConnected;

        public bool AgentConnected
        {
            get { return m_AgentConnected; }
            set { m_AgentConnected = value; }
        }

        #endregion

        #region Client Fields

        private Client m_clientSocket;

        private uint m_ClientVersion;

        public uint ClientVersion
        {
            get { return m_ClientVersion; }
            set { m_ClientVersion = value; }
        }

        private byte m_ClientLocal;

        public byte ClientLocal
        {
            get { return m_ClientLocal; }
            set { m_ClientLocal = value; }
        }

        private bool m_ClientConnected;

        public bool ClientConnected
        {
            get { return m_ClientConnected; }
        }

        private ushort m_LocalPort;

        public ushort LocalPort
        {
            get { return m_LocalPort; }
            set { m_LocalPort = value; }
        }

        #endregion

        private bool m_Clientless;
        private bool m_SwitchClient;
        private bool m_ConnectToAgent;

        private bool m_ClientWaitingForData;
        private bool m_ClientWatingForFinish;

        #region 0x6102/0x6103 logindata storage

        private uint m_SessionID;
        private string m_Username;
        private string m_Password;
        private ushort m_ServerID;

        #endregion

        public Proxy()
        {
            m_gatewaySocket = new Server();
            m_gatewaySocket.Connected += m_gatewaySocket_Connected;
            m_gatewaySocket.Disconnected += m_gatewaySocket_Disconnected;
            m_gatewaySocket.Kicked += m_gatewaySocket_Kicked;
            m_gatewaySocket.PacketReceived += m_gatewaySocket_PacketReceived;

            m_agentSocket = new Server();
            m_agentSocket.Connected += m_agentSocket_Connected;
            m_agentSocket.Disconnected += m_agentSocket_Disconnected;
            m_agentSocket.Kicked += m_agentSocket_Kicked;
            m_agentSocket.PacketReceived += m_agentSocket_PacketReceived;

            m_clientSocket = new Client();
            m_clientSocket.Connected += m_clientSocket_Connected;
            m_clientSocket.Disconnected += m_clientSocket_Disconnected;
            m_clientSocket.PacketReceived += m_clientSocket_PacketReceived;
        }

        public void PerformClientless(string GatewayIP, ushort GatewayPort, uint ClientVersion, byte ClientLocal)
        {
            m_GatewayIP = GatewayIP;
            m_GatewayPort = GatewayPort;
            m_ClientVersion = ClientVersion;
            m_ClientLocal = ClientLocal;

            m_Clientless = true; //FLAG CLIENTLESS

            m_gatewaySocket.Connect(m_GatewayIP, m_GatewayPort);
        }

        internal void Connect(string GatewayIP, ushort GatewayPort, uint ClientVersion, byte ClientLocal)
        {
            m_GatewayIP = GatewayIP;
            m_GatewayPort = GatewayPort;
            m_ClientVersion = ClientVersion;
            m_ClientLocal = ClientLocal;

            m_Clientless = false; //FLAG CLIENTLESS

            m_gatewaySocket.Connect(m_GatewayIP, m_GatewayPort);
        }

        public void SendLogin(string username, string password, Shard Server)
        {
            //[C -> S][6102][22 bytes][Enc]
            //16 //Local
            //06 00 //UsernameLenght
            //64 61 78 74 65 72 //Username
            //09 00 //PasswordLenght
            //31 33 62 69 74 74 65 32 34 //Password
            //40 00  //ShardID                     

            m_Username = username;
            m_Password = password;
            m_ServerID = Server.ID;

            Packet packet = new Packet(0x6102, true);
            packet.WriteUInt8(m_ClientLocal);
            packet.WriteAscii(m_Username);
            packet.WriteAscii(m_Password);
            packet.WriteUInt16(m_ServerID);
            packet.Lock();

            m_gatewaySocket.Send(packet);
        }

        public void SendCaptcha(string captcha)
        {
            Packet packet = new Packet(0x6323);
            packet.WriteAscii(captcha);
            packet.Lock();
            m_gatewaySocket.Send(packet);
        }

        public void Listen()
        {
            ushort testPort = 16000;
            bool validPort = false;
            System.Net.Sockets.Socket socket = new System.Net.Sockets.Socket(
                System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Stream,
                System.Net.Sockets.ProtocolType.Tcp);
            do
            {
                try
                {
                    socket.Bind(new System.Net.IPEndPoint(System.Net.IPAddress.Loopback, testPort));
                    m_LocalPort = testPort;
                    validPort = true;
                }
                catch
                {
                    testPort++;
                }

            } while (!validPort);

            socket.Close();
            socket = null;

            m_clientSocket.Listen(m_LocalPort);
        }

        #region Gateway-EventHandlers

        void m_gatewaySocket_PacketReceived(Packet packet)
        {
            if (m_Clientless)
            {
                if (!m_SwitchClient) //Clientless connection packets should end up here
                {
                    #region Request Patches

                    if (packet.Opcode == 0x2001)
                    {
                        //Generate the Patchverification
                        Packet response = new Packet(0x6100, true);
                        response.WriteUInt8(m_ClientLocal);
                        response.WriteAscii("SR_Client"); //ServiceName
                        response.WriteUInt32(m_ClientVersion);

                        m_gatewaySocket.Send(response);
                    }
                    else
                    {
                        if (packet.Opcode == 0xa100)
                        {
                            switch (packet.ReadUInt8())
                            {
                                case 1:
                                    Packet packet2 = new Packet(0x6101, true);
                                    m_gatewaySocket.Send(packet2);
                                    goto Label_04A2;

                                case 2:
                                    Logger.LogIt("Version yanlış lütfen tekrar deneyin !", LogType.Normal);
                                    goto Label_04A2;
                            }
                            return;
                        }

                        #endregion


                        #region Reconnect to AgentServer on successfull login
                        if (packet.Opcode == 0xa102)
                        {
                            byte num2 = packet.ReadUInt8();
                            if (num2 == 1)
                            {
                                m_SessionID = packet.ReadUInt32();
                                m_AgentIP = packet.ReadAscii();
                                m_AgentPort = packet.ReadUInt16();
                                m_gatewaySocket.Disconnect();
                                m_agentSocket.Connect(m_AgentIP, m_AgentPort);
                            }
                            if (num2 == 2)
                            {
                                switch (packet.ReadUInt8())
                                {
                                    case 1:
                                        {
                                            byte num6 = packet.ReadUInt8();
                                            byte num7 = packet.ReadUInt8();
                                            byte num8 = packet.ReadUInt8();
                                            byte num9 = packet.ReadUInt8();
                                            byte num10 = packet.ReadUInt8();
                                            Logger.LogIt(string.Format("Yanlış parola  ( ", num10, " / ", num6, " )"), LogType.Normal);
                                            break;
                                        }
                                    case 2:
                                        if (packet.ReadUInt8() == 1)
                                        {
                                            Logger.LogIt("Engellenme sebebi: " + packet.ReadAscii(), LogType.Normal);
                                        }
                                        break;

                                    case 3:
                                        Logger.LogIt("Karakter zaten oyunda !", LogType.Normal);
                                        break;
                                }
                            }
                        }
                    }

                #endregion

                #region ServerList

                Label_04A2:
                    if ((packet.Opcode == 0xa101) && (packet.ReadUInt8() == 1))
                    {
                        byte num12 = packet.ReadUInt8();
                        string str3 = packet.ReadAscii();
                        byte num13 = packet.ReadUInt8();
                        byte num14 = packet.ReadUInt8();
                        ushort num15 = packet.ReadUInt16();
                        string str4 = packet.ReadAscii();
                        ushort num16 = packet.ReadUInt16();
                        ushort num17 = packet.ReadUInt16();
                        byte num18 = packet.ReadUInt8();
                        //send login packet
                        if (!CharSpawned)
                        {
                            Packet p = new Packet(0x6102);
                            p.WriteUInt8(Program.main.s_locale.Text);
                            p.WriteAscii(Program.main.tbUsername.Text);
                            p.WriteAscii(Program.main.tbPassword.Text);
                            p.WriteUInt16(64);
                            m_gatewaySocket.Send(p);
                        }
                    }

                    #endregion

                    #region ImageCode/Login

                    //ImageCode Challenge
                    if (packet.Opcode == 0x2322)
                    {
                        Packet p = new Packet(0x6323);
                        p.WriteAscii(Program.main.tbCaptcha.Text);
                        m_gatewaySocket.Send(p);
                    }

                    //ImageCode Response
                    else if (packet.Opcode == 0xA323)
                    {
                        byte bb = packet.ReadUInt8();
                        if (bb != 1)
                        {
                            packet.ReadUInt8Array(4);
                            byte wrong = packet.ReadUInt8();
                            Logger.LogIt("Yanlış Captcha (" + wrong + "/3)", LogType.Hata);
                            if (wrong == 3)
                            {
                                Logger.LogIt("Disconnected ! 5 saniye içerisinde program kapanacak.", LogType.Normal);
                                Thread.Sleep(5000);
                                Environment.Exit(0);
                            }
                        }
                    }

                    #endregion
                } //Else? Else would be clientless and switching client, you could implement Gateway Clientless->Client function, but thats kinda useless.       
            }
            else //Proxy Mode - Forwarding Packets between Client and Server
            {
                #region Redirect client to local AgentListener

                if (packet.Opcode == 0xA102)
                {
                    byte result = packet.ReadUInt8();
                    if (result == 1)
                    {
                        m_SessionID = packet.ReadUInt32();
                        m_AgentIP = packet.ReadAscii();
                        m_AgentPort = packet.ReadUInt16();


                        //Create fake response for Client to redirect to localIP/localPort
                        Packet response = new Packet(0xA102, true);
                        response.WriteUInt8(result);
                        response.WriteUInt32(m_SessionID);
                        response.WriteAscii(System.Net.IPAddress.Loopback.ToString());
                        response.WriteUInt16(m_LocalPort);
                        response.Lock();
                        m_ConnectToAgent = true;
                        packet = response;
                    }
                    else
                    {
                        //Program.main.groupLogin.Enabled = true;
                    }
                }

                #endregion

                m_clientSocket.Send(packet);
            }
        }

        void m_gatewaySocket_Kicked()
        {
            m_GatewayConnected = false;
            m_gatewaySocket.Disconnect();
            m_clientSocket.Disconnect();

            if (Log != null)
            {
                Log("Gateway kicked");
            }
        }

        void m_gatewaySocket_Disconnected()
        {
            m_GatewayConnected = false;

            if (Log != null)
            {
                Log("Gateway disconnected");
            }
        }

        void m_gatewaySocket_Connected(string ip, ushort port)
        {
            m_GatewayConnected = true;

            if (Log != null)
            {
                Log(string.Format("GatewaySocket connected to {0}:{1}", ip, port));
            }

            ///!!! CrossThreading !!!
            //Program.main.groupLogin.Enabled = true;
        }

        #endregion

        #region Agent-EventHandlers

        void m_agentSocket_PacketReceived(Packet packet)
        {
            if (m_Clientless)
            {
                if (!m_SwitchClient) //Normal Clientless connection should end up here
                {
                    #region Login

                    if (packet.Opcode == 0x6005)
                    {
                        Packet response = new Packet(0x6103, true);
                        response.WriteUInt32(m_SessionID);
                        response.WriteAscii(m_Username);
                        response.WriteAscii(m_Password);
                        response.WriteUInt8(m_ClientLocal);

                        Random random = new Random();
                        byte[] MAC = new byte[6];
                        random.NextBytes(MAC);
                        response.WriteUInt8Array(MAC);
                        response.Lock();

                        m_agentSocket.Send(response);
                    }


                    else if (packet.Opcode == 0xa103)
                    {
                        if (packet.ReadUInt8() == 1)
                        {
                            Packet packet2 = new Packet(0x7007);
                            packet2.WriteUInt8((byte)2);
                            m_agentSocket.Send(packet2);
                        }
                    }

                    #endregion

                    #region CharacterSelection

                    else if (packet.Opcode == 0xb007)
                    {
                        Packet selectcharpacket = new Packet(0x7001);
                        selectcharpacket.WriteAscii(Program.main.tbox_charname.Text);
                        m_agentSocket.Send(selectcharpacket);
                    }


                    #endregion

                    #region Ingame

                    if (packet.Opcode == 0x3020)
                    {
                        Packet response = new Packet(0x3012);
                        m_agentSocket.Send(response);
                        Packet response2 = new Packet(0x750E); //CLIENT_REQUEST_WEATHER
                        m_agentSocket.Send(response2);
                    }


                    if (packet.Opcode == 0x34A6) //End CharacterData
                    {

                    }

                    if (packet.Opcode == 0x3013)
                    {
                        Thread t = new Thread(() => CharSpawn.parseSpawn(packet));
                        t.Start();
                        if (!Program.AlreadyStarted)
                        {
                            // new Thread(Ping).Start();
                            Thread.Sleep(2000);
                            Program.AlreadyStarted = true;
                            new Thread(EventTime).Start();
                            new Thread(AutoNotice).Start();
                            new Thread(AutoPm).Start();
                            Logger.LogIt("Event süreleri aktif edildi.", LogType.Normal);
                            Logger.LogIt("AutoNotice aktif edildi.", LogType.Normal);
                            Logger.LogIt("AutoPm aktif edildi.", LogType.Normal);
                        }
                    }

                    else if (packet.Opcode == 0xB516) // Cap Opcodes
                    {
                        Thread t = new Thread(() => CapFunc.Cap_Parse(packet));
                        t.Start();
                    }

                    else if (packet.Opcode == 0x300C)
                    {
                        Thread t = new Thread(() => Program.u_parsing.unique_parse(packet));
                        t.Start();
                    }

                    else if (packet.Opcode == 0x3080)
                    {
                        Thread t = new Thread(() => ExcParse.EXC_parse(packet));
                        t.Start();
                    }

                    else if ((packet.Opcode == 0x34b5))
                    {
                        Packet p = new Packet(0x34b6);
                        m_agentSocket.Send(p);
                    }

                    else if ((packet.Opcode == 0x3011) && (Program.gmkill_status == 1)) // GM Killed
                    {
                        using (SqlDataReader reader = Program.event_db.ExecuteReader("select CharName from GmKiller"))
                        {

                            while (reader.Read())
                            {
                                string killername = Convert.ToString(reader["CharName"]);
                                if (killername == "NULL")
                                {
                                    Logger.LogIt("KillerName bulunamadı!", LogType.Hata);
                                    CapeOff();
                                    Program.HaveEvent = false;
                                }
                                else
                                {
                                    if (Program.main.cbox_gmkill_silk.Checked)
                                    {
                                        if (Program.main.gmkill_silk.Text != "" || Program.main.gmkill_silkpoint.Text != "" || Program.main.gmkill_giftsilk.Text != "")
                                        {
                                            Program.rewards.GiveSilk(killername, Convert.ToInt32(Program.main.gmkill_silk.Text), Convert.ToInt32(Program.main.gmkill_giftsilk.Text), Convert.ToInt32(Program.main.gmkill_silkpoint.Text));
                                        }
                                    }

                                    if (Program.main.cbox_gmkill_item.Checked)
                                    {
                                        if (Program.main.gmkill_itemid.Text != "" || Program.main.gmkill_quantity.Text != "" || Program.main.gmkill_plus.Text != "")
                                        {
                                            Program.rewards.GiveItem(killername, Convert.ToInt32(Program.main.gmkill_itemid.Text), Convert.ToInt32(Program.main.gmkill_quantity.Text), Convert.ToInt32(Program.main.gmkill_plus.Text));
                                        }
                                    }

                                    if (Program.main.cbox_gmkill_gold.Checked)
                                    {
                                        if (Program.main.gmkill_gold.Text != "")
                                        {
                                            Program.rewards.GiveGold(killername, Convert.ToInt32(Program.main.gmkill_gold.Text));
                                        }
                                    }

                                    if (Program.main.cbox_gmkill_title.Checked)
                                    {
                                        if (Program.main.gmkill_title.Text != "")
                                        {
                                            Program.rewards.GiveTitle(killername, Convert.ToInt32(Program.main.gmkill_title.Text));
                                        }
                                    }
                                    string win = Program.gmkill_win.Replace("%charname%", killername);
                                    SendNotice(win);
                                    CapeOff();
                                    Logger.LogIt("Gm Kill ödülleri verilmiştir.", LogType.Normal);
                                    Program.event_db.ExecuteCommand("truncate table GmKiller");
                                    Program.HaveEvent = false;
                                }

                            }
                        }
                    }

                    else if (packet.Opcode == 0xB06C)
                    {
                        if ((Program.lpn_check_ptno == 0) && (Program.lpn_status == 1)) //ptno henüz yoksa
                        {
                            try
                            {
                                packet.ReadUInt8();
                                packet.ReadUInt8();
                                packet.ReadUInt8();
                                uint NumberOfParties = packet.ReadUInt8();
                                int lastPtNumb;
                                for (int i = 0; i < NumberOfParties; i++)
                                {
                                    uint pt_no = packet.ReadUInt32(); //pt number
                                    packet.ReadUInt32(); //char model
                                    string PTcharname = packet.ReadAscii();
                                    int Number = i + 1;
                                    if (Number.ToString() == NumberOfParties.ToString())
                                    {
                                        lastPtNumb = Convert.ToInt32(pt_no);
                                        Program.lpn_target_number = rdm.Next(lastPtNumb + Convert.ToInt32(Program.main.lpn_min.Text), lastPtNumb + Convert.ToInt32(Program.main.lpn_max.Text));
                                        string lpnstnotice = Program.lpn_start_notice.Replace("%number%", Program.lpn_target_number.ToString());
                                        SendNotice(lpnstnotice);
                                        //SendNotice("[Event Lucky Party Number] Event başladı. Şanslı parti no: " + Program.lpn_target_number.ToString() + "");

                                        Program.lpn_check_ptno = 1;
                                    }
                                    packet.ReadUInt32Array(6);
                                    packet.ReadAscii();
                                }
                            }
                            catch { Console.WriteLine("Failed to lpn"); }
                        } ///// END DEactive
                        else if ((Program.lpn_check_ptno == 1) && (Program.lpn_status == 1)) //ptno varsa
                        {
                            try
                            {
                                packet.ReadUInt8();
                                packet.ReadUInt8();
                                packet.ReadUInt8();
                                uint NumberOfParties = packet.ReadUInt8();
                                for (int i = 0; i < NumberOfParties; i++)
                                {
                                    uint pt_no = packet.ReadUInt32(); //pt number
                                    packet.ReadUInt32(); //char model
                                    string PTcharname = packet.ReadAscii();

                                    if (pt_no == Program.lpn_target_number)
                                    {
                                        Program.lpn_status = 0;
                                        Program.lpn_check_ptno = 0;
                                        Program.lpn_target_number = 0;
                                        String NoticeWin = Program.lpn_win_notice.Replace("%charname%", PTcharname);
                                        SendNotice(NoticeWin);

                                        if (Program.main.cbox_lpn_silk.Checked)
                                        {
                                            if (Program.main.lpn_silk.Text != "" || Program.main.lpn_silkpoint.Text != "" || Program.main.lpn_giftsilk.Text != "")
                                            {
                                                Program.rewards.GiveSilk(PTcharname, Convert.ToInt32(Program.main.lpn_silk.Text), Convert.ToInt32(Program.main.lpn_giftsilk.Text), Convert.ToInt32(Program.main.lpn_silkpoint.Text));
                                            }
                                        }

                                        if (Program.main.cbox_lpn_item.Checked)
                                        {
                                            if (Program.main.lpn_itemid.Text != "" || Program.main.lpn_quantity.Text != "" || Program.main.lpn_plus.Text != "")
                                            {
                                                Program.rewards.GiveItem(PTcharname, Convert.ToInt32(Program.main.lpn_itemid.Text), Convert.ToInt32(Program.main.lpn_quantity.Text), Convert.ToInt32(Program.main.lpn_plus.Text));
                                            }
                                        }

                                        if (Program.main.cbox_lpn_gold.Checked)
                                        {
                                            if (Program.main.lpn_gold.Text != "")
                                            {
                                                Program.rewards.GiveGold(PTcharname, Convert.ToInt32(Program.main.lpn_gold.Text));
                                            }
                                        }

                                        if (Program.main.cbox_lpn_title.Checked)
                                        {
                                            if (Program.main.lpn_title.Text != "")
                                            {
                                                Program.rewards.GiveTitle(PTcharname, Convert.ToInt32(Program.main.lpn_title.Text));
                                            }
                                        }
                                        Logger.LogIt("LPN Ödülleri verildi.", LogType.Normal);
                                        Program.HaveEvent = false;
                                    }
                                    /*packet.ReadUInt8Array(6);
                                    packet.ReadAscii();*/
                                }
                            }
                            catch { Console.WriteLine("Failed to lpn"); }
                        }
                    }

                    else if (packet.Opcode == 0x3026)
                    {
                        byte Type = packet.ReadUInt8();
                        if (Type == 02)
                        {
                            string Sender = packet.ReadAscii();
                            string Message = packet.ReadAscii();
                            Logger.LogIt(string.Format("{0} <== {1}", Sender, Message), LogType.PM);
                            if (Program.trivia_event && !QueryChecks.containsQuotes(Message))
                            {
                                try
                                {
                                    {
                                        string[] read = Program.event_db.getSingleArray("exec _AnswerControl '" + Sender + "', '" + Message + "'");
                                        {
                                            string tFalse = read[0];
                                            if (tFalse == "1")
                                            {
                                                if (Program.TriviaList.Contains(Sender))
                                                {
                                                    SendPM(Sender, Program.trivia_wrong2);

                                                }
                                                Program.trivia_event = false;

                                                if (Program.main.cbox_trivia_silk.Checked)
                                                {
                                                    if (Program.main.trivia_silk.Text != "" || Program.main.trivia_silkpoint.Text != "" || Program.main.trivia_giftsilk.Text != "")
                                                    {
                                                        Program.rewards.GiveSilk(Sender, Convert.ToInt32(Program.main.trivia_silk.Text), Convert.ToInt32(Program.main.trivia_giftsilk.Text), Convert.ToInt32(Program.main.trivia_silkpoint.Text));
                                                    }
                                                }

                                                if (Program.main.cbox_trivia_item.Checked)
                                                {
                                                    if (Program.main.trivia_itemid.Text != "" || Program.main.trivia_quantity.Text != "" || Program.main.trivia_plus.Text != "")
                                                    {
                                                        Program.rewards.GiveItem(Sender, Convert.ToInt32(Program.main.trivia_itemid.Text), Convert.ToInt32(Program.main.trivia_quantity.Text), Convert.ToInt32(Program.main.trivia_plus.Text));
                                                    }
                                                }

                                                if (Program.main.cbox_trivia_gold.Checked)
                                                {
                                                    if (Program.main.trivia_gold.Text != "")
                                                    {
                                                        Program.rewards.GiveGold(Sender, Convert.ToInt32(Program.main.trivia_gold.Text));
                                                    }
                                                }

                                                if (Program.main.cbox_trivia_title.Checked)
                                                {
                                                    if (Program.main.trivia_title.Text != "")
                                                    {
                                                        Program.rewards.GiveTitle(Sender, Convert.ToInt32(Program.main.trivia_title.Text));
                                                    }
                                                }
                                                string triviawin = Program.trivia_win.Replace("%charname%", Sender);
                                                SendGlobal(triviawin);
                                                Program.event_db.ExecuteCommand("update _questions set Service = 0");
                                                Program.TriviaList.Clear();
                                                Logger.LogIt("Trivia Ödülleri verildi.", LogType.Normal);
                                                Program.HaveEvent = false;
                                            }
                                            else if (tFalse == "0")
                                            {
                                                if (Program.TriviaList.Contains(Sender))
                                                {
                                                    SendPM(Sender, Program.trivia_wrong2);
                                                }
                                                else
                                                {
                                                    SendPM(Sender, Program.trivia_wrong);
                                                    Program.TriviaList.Add(Sender);
                                                }
                                            }
                                        }
                                    }
                                }
                                catch
                                {
                                    SendPM(Sender, "hata");
                                }
                            }
                            if (Program.HaveEvent && Program.main.toggle_lms.Checked && !QueryChecks.containsQuotes(Message))
                            {
                                try
                                {
                                    string toLower = Message.ToLower();
                                    var values = new[] { "/reg", "reg", "/registeration" };
                                    if (values.Any(toLower.Contains))
                                    {
                                        if (Program.lms_status == 1)
                                        {

                                            if (Program.lms_playerlist.IndexOf(Sender) == -1)
                                            {
                                                Program.lms_playerlist.Add(Sender);
                                                SendPM(Sender, "[Last Man Standing Event] Kayıt başarılı.");
                                            }
                                            else
                                            {
                                                SendPM(Sender, "[Last Man Standing Event] Zaten bir kaydın var.");
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {

                                    Logger.LogIt("Trivia: " + ex.ToString(), LogType.Hata);
                                }
                            }
                            if (!Program.HaveEvent && !Program.trivia_event && Program.chat_log)
                            {
                                Program.event_db.ExecuteCommand("exec [_RecvPm] '" + Sender + "', '" + Message + "'");
                            }
                        }

                    }


                    if (packet.Opcode == 0x3015)
                    {
                        try
                        {
                            Thread t = new Thread(() => CharSpawn.START_ParseChar(packet));
                            t.Start();
                        }
                        catch
                        {
                            Logger.LogIt("Error Single Spawn", LogType.Hata);
                        }

                    }
                    #endregion

                }
                else //For SwitchingService
                {
                    #region Ingame

                    //Teleport Request
                    if (packet.Opcode == 0x30D2)
                    {

                        //Wait for Client
                        //!!! Create Callback here, because this would freeze the thread resulting in connection lost when taking too long!!!
                        while (m_ClientWaitingForData == false && m_ClientWatingForFinish == false)
                        {
                            System.Threading.Thread.Sleep(1);
                        }

                        //Client is ready
                        if (m_ClientWaitingForData)
                        {
                            //Accept Teleport
                            Packet respone = new Packet(0x34B6);
                            m_agentSocket.Send(respone);

                            m_ClientWatingForFinish = true;

                            //!!! CrossThreading!!!
                        }
                    }

                    //Incoming CharacterData
                    if (packet.Opcode == 0x34A5)
                    {
                        if (m_ClientWatingForFinish)
                        {
                            m_SwitchClient = false;
                            m_Clientless = false;
                        }
                    }

                    #endregion
                }
            }
            else //Proxy Mode - Forwarding Packets between Client and Server
            {
                m_clientSocket.Send(packet);
            }
        }

        void m_agentSocket_Kicked()
        {
            m_AgentConnected = false;
            m_agentSocket.Disconnect();
            m_clientSocket.Disconnect();
            if (Log != null)
            {
                Log("Agent kicked");
            }
        }

        void m_agentSocket_Disconnected()
        {
            m_AgentConnected = false;
            if (Log != null)
            {
                Log("Agent disconnected");
            }
        }

        void m_agentSocket_Connected(string ip, ushort port)
        {
            m_AgentConnected = true;
            if (Log != null)
            {
                Log(string.Format("AgentServer connected to {0}:{1}", ip, port));
            }
        }

        #endregion

        #region Client-EventHandlers

        byte m_AgentLoginFixCounter;

        void m_clientSocket_PacketReceived(Packet packet)
        {
            //For ClientlessSwitcher
            if (m_SwitchClient)
            {
                #region Fake Client

                #region 0x2001

                if (packet.Opcode == 0x2001)
                {

                    //[S -> C][2001][16 bytes]
                    //0D 00 47 61 74 65 77 61 79 53 65 72 76 65 72 00   ..GatewayServer.
                    Packet response = new Packet(0x2001);
                    if (!m_ConnectToAgent)
                    {
                        response.WriteAscii("GatewayServer");
                    }
                    else
                    {
                        response.WriteAscii("AgentServer");
                        m_ConnectToAgent = false;
                    }

                    response.WriteUInt8(0); //Client-Connection
                    response.Lock();
                    m_clientSocket.Send(response);

                    //S->P:2005 Data:01 00 01 BA 02 05 00 00 00 02
                    response = new Packet(0x2005, false, true);
                    response.WriteUInt8(0x01);
                    response.WriteUInt8(0x00);
                    response.WriteUInt8(0x01);
                    response.WriteUInt8(0xBA);
                    response.WriteUInt8(0x02);
                    response.WriteUInt8(0x05);
                    response.WriteUInt8(0x00);
                    response.WriteUInt8(0x00);
                    response.WriteUInt8(0x00);
                    response.WriteUInt8(0x02);
                    response.Lock();
                    m_clientSocket.Send(response);

                    //S->P:6005 Data:03 00 02 00 02
                    response = new Packet(0x6005, false, true);
                    response.WriteUInt8(0x03);
                    response.WriteUInt8(0x00);
                    response.WriteUInt8(0x02);
                    response.WriteUInt8(0x00);
                    response.WriteUInt8(0x02);
                    response.Lock();
                    m_clientSocket.Send(response);
                }

                #endregion

                #region 0x6100

                if (packet.Opcode == 0x6100)
                {
                    byte local = packet.ReadUInt8();
                    string client = packet.ReadAscii();
                    uint version = packet.ReadUInt32();

                    //S->P:A100 Data:01
                    Packet response = new Packet(0xA100, false, true);

                    if (local != m_ClientLocal)
                    {
                        response.WriteUInt8(0x02); //Faild
                        response.WriteUInt8(0x01); //Faild to connect to server.(C4)                   
                    }
                    else if (client != "SR_Client")
                    {
                        response.WriteUInt8(0x02); //Faild
                        response.WriteUInt8(0x03); //Faild to connect to server.(C4)                 
                    }
                    else if (version != m_ClientVersion)
                    {
                        response.WriteUInt8(0x02); //Faild
                        response.WriteUInt8(
                            0x02); //Update - Missing bytes but still trigger update message on Client, launcher will crash :/
                    }
                    else
                    {
                        response.WriteUInt8(0x01); //Sucess
                    }

                    response.Lock();
                    m_clientSocket.Send(response);
                }

                #endregion

                #region 0x6101

                if (packet.Opcode == 0x6101 && m_ConnectToAgent == false)
                {
                    Packet response = new Packet(0xA102);
                    response.WriteUInt8(0x01); //Sucess
                    response.WriteUInt32(uint.MaxValue); //SessionID
                    response.WriteAscii("127.0.0.1"); //AgentIP
                    response.WriteUInt16(m_LocalPort);
                    response.Lock();

                    m_ConnectToAgent = true;
                    m_clientSocket.Send(response);
                }

                #endregion

                #region 0x6103

                if (packet.Opcode == 0x6103)
                {
                    //FF FF FF FF 00 00 00 00 16 00 00 9D 53 84 00
                    uint sessionID = packet.ReadUInt32();
                    string username = packet.ReadAscii();
                    string password = packet.ReadAscii();
                    byte local = packet.ReadUInt8();
                    //byte[] mac = packet.ReadByteArray(6); //No need

                    Packet response = new Packet(0xA103);
                    if (sessionID != uint.MaxValue)
                    {
                        response.WriteUInt8(0x02);
                        response.WriteUInt8(0x02);
                    }
                    else if (username != "")
                    {
                        response.WriteUInt8(0x02);
                        response.WriteUInt8(0x02);
                    }
                    else if (password != "")
                    {
                        response.WriteUInt8(0x02);
                        response.WriteUInt8(0x02);
                    }
                    else if (local != m_ClientLocal)
                    {
                        response.WriteUInt8(0x02);
                        response.WriteUInt8(0x02);
                    }
                    else
                    {
                        response.WriteUInt8(0x01); //Sucess
                    }

                    response.Lock();
                    m_clientSocket.Send(response);
                }

                #endregion

                #region 0x7007

                if (packet.Opcode == 0x7007)
                {
                    byte type = packet.ReadUInt8();
                    if (type == 0x02)
                    {
                        Packet responseEndCS = new Packet(0xB001);
                        responseEndCS.WriteUInt8(0x01);

                        Packet responseInitLoad = new Packet(0x34A5);

                        m_clientSocket.Send(responseEndCS);
                        m_clientSocket.Send(responseInitLoad);
                        m_ClientWaitingForData = true;
                    }
                }

                #endregion

                #endregion
            }
            else
            {
                //Not sure why but after clientless->client the clients preferes to send 0x6103 twice.
                if (packet.Opcode == 0x6103)
                {
                    if (m_AgentLoginFixCounter > 0)
                    {
                        return;
                    }

                    m_AgentLoginFixCounter++;
                }

                if (packet.Opcode == 0x6102)
                {
                    m_AgentLoginFixCounter = 0;
                }

                if (m_AgentConnected)
                {
                    m_agentSocket.Send(packet);
                }
                else
                {
                    m_gatewaySocket.Send(packet);
                }
            }
        }

        void m_clientSocket_Disconnected()
        {
            m_ClientConnected = false;

            if (!m_AgentConnected)
            {
                m_gatewaySocket.Disconnect();
            }
        }

        void m_clientSocket_Connected()
        {
            m_ClientConnected = true;

            //If a clients connects while having a clientless connection enable clientless switch
            if (m_Clientless)
            {
                m_SwitchClient = true;

                m_ClientWaitingForData = false;
                m_ClientWatingForFinish = false;
            }
            else
            {
                // m_doClientlessSwitch = false;                   

                if (m_ConnectToAgent)
                {
                    if (m_GatewayConnected)
                        m_gatewaySocket.Disconnect();

                    m_ConnectToAgent = false;
                    m_ClientWaitingForData = false;
                    m_ClientWatingForFinish = false;

                    m_agentSocket.Connect(m_AgentIP, m_AgentPort);
                }
                else
                {
                    if (m_AgentConnected)
                        m_agentSocket.Disconnect();
                    m_gatewaySocket.Connect(m_GatewayIP, m_GatewayPort);
                }
            }
        }

        #endregion


        internal void SendCharacterSelection(Character character)
        {
            Packet packet = new Packet(0x7001);
            packet.WriteAscii(character.Name);
            packet.Lock();
            m_agentSocket.Send(packet);
        }

        public async void EventTime()
        {
            try
            {
                while (Program.AlreadyStarted)
                {
                    string hour = DateTime.Now.ToString("HH:mm");
                    string day = CultureInfo.CurrentCulture.DateTimeFormat.DayNames[(int)DateTime.Now.DayOfWeek];

                    using (SqlDataReader dr = Program.event_db.ExecuteReader("select * from event_time"))
                    {
                        while (dr.Read())
                        {
                            string dbHour = Convert.ToString(dr["Time"]);
                            string Type = Convert.ToString(dr["Type"]);
                            string dbDay = Convert.ToString(dr["Day"]);

                            if (hour == dbHour && day == dbDay)
                            {
                                switch (Type)
                                {
                                    case "Trivia":
                                        if (!Program.HaveEvent && Program.main.toggle_trivia.Checked)
                                        {
                                            new Thread(eTrivia.TriviaSystem).Start();
                                            Logger.LogIt("Trivia Event başladı.", LogType.Normal);
                                            Program.HaveEvent = true;
                                        }
                                        break;
                                    case "LPN":
                                        if (!Program.HaveEvent && Program.main.toggle_lpn.Checked)
                                        {
                                            SendNotice("[Event Lucky Party Event]  Event Will Start Now ");
                                            //MakePTMatch();
                                            Program.lpn_status = 1;
                                            new Thread(LpnDelay).Start();
                                            new Thread(LpnReForm).Start();
                                            Program.HaveEvent = true;
                                        }
                                        break;
                                    case "SnD":
                                        if (!Program.HaveEvent && Program.main.toggle_kayipunique.Checked)
                                        {
                                            SendNotice("[Search And Destroy Event] başlıyor!");
                                            new Thread(SnDEvent).Start();
                                            Program.HaveEvent = true;
                                        }
                                        break;
                                    case "GmKill":
                                        if (!Program.HaveEvent && Program.main.toggle_gmkill.Checked)
                                        {
                                            SendNotice("[GM Killer Event] Başlıyor.");
                                            new Thread(Cape).Start();
                                            Program.HaveEvent = true;
                                        }
                                        break;
                                    case "Alchemy":
                                        if (!Program.HaveEvent && Program.main.toggle_alchemy.Checked)
                                        {
                                            SendNotice("[Alchemy Event] Başlıyor.");
                                            new Thread(AlchemyEvent).Start();
                                            Program.HaveEvent = true;
                                        }
                                        break;
                                    case "LMS":
                                        if (!Program.HaveEvent && Program.main.toggle_lms.Checked)
                                        {
                                            SendNotice("[Last Man Standing Event] Başlıyor.");
                                            new Thread(LmsEvent).Start();
                                            Program.lms_status = 1;
                                            Program.HaveEvent = true;
                                        }
                                        break;
                                    case "HnS":
                                        if (!Program.HaveEvent && Program.main.toggle_kayipgm.Checked)
                                        {
                                            SendNotice("[Hide and Seek Event] Başlıyor.");
                                            new Thread(HideANDSickEvent).Start();
                                            Program.kayipgm_status = 1;
                                            Program.HaveEvent = true;
                                        }
                                        break;
                                    case "Unique":
                                        if (!Program.HaveEvent && Program.main.toggle_unique.Checked)
                                        {
                                            SendNotice("[Unique Event] Başlıyor.");
                                            new Thread(UniqueSpawnEvent).Start();
                                            Program.HaveEvent = true;
                                            Program.ilk = true;
                                            Program.uniquespawn_status = 1;
                                        }
                                        break;
                                }
                            }
                        }
                    }
                    await Task.Delay(59500);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        public void AutoNotice()
        {
            try
            {
                while (true)
                {

                    using (SqlDataReader r = Program.event_db.ExecuteReader("select * from _Notice where Service = 0"))
                    {
                        while (r.Read())
                        {
                            string Message = Convert.ToString(r["Message"]);
                            SendNotice(Message);
                            Program.event_db.ExecuteCommand("update _Notice set Service = 1 where Message = '" + Message + "'");

                            if (Message.Contains("[Last Man Standing Kaybeden]"))
                            {
                                string PN = Program.getBetween(Message, "Tarafından Öldürüldü [", "]");
                                Thread t = new Thread(() => PlayertoTown_Double(PN));
                                t.Start();
                                LMS_REMOVE_CHEATER(PN);
                                // Logger.LogIt("Dead P:" + PN, LogType.Normal);
                            }
                            else if (Message.Contains("[Last Man Standing Kazanan]"))
                            {
                                Program.lms_status = 0;
                                Program.lms_playerlist.Clear();
                                Program.lms_recalledplayers.Clear();
                                string PN = Program.getBetween(Message, " Kazanan] [", "] Eventi");
                                Program.event_db.ExecuteCommand("truncate Table _LMSPlayers");

                                PlayertoTown(PN);
                                Gotown();
                                if (Program.main.cbox_lms_silk.Checked)
                                {
                                    if (Program.main.lms_silk.Text != "" || Program.main.lms_silkpoint.Text != "" || Program.main.lms_giftsilk.Text != "")
                                    {
                                        Program.rewards.GiveSilk(PN, Convert.ToInt32(Program.main.lms_silk.Text), Convert.ToInt32(Program.main.lms_giftsilk.Text), Convert.ToInt32(Program.main.lms_silkpoint.Text));
                                    }
                                }

                                if (Program.main.cbox_lms_item.Checked)
                                {
                                    if (Program.main.lms_itemid.Text != "" || Program.main.lms_quantity.Text != "" || Program.main.lms_plus.Text != "")
                                    {
                                        Program.rewards.GiveItem(PN, Convert.ToInt32(Program.main.lms_itemid.Text), Convert.ToInt32(Program.main.lms_quantity.Text), Convert.ToInt32(Program.main.lms_plus.Text));
                                    }
                                }

                                if (Program.main.cbox_lms_gold.Checked)
                                {
                                    if (Program.main.lms_gold.Text != "")
                                    {
                                        Program.rewards.GiveGold(PN, Convert.ToInt32(Program.main.lms_gold.Text));
                                    }
                                }

                                if (Program.main.cbox_lms_title.Checked)
                                {
                                    if (Program.main.lms_title.Text != "")
                                    {
                                        Program.rewards.GiveTitle(PN, Convert.ToInt32(Program.main.lms_title.Text));
                                    }
                                }
                                Program.HaveEvent = false;

                            }
                        }
                    }

                    //Program.event_db.ExecuteCommand("exec proc");
                    Thread.Sleep(1000);
                }
            }
            catch (Exception ex) { Console.WriteLine("{0}", ex.Message); }
        }

        public void AutoPm()
        {
            try
            {
                while (true)
                {

                    using (SqlDataReader r = Program.event_db.ExecuteReader("select * from _SendPm where Service = 0"))
                    {
                        while (r.Read())
                        {
                            string Target = Convert.ToString(r["CharName"]);
                            string Message = Convert.ToString(r["Msg"]);
                            SendPM(Target, Message);
                            Program.event_db.ExecuteCommand("update _SendPm set Service = 1 where Msg = '" + Message + "'");
                        }
                    }
                    Thread.Sleep(1000);
                }
            }
            catch (Exception ex) { Console.WriteLine("{0}", ex.Message); }
        }

        public async void LmsEvent() // Last Man Standing
        {
            try
            {
                string fnotice = Program.lms_firstnotice.Replace("%plus%", Convert.ToString(Program.lms_lvllimit));
                SendNotice(fnotice);
                await Task.Delay(5000);
                Packet packet = new Packet(0x7010); // move to place
                packet.WriteUInt8(0x10); //static 
                packet.WriteUInt8(0); //static 
                packet.WriteInt16(Program.main.lms_region.Text); //regionID 
                packet.WriteSingle(Program.main.lms_posx.Text); //x 
                packet.WriteSingle(Program.main.lms_posy.Text); //Y 
                packet.WriteSingle(Program.main.lms_posz.Text); //Z 
                packet.WriteInt8(1); //worldid 
                packet.WriteUInt8(0); //static 
                m_agentSocket.Send(packet);
                await Task.Delay(55000);
                SendGlobal(Program.lms_regglobal);
                await Task.Delay((int)TimeSpan.FromMinutes(Convert.ToInt32(Program.lms_regtime)).TotalMilliseconds); // dakika
                SendGlobal(Program.lms_closeglobal);
                await Task.Delay(5000);
                Program.event_db.ExecuteCommand("truncate Table _LMSPlayers");
                RecallPlayer_LMS_EVENT();
                await Task.Delay(20000);
                SendNotice(Program.lms_startnotice);
                await Task.Delay(40000);
                new Thread(LMS_CHECK_CapOn).Start();
            }
            catch { Logger.LogIt("Error lms event ", LogType.Hata); }

        }
        public void RecallPlayer_LMS_EVENT() // None Job Suit + Required Level
        {
            try
            {
                Program.lms_playerlist.ForEach(delegate (String name)
                {
                    using (SqlDataReader r = Program.event_db.ExecuteReader("EXEC LMSCheckSuit '" + name + "'"))
                    {
                        while (r.Read())
                        {
                            String ItemID = (r["ItemID"]).ToString();
                            int Plvl = Convert.ToInt32(r["CurLevel"]);
                            if ((ItemID != "0") && (Plvl <= Program.lms_lvllimit))
                            {

                                SendPM(name, "Gelecek sefere Job suit takmayınız ve level limitinin üzerinde olunuz! " + Program.lms_lvllimit + " To join it!");
                            }
                            else
                            {
                                RecallPlayer(name);
                                if (Program.lms_recalledplayers.IndexOf(name) == -1)
                                {
                                    Program.lms_recalledplayers.Add(name);
                                }
                                Program.event_db.ExecuteCommand("INSERT INTO [EventBot].[dbo].[_LMSPlayers] (Service,CharName) VALUES (1,'" + name + "')");
                                Logger.LogIt("[Last Man Standing ] RecallPlayer " + name, LogType.Normal);

                            }
                        }
                    }
                });
                new Thread(LMS_CHECK_Time).Start();

            }
            catch { Logger.LogIt("Error lms recalluser ", LogType.Hata); }
        }
        public void LMS_REMOVE_CHEATER(string CheaterName)
        {
            try
            {
                for (int i = 0; i < Program.lms_recalledplayers.Count; i++)
                {
                    if (Program.lms_recalledplayers[i].Contains(CheaterName))
                    {
                        int AlreadyIndex = Program.lms_recalledplayers.IndexOf(CharStrings.CharNameANDuniqueID[i]);
                        Program.lms_recalledplayers.RemoveAt(AlreadyIndex);
                    }
                }
            }
            catch (Exception ex)
            {
                // Logger.LogIt("LmsRemove  " +ex.ToString(), LogType.Hata); 
            }
        }

        public async void LMS_CHECK_Time()
        {
            await Task.Delay((int)TimeSpan.FromMinutes(Convert.ToInt32(Program.lms_matchtime)).TotalMilliseconds); // dakika
            if (Program.lms_status == 1)
            {
                Program.lms_status = 0;
                Program.lms_playerlist.Clear();
                Program.lms_recalledplayers.Clear();
                SendNotice(Program.lms_nowin);

                using (SqlDataReader r = Program.event_db.ExecuteReader("Declare @Count int = ( Select Count(*) From _LMSPlayers ) IF (@Count = 1 ) BEGIN Select Charname From _LMSPlayers  END"))
                {
                    while (r.Read())
                    {
                        string PlayerNamePm = Convert.ToString(r["Charname"]);
                        PlayertoTown(PlayerNamePm);
                    }
                }

                Program.event_db.ExecuteCommand("truncate Table _LMSPlayers");
                Program.HaveEvent = false;
                Gotown();
            }
        }
        public async void LMS_CHECK_CapOn()
        {

            foreach (string Player in Program.lms_recalledplayers) // Loop through List with foreach.
            {
                using (SqlDataReader r = Program.event_db.ExecuteReader("Declare @yes int = 1 Declare @no int = 0 if EXISTS ( Select Charname From _LMSPlayers Where CharName like '" + Player + "') BEGIN Select @yes as [InOrNo] END ELSE BEGIN Select @no as [InOrNo] END"))
                {
                    while (r.Read())
                    {
                        int YesOrNo = Convert.ToInt32(r["InOrNo"]);
                        if (YesOrNo == 0)
                        {
                            SendPM(Player, "40 saniyede cape takmış olman gerekiyor.");
                            PlayertoTown(Player);
                        }
                    }
                }

                await Task.Delay(50);
            }
        }


        public async void HideANDSickEvent()
        {
            try
            {
                String RegionID, X, Y, Z, PlaceName;
                using (SqlDataReader r = Program.event_db.ExecuteReader("SELECT top 1* from hideandseek_regions ORDER BY NEWID ()"))
                {
                    while (r.Read())
                    {
                        RegionID = Convert.ToString(r["RegionID"]);
                        X = Convert.ToString(r["X"]);
                        Y = Convert.ToString(r["Y"]);
                        Z = Convert.ToString(r["Z"]);
                        PlaceName = Convert.ToString(r["Place_Name"]);
                        await Task.Delay(4000);

                        Packet packet = new Packet(0x7010); // move to place
                        packet.WriteUInt8(0x10); //static 
                        packet.WriteUInt8(0); //static 
                        packet.WriteInt16(RegionID); //regionID 
                        packet.WriteSingle(X); //x 
                        packet.WriteSingle(Y); //Y 
                        packet.WriteSingle(Z); //Z 
                        packet.WriteInt8(1); //worldid 
                        packet.WriteUInt8(0); //static 
                        m_agentSocket.Send(packet);

                        await Task.Delay(4000);
                        String Snotice = Program.kayipgm_startnotice.Replace("%placename%", PlaceName);
                        SendNotice(Snotice);
                        await Task.Delay(4000);
                        SendNotice(Program.kayipgm_timenotice);
                        Program.kayipgm_status = 1;
                        Packet p = new Packet(0x7010); // invisible
                        p.WriteUInt8(0x0E);
                        p.WriteUInt8(0x00);
                        m_agentSocket.Send(p);

                        new Thread(HIDE_SEEK_DELAY).Start();
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        public async void HIDE_SEEK_DELAY()  // hide and seek Wait
        {
            await Task.Delay((int)TimeSpan.FromMinutes(Convert.ToInt32(Program.main.kayipgm_wait.Text)).TotalMilliseconds); // dakika
            if (Program.kayipgm_status == 1)
            {
                SendNotice(Program.kayipgm_nowin);
                Program.kayipgm_status = 0;
                await Task.Delay(1000);
                Gotown();
            }
        }

        public async void UniqueSpawnEvent()
        {
            int mobid = Convert.ToInt32(Program.main.listView_unique.Items[Program.MobSira].SubItems[0].Text);
            int count = Convert.ToInt32(Program.main.listView_unique.Items[Program.MobSira].SubItems[1].Text);
            if (Program.main.listView_unique.Items.Count > 0)
            {
                if (Program.ilk)
                {

                    string firstnotice = Program.uniqueevent_firstnotice.Replace("%placename%", Program.main.unique_town.Text);
                    SendNotice(firstnotice);
                    await Task.Delay(5000);
                    Packet packet = new Packet(0x7010); // move to place
                    packet.WriteUInt8(0x10); //static 
                    packet.WriteUInt8(0); //static 
                    packet.WriteInt16(Program.main.unique_regionid.Text); //regionID 
                    packet.WriteSingle(Program.main.unique_posx.Text); //x 
                    packet.WriteSingle(Program.main.unique_posy.Text); //Y 
                    packet.WriteSingle(Program.main.unique_posz.Text); //Z 
                    packet.WriteInt8(1); //worldid 
                    packet.WriteUInt8(0); //static 
                    m_agentSocket.Send(packet);
                    await Task.Delay(5000);
                    Program.ilk = false;
                    Packet packetSpawn = new Packet(0x7010); // Spawn Unique Packet
                    packetSpawn.WriteUInt8((byte)6);
                    packetSpawn.WriteUInt8((byte)0);
                    packetSpawn.WriteUInt16(mobid); // mob ID
                    packetSpawn.WriteUInt8((byte)0);
                    packetSpawn.WriteUInt8((byte)0);
                    packetSpawn.WriteUInt8(count); // MOB COUNT
                    packetSpawn.WriteUInt8((byte)3);
                    m_agentSocket.Send(packetSpawn);
                    await Task.Delay(1000);
                    Program.main.StartTimers();


                }
                else if ((ulong)Program.mobid == (ulong)((long)mobid))
                {
                    Program.mobsay++;
                    if (Program.mobsay == count)
                    {
                        Program.mobsay = 0;
                        Program.MobSira++;
                        if (Program.MobSira < Program.main.listView_unique.Items.Count)
                        {
                            int mobid1 = Convert.ToInt32(Program.main.listView_unique.Items[Program.MobSira].SubItems[0].Text);
                            int count1 = Convert.ToInt32(Program.main.listView_unique.Items[Program.MobSira].SubItems[1].Text);
                            Packet packetSpawn = new Packet(0x7010); // Spawn Unique Packet
                            packetSpawn.WriteUInt8((byte)6);
                            packetSpawn.WriteUInt8((byte)0);
                            packetSpawn.WriteUInt16(mobid1); // mob ID
                            packetSpawn.WriteUInt8((byte)0);
                            packetSpawn.WriteUInt8((byte)0);
                            packetSpawn.WriteUInt8(count1); // MOB COUNT
                            packetSpawn.WriteUInt8((byte)3);
                            m_agentSocket.Send(packetSpawn);
                        }
                        else
                        {
                            Program.HaveEvent = false;
                            Program.uniquespawn_status = 0;
                            Program.ilk = false;
                            SendNotice(Program.uniqueevent_end);
                            Gotown();
                        }
                    }
                }
            }
        }


        public async void AlchemyEvent()
        {
            try
            {

                Packet packet = new Packet(0x7010);
                packet.WriteUInt8(0x10); //static 
                packet.WriteUInt8(0); //static 
                packet.WriteInt16(24744); //regionID 
                packet.WriteSingle(988); //x 
                packet.WriteSingle(-6.789643); //Y 
                packet.WriteSingle(1414); //Z 
                packet.WriteInt8(1); //worldid 
                packet.WriteUInt8(0); //static 
                SendNotice(Program.alchemy_startnotice);
                m_agentSocket.Send(packet);
                await Task.Delay(4000);
                Packet p = new Packet(0x7010);
                p.WriteUInt8(0x0F);
                p.WriteUInt8(0x00);
                m_agentSocket.Send(p);
                await Task.Delay(3000);
                Program.event_db.ExecuteCommand("exec _AlchemyEventStatus 1");
                new Thread(MakeItem).Start();
                SendNotice(Program.alchemy_r1notice);
                await Task.Delay(150000);
                new Thread(MakeItem).Start();
                SendNotice(Program.alchemy_r2notice);
                await Task.Delay(150000);
                new Thread(MakeItem).Start();
                SendNotice(Program.alchemy_r3notice);
                string waitnotice = Program.alchemy_waitnotice.Replace("%min%", Program.main.alchemy_wait.Text);
                SendNotice(waitnotice);
                Packet p2 = new Packet(0x7010);
                p2.WriteUInt8(0x0E);
                p2.WriteUInt8(0x00);
                m_agentSocket.Send(packet);
                await Task.Delay((int)TimeSpan.FromMinutes(Convert.ToInt32(Program.main.alchemy_wait.Text)).TotalMilliseconds); // dakika

                SendNotice(Program.alchemy_finishnotice);
                // Program.event_db.ExecuteCommand("exec _AlchemyEventStatus 0");
                await Task.Delay(3000);
                using (SqlDataReader reader = Program.event_db.ExecuteReader("exec _AlchemyEventStatus 0"))
                {
                    while (reader.Read())
                    {
                        string charname = Convert.ToString(reader["CharName"]);
                        string plus = Convert.ToString(reader["Plus"]);
                        if (charname == "NULL")
                        {
                            SendNotice(Program.alchemy_nowin);
                        }
                        else
                        {
                            string notice_win = Program.alchemy_win.Replace("%charname%", charname).Replace("%plus%", plus);
                            SendNotice(notice_win);
                            if (Program.main.cbox_alchemy_silk.Checked)
                            {
                                if (Program.main.alchemy_silk.Text != "" || Program.main.alchemy_silkpoint.Text != "" || Program.main.alchemy_giftsilk.Text != "")
                                {
                                    Program.rewards.GiveSilk(charname, Convert.ToInt32(Program.main.alchemy_silk.Text), Convert.ToInt32(Program.main.alchemy_giftsilk.Text), Convert.ToInt32(Program.main.alchemy_silkpoint.Text));
                                }
                            }

                            if (Program.main.cbox_alchemy_item.Checked)
                            {
                                if (Program.main.alchemy_itemid.Text != "" || Program.main.alchemy_quantity.Text != "" || Program.main.alchemy_plus.Text != "")
                                {
                                    Program.rewards.GiveItem(charname, Convert.ToInt32(Program.main.alchemy_itemid.Text), Convert.ToInt32(Program.main.alchemy_quantity.Text), Convert.ToInt32(Program.main.alchemy_plus.Text));
                                }
                            }

                            if (Program.main.cbox_alchemy_gold.Checked)
                            {
                                if (Program.main.alchemy_gold.Text != "")
                                {
                                    Program.rewards.GiveGold(charname, Convert.ToInt32(Program.main.alchemy_gold.Text));
                                }
                            }

                            if (Program.main.cbox_alchemy_title.Checked)
                            {
                                if (Program.main.alchemy_title.Text != "")
                                {
                                    Program.rewards.GiveTitle(charname, Convert.ToInt32(Program.main.alchemy_title.Text));
                                }
                            }
                            Logger.LogIt("Alchemy ödülleri verildi.", LogType.Normal);
                        }
                    }
                }
                Program.HaveEvent = false;
                Gotown();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public void MakeItem()
        {
            string[] dr = Program.event_db.getSingleArray("select ItemID,ItemCount from _AlchemyItems where Service = 1");
            Thread.Sleep(2000);
            if (dr != null)
            {
                int say = 0;
                int tekrar = Convert.ToInt16(dr[1]);
                while (true)
                {
                    say = say + 1;
                    Packet packet = new Packet(0x7010);
                    packet.WriteUInt8(07);
                    packet.WriteUInt8(00);
                    packet.WriteUInt32(dr[0]);
                    packet.WriteUInt8(0);
                    m_agentSocket.Send(packet);
                    if (say >= tekrar)
                    {
                        break;
                    }
                }
            }
        }
        public async void Cape()
        {

            Program.gmkill_status = 1;
            await Task.Delay(3000);
            string start_notice = Program.gmkill_firstnotice.Replace("%placename%", Program.main.tbox_gmkill_town.Text);
            SendNotice(start_notice);
            await Task.Delay(60000);
            Packet packet3 = new Packet(0x7010);
            packet3.WriteUInt8(0x10); //static 
            packet3.WriteUInt8(0); //static 
            packet3.WriteInt16(26009); //regionID 
            packet3.WriteSingle(903); //x 
            packet3.WriteSingle(-103.749008); //Y 
            packet3.WriteSingle(1614); //Z
            packet3.WriteUInt8(1); //worldid 
            packet3.WriteUInt8(0); //static 
            m_agentSocket.Send(packet3);
            await Task.Delay(1000);
            string running_notice = Program.gmkill_running.Replace("%placename%", Program.main.tbox_gmkill_town.Text);
            SendNotice(running_notice);
            await Task.Delay(1000);
            Packet packet2 = new Packet(0x7516);
            packet2.WriteUInt8(0x05);
            m_agentSocket.Send(packet2);
            await Task.Delay(10500);
            Packet packet = new Packet(0x7010);
            packet.WriteUInt8(0x0e);
            packet.WriteUInt8(0x00);
            m_agentSocket.Send(packet);

            new Thread(Gmkill_Delay).Start();
        }

        public async void CapeOff()
        {
            Program.gmkill_status = 0;

            Packet p = new Packet(0x3053);
            p.WriteUInt8(0x01);
            m_agentSocket.Send(p);
            await Task.Delay(3000);
            Gotown();
            await Task.Delay(3000);
            SendNotice(Program.gmkill_finish);
            Program.HaveEvent = false;
        }
        public async void Gmkill_Delay()
        {
            await Task.Delay((int)TimeSpan.FromMinutes(Convert.ToInt32(Program.main.gmkill_wait.Text)).TotalMilliseconds); // dakika
            if (Program.gmkill_status == 1)
            {
                Packet p = new Packet(0x7010);
                p.WriteUInt32(0x03);
                p.WriteAscii(Program.main.cbKarakter.Text);
                m_agentSocket.Send(p);
                await Task.Delay(1000);
                SendNotice(Program.gmkill_end);
                Program.gmkill_status = 0;
                Gotown();
                Program.HaveEvent = false;
            }
        }

        public async void SnDEvent() // Search and destroy
        {
            try
            {
                String regionid, posx, posy, posz, place_name;
                int mobid;
                using (SqlDataReader reader = Program.event_db.ExecuteReader("SELECT top 1* from unique_regions ORDER BY NEWID ()"))
                {
                    while (reader.Read())
                    {
                        regionid = Convert.ToString(reader["region_id"]);
                        posx = Convert.ToString(reader["pos_x"]);
                        posy = Convert.ToString(reader["pos_y"]);
                        posz = Convert.ToString(reader["pos_z"]);
                        place_name = Convert.ToString(reader["name"]);
                        mobid = Convert.ToInt16(reader["unique_id"]);
                        Program.snd_mobid = mobid;
                        await Task.Delay(4000);
                        Packet packet = new Packet(0x7010); //movetoplace
                        packet.WriteUInt8(0x10); //static 
                        packet.WriteUInt8(0); //static 
                        packet.WriteUInt16(regionid); //regionid 
                        packet.WriteSingle(posx); //x 
                        packet.WriteSingle(posy); //Y 
                        packet.WriteSingle(posz); //Z 
                        packet.WriteUInt8(1); //worldid 
                        packet.WriteUInt8(0); //static 
                        m_agentSocket.Send(packet);
                        await Task.Delay(4000);
                        String sndnotice = Program.snd_firstnotice.Replace("%placename%", place_name);
                        SendNotice(sndnotice);
                        await Task.Delay(4000);
                        string snotice = Program.snd_wait_notice.Replace("%min%", Program.main.tbox_snd_wait.Text);
                        SendNotice(snotice);
                        Program.snd_status = 1;
                        Packet packetSpawn = new Packet(0x7010); //uniquespawn
                        packetSpawn.WriteUInt8(6);
                        packetSpawn.WriteUInt8(0);
                        packetSpawn.WriteUInt32(mobid); //mobid
                        packetSpawn.WriteUInt8(0);
                        packetSpawn.WriteUInt8(0);
                        packetSpawn.WriteUInt8(1); //count
                        packetSpawn.WriteUInt8(3);
                        m_agentSocket.Send(packetSpawn);
                        new Thread(SnD_Delay).Start();
                    }
                }

            }
            catch (Exception ex)
            {
                Logger.LogIt("SnD " + ex.ToString(), LogType.Hata);
            }
        }

        public async void SnD_Delay()  // Search and destroy Wait
        {
            await Task.Delay((int)TimeSpan.FromMinutes(Convert.ToInt32(Program.main.tbox_snd_wait.Text)).TotalMilliseconds); // dakika
            if (Program.snd_status == 1)
            {
                SendNotice(Program.snd_nowin_notice);
                Program.snd_mobid = 0;
                Program.snd_status = 0;
                await Task.Delay(1000);
                Gotown();
                Program.HaveEvent = false;
            }
        }


        public async void LpnDelay()
        {
            await Task.Delay((int)TimeSpan.FromMinutes(Convert.ToInt32(Program.main.lpn_wait.Text)).TotalMilliseconds); // dakika

            if (Program.lpn_status == 1)
            {
                Program.lpn_status = 0;
                Program.lpn_check_ptno = 0;
                Program.lpn_target_number = 0;
                SendNotice(Program.lpn_nowin_notice);
                Program.HaveEvent = false;

            }
        }

        public async void LpnReForm()
        {
            if (Program.lpn_status == 1)
            {
                await Task.Delay(600);
                Packet p3 = new Packet(0x706C);
                p3.WriteUInt32(0x00);
                m_agentSocket.Send(p3);
                new Thread(LpnReForm).Start();
            }
        }


        public void SendGlobal(string message)
        {
            try
            {
                var random = new Random();
                int index = random.Next(CharStrings.GlobalsTypeSlot.Count);
                String GlobalSlot = CharStrings.GlobalsTypeSlot[index];
                string[] tttxt;
                string TType = string.Empty, slot = string.Empty;
                int Count = 0;
                if (GlobalSlot != "")
                {
                    tttxt = GlobalSlot.Split(',');
                    TType = tttxt[0];
                    Count = (Convert.ToInt32(tttxt[1]));
                    slot = tttxt[2];
                    CharStrings.GlobalsTypeSlot.Remove(GlobalSlot);

                    Packet packet = new Packet(0x704C, true);
                    packet.WriteUInt8(byte.Parse(slot));
                    packet.WriteUInt8(0xEC);
                    packet.WriteUInt8(0x29);
                    packet.WriteAscii(message);
                    m_agentSocket.Send(packet);
                    Logger.LogIt(message, LogType.Global);
                    Count = Count - 1;
                    CharStrings.GlobalsTypeSlot.Add(TType + "," + Count + "," + slot);
                }
                else
                {
                    Logger.LogIt("Karakter üzerinde Global bulunamadı. CodeName128: ITEM_EVENT_RENT_GLOBAL_CHATTING", LogType.Hata);
                }
            }
            catch (Exception ex)
            {
                Logger.LogIt(ex.ToString(), LogType.Hata);

            }
        }

        public void SendNotice(string Message)
        {
            Packet packet = new Packet(0x7025);
            packet.WriteUInt8(7);
            packet.WriteUInt8(2);
            packet.WriteAscii(Message);
            m_agentSocket.Send(packet);
            Logger.LogIt(Message, LogType.Notice);
        }

        public void SendPM(string Target, string Message)
        {
            try
            {
                Packet packet = new Packet(0x7025);
                packet.WriteUInt8(0x02);
                packet.WriteUInt8(0x00);
                packet.WriteAscii(Target);
                packet.WriteAscii(Message);
                m_agentSocket.Send(packet);
                Logger.LogIt($"{Target} ==> {Message}", LogType.PM);
            }
            catch
            {
                Logger.LogIt("Pm Hatası", LogType.Hata);
            }
        }

        public void AllChat(string Message)
        {
            Packet packet = new Packet(0x7025);
            packet.WriteUInt32(0x03);
            packet.WriteUInt32(0x00);
            packet.WriteAscii(Message);
            m_agentSocket.Send(packet);

        }

        public void Gotown()
        {
            Packet packet = new Packet(0x7010, true);
            packet.WriteUInt16(0x02);
            packet.WriteAscii(Program.main.cbKarakter.Text);
            m_agentSocket.Send(packet);
        }

        public void PlayertoTown(String PlayerName)
        {
            Packet packet = new Packet(0x7010, true);
            packet.WriteUInt16(0x03);
            packet.WriteAscii(PlayerName);
            m_agentSocket.Send(packet);
        }

        public void RecallPlayer(string PlayerName)
        {
            try
            {
                Packet packet = new Packet(0x7010);
                packet.WriteUInt16(0x11);
                packet.WriteAscii(PlayerName);
                m_agentSocket.Send(packet);

            }
            catch { Logger.LogIt("Cant Recall Player " + PlayerName, LogType.Hata); }
        }

        public void PlayertoTown_Double(String PlayerName)
        {
            String TheName = PlayerName;

            Packet packet = new Packet(0x7010, true);
            packet.WriteUInt16(0x03);
            packet.WriteAscii(TheName);
            m_agentSocket.Send(packet);

            Thread.Sleep(2000);

            Packet packetQ = new Packet(0x7010, true);
            packetQ.WriteUInt16(0x03);
            packetQ.WriteAscii(TheName);
            m_agentSocket.Send(packetQ);
        }


        private void Ping()
        {
            while (true)
            {
                try
                {
                    Thread.Sleep(5000);
                    Packet packet = new Packet(0x2002);
                    m_agentSocket.Send(packet);
                }
                catch { }
            }
        }
    }
}