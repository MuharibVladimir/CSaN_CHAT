using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Windows;

namespace Chat
{
    class ChatManager
    {
        public delegate void UpdateWindowChat(string text);

        private const int udpPort = 11200;
        private const int tcpPort = 11800;
        private string myLogin;
        private List<Client> clients = new List<Client>();
        public IPAddress chooseIP;
        public UpdateWindowChat updateChat;
        private StringBuilder chatHistory;
        private DateTime currentTime;
        private readonly SynchronizationContext synchronizationContext;

        public ChatManager(UpdateWindowChat uChat)
        {
            updateChat = uChat;
            chatHistory = new StringBuilder();
            currentTime = new DateTime();
            synchronizationContext = SynchronizationContext.Current;
        }

        public void ChatConnection(string login)
        {
            IPEndPoint srcIP = new IPEndPoint(chooseIP, udpPort);
            IPEndPoint destIP = new IPEndPoint(MakeBroadcastAdress(chooseIP), udpPort);
            UdpClient udpClient = new UdpClient(srcIP);
            udpClient.EnableBroadcast = true;

            myLogin = login;
            byte[] MessageBytes = Encoding.UTF8.GetBytes(login);

            try
            {
                udpClient.Send(MessageBytes, MessageBytes.Length, destIP);
                udpClient.Close();

                currentTime = DateTime.Now;
                string connectMessage = $"{currentTime} : IP [{chooseIP}] {login} подключился к чату\n";
                chatHistory.Append(connectMessage);
                updateChat($"{currentTime} : IP [{chooseIP}] Вы ({login}) подключились к чату\n");

                Task recieveUdpBroadcast = new Task(ReceiveBroadcast);
                recieveUdpBroadcast.Start();

                Task recieveTCP = new Task(ReceiveTCP);
                recieveTCP.Start();
            }
            catch
            {
                MessageBox.Show("Sending Error!", "BAD", MessageBoxButton.OKCancel);
            }
        }

        private void ReceiveBroadcast()
        {
            IPEndPoint srcIP = new IPEndPoint(chooseIP, udpPort);
            IPEndPoint destIP = new IPEndPoint(IPAddress.Any, udpPort);
            UdpClient udpReceiver = new UdpClient(srcIP);

            while (true)
            {
                byte[] receivedData = udpReceiver.Receive(ref destIP);
                string clientLogin = Encoding.UTF8.GetString(receivedData);

                Client newClient = new Client(clientLogin, destIP.Address, tcpPort);
                newClient.EstablishConnection();
                clients.Add(newClient);
                newClient.SendMessage(new Message(Message.CONNECTION, myLogin));

                currentTime = DateTime.Now;
                string infoMess = $"{currentTime} : IP [{newClient.IP}] {newClient.login} подключился к чату\n";

                synchronizationContext.Post(delegate { updateChat(infoMess); }, null);


                Task.Factory.StartNew(() => Listen(newClient));
            }

        }

        private void ReceiveTCP()
        {
            TcpListener tcpListener = new TcpListener(chooseIP, tcpPort);
            tcpListener.Start();

            while (true)
            {
                TcpClient tcpNewClient = tcpListener.AcceptTcpClient();
                Client newClient = new Client(tcpNewClient, tcpPort);

                Task.Factory.StartNew(() => Listen(newClient));
            }

        }

        private void Listen(Client client)
        {
            while (true)
            {

                Message tcpMessage = client.ReceiveMessage();
                string infoMessage;

                switch (tcpMessage.code)
                {
                    case Message.CONNECTION:
                        client.login = tcpMessage.data;
                        clients.Add(client);
                        GetHistoryMessageToConnect(client);

                        break;

                    case Message.DISCONNECTION:
                        currentTime = DateTime.Now;
                        infoMessage = $"{currentTime} : IP [{client.IP}] {client.login} покинул чат\n";
                        synchronizationContext.Post(delegate { updateChat(infoMessage); chatHistory.Append(infoMessage); }, null);
                        clients.Remove(client);
                        return;

                    case Message.MESSAGE:
                        currentTime = DateTime.Now;
                        infoMessage = $"{currentTime} : IP [{client.IP}] {client.login} : {tcpMessage.data}\n";
                        synchronizationContext.Post(delegate { updateChat(infoMessage); chatHistory.Append(infoMessage); }, null);
                        break;

                    case Message.GET_HISTORY:
                        SendHistory(client);
                        break;

                    case Message.SHOW_HISTORY:
                        synchronizationContext.Post(delegate { updateChat(tcpMessage.data); chatHistory.Append(tcpMessage.data); }, null);
                        break;

                    default:
                        MessageBox.Show("Сообщение введено некорректно", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                }

            }
        }

        public void SendHistory(Client client)
        {
            Message historyMessage = new Message(Message.SHOW_HISTORY, chatHistory.ToString());
            client.SendMessage(historyMessage);
        }

        public void GetHistoryMessageToConnect(Client client)
        {
            Message historyMessage = new Message(Message.GET_HISTORY, "");
            client.SendMessage(historyMessage);
        }

        public void SendDisconnectMessage()
        {
            string disconnectStr = $"{myLogin} покинул чат";
            Message disconnectMes = new Message(Message.DISCONNECTION, disconnectStr);
            SendMessageToAllClients(disconnectMes);
        }

        public void SendSimpleMessage(string message)
        {
            if (message != "")
            {
                Message normalMessage = new Message(Message.MESSAGE, message);
                SendMessageToAllClients(normalMessage);
            }
        }

        public void SendMessageToAllClients(Message tcpMessage)
        {
            foreach (var user in clients)
            {
                try
                {
                    user.SendMessage(tcpMessage);
                }
                catch
                {
                    MessageBox.Show($"Не удалось отправить сообщение пользователю {user.login}.",
                        "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            if (tcpMessage.code == Message.MESSAGE)
            {
                currentTime = DateTime.Now;
                string infoMessage = $"{currentTime} : IP [{chooseIP}] Вы : {tcpMessage.data}\n";

                updateChat(infoMessage);

                infoMessage = $"{currentTime} : IP [{chooseIP}] {myLogin} : {tcpMessage.data}\n";
                chatHistory.Append(infoMessage);
            }

        }

        private IPAddress MakeBroadcastAdress(IPAddress ip)
        {
            string broadcastAdress = ip.ToString();
            broadcastAdress = broadcastAdress.Substring(0, broadcastAdress.LastIndexOf('.') + 1) + "255";

            return IPAddress.Parse(broadcastAdress);
        }
    }
}
