using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace ClientChatMultcast
{
    public partial class Form1 : Form
    {
        private const int ServerTCPPort = 8800;
        private const int UdpPort = 8801;
        private const string MultycastIP = "224.5.5.5";
        private const string ServerIP = "127.0.0.1";

        private bool isConnectedToServer = false;

        private PublicChatListener pChatListener = null;
        private TCPChatClient myChatClient = null;
        private LoginForm loginForm = null;

        public Form1()
        {
            InitializeComponent();
            InitializeChat();
            InitializeUsersList(); //TODO вызывать после подключения к серверу
        }
        private void InitializeChat()
        {
            StartListenPublicChat();
            myChatClient = new TCPChatClient(ServerIP, ServerTCPPort, RefreshUserList);

            loginForm = new LoginForm(myChatClient);
            loginForm.ShowDialog();
            isConnectedToServer = loginForm.IsConnected();
        }

        private void StartListenPublicChat()
        {
            pChatListener = new PublicChatListener(UdpPort, MultycastIP, AddNewMessageToPublicChat);
            pChatListener.StartListening();
        }

        private void AddNewMessageToPublicChat(string message)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(AddNewMessageToPublicChat), new object[] { message });
                return;
            }
            listBox2.Items.Add(message);
        }

        private void InitializeUsersList()
        {
            listBox1.DisplayMember = "name";
            new Thread(() =>
            {
                while (!this.IsDisposed && isConnectedToServer) //пока главное окно существет
                {
                    myChatClient?.RequestUserList("GET_USERS");
                    Thread.Sleep(5000);
                }
            }).Start();
        }


        private void button1_Click(object sender, EventArgs e)
        {
            string message = currentMessageTextBox.Text;
            if (string.IsNullOrEmpty(message))
            {
                return;
            }
            myChatClient.SendMessage(message);
            currentMessageTextBox.Clear();
        }


        private void RefreshUserList(List<User> users)
        {
            listBox1.Items.Clear();
            foreach (User user in users)
            {
                listBox1.Items.Add(user);
            }
        }

        private void loginToolStripMenuItem_Click(object sender, EventArgs e)
        {
            loginForm.Show();
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            var user = (User)listBox1.SelectedItem;
            if (user == null)
                return;

            MessageBox.Show($"Пользователь: {user.name} ip: {user.ip}", "информация о пользователе");

        }
    }
}



