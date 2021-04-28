using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;

namespace Chat
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<IPAddress> ipList;
        private IPAddress selectedIP;
        private ChatManager connection;

        public MainWindow()
        {
            InitializeComponent();

            txtboxMessage.IsEnabled = false;
            btnSend.IsEnabled = false;
            btnConnect.IsDefault = true;

            ipList = IPList.GetIPList();
            foreach (IPAddress ip in ipList)
            {
                cmboxUserIP.Items.Add(ip.ToString());
            }
            cmboxUserIP.SelectedIndex = 0;
            selectedIP = ipList[0];


            connection = new ChatManager(UpdateChat);
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            connection.chooseIP = selectedIP;
            connection.ChatConnection(txtboxLogin.Text);
            btnConnect.IsEnabled = false;
            cmboxUserIP.IsEnabled = false;
            btnConnect.IsDefault = false;
            txtboxLogin.IsReadOnly = true;

            btnSend.IsDefault = true;
            txtboxMessage.IsEnabled = true;
            btnSend.IsEnabled = true;
        }

        private void cmboxUserIP_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedIP = IPAddress.Parse(cmboxUserIP.SelectedItem.ToString());
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            string currMess = txtboxMessage.Text;
            connection.SendSimpleMessage(currMess);
            txtboxMessage.Text = "";
            txtboxChatWindow.ScrollToEnd();
        }

        private void UpdateChat(string text)
        {
            txtboxChatWindow.AppendText(text);
        }

        private void formMain_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            connection.SendDisconnectMessage();
            System.Environment.Exit(0);
        }
    }
}

