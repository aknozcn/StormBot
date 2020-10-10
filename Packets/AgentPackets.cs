using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SilkroadSecurityApi;

namespace StormBot
{
    class AgentPackets
    {
       /* Server m_proxy = new Server();

        public void SendGlobal(string message)
        {
            Packet packet = new Packet(0x704C, true);
            packet.WriteUInt(0x0D);
            packet.WriteUInt(0xEC);
            packet.WriteUInt(0x29);
            packet.WriteAscii(message);
            m_proxy.Send(packet);
            Logger.LogIt(message, LogType.Global);
        }

        public void SendNotice(string Message)
        {
            Packet packet = new Packet(0x7025);
            packet.WriteUInt(7);
            packet.WriteUInt(0);
            packet.WriteAscii(Message);
            m_proxy.Send(packet);
            Logger.LogIt(Message, LogType.Notice);
        }

        public void SendPM(string Target, string Message)
        {
            try
            {
                Packet packet = new Packet(0x7025);
                packet.WriteUInt(0x02);
                packet.WriteUInt(0x00);
                packet.WriteAscii(Target);
                packet.WriteAscii(Message);
                m_proxy.Send(packet);
                Logger.LogIt(string.Format("{0} ==> {1}", Target, Message), LogType.PM);
            }
            catch { Logger.LogIt("Pm Hatası", LogType.Hata); }
        }

        public void AllChat(string Message)
        {
            Packet packet = new Packet(0x7025);
            packet.WriteUInt(0x03);
            packet.WriteUInt(0x00);
            packet.WriteAscii(Message);
            m_proxy.Send(packet);
        }*/
    }
}
