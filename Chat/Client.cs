using System;
using System.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Windows;

namespace Chat
{
    class Client
    {
        public string login;
        public IPAddress IP;
        private IPEndPoint endIP;
        private TcpClient tcpClient;
        private int tcpPort;
        public NetworkStream Stream;

        public Client(TcpClient clTcpClient, int clPort)
        {
            tcpClient = clTcpClient;
            tcpPort = clPort;
            IP = ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address;
            Stream = tcpClient.GetStream();
        }

        public Client(string clLogin, IPAddress clIP, int clPort)
        {
            login = clLogin;
            IP = clIP;
            tcpPort = clPort;
            endIP = new IPEndPoint(IP, tcpPort);
        }

        public void EstablishConnection()
        {
            tcpClient = new TcpClient();
            tcpClient.Connect(endIP);
            Stream = tcpClient.GetStream();
        }

        public void SendMessage(Message clMessage)
        {
            byte[] arMessage = Encoding.UTF8.GetBytes((char)clMessage.code + clMessage.data);
            Stream.Write(arMessage, 0, arMessage.Length);
        }

        public Message ReceiveMessage()
        {
            StringBuilder message = new StringBuilder();
            byte[] buff = new byte[1024];

            do
            {
                try
                {
                    int size = Stream.Read(buff, 0, buff.Length);
                    message.Append(Encoding.UTF8.GetString(buff, 0, size));
                }
                catch
                {
                    return new Message(Message.DISCONNECTION, "");
                }

            }
            while (Stream.DataAvailable);

            Message recvMessage = new Message(message[0], message.ToString().Substring(1));

            return recvMessage;
        }

    }
}
