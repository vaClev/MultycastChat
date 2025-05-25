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
        private const int PrivateMessagesListenerTCPPort = 8805;
        private const int UdpPort = 8801;
        private const string MultycastIP = "224.5.5.5";
        private const string ServerIP = "127.0.0.1";

        private bool isConnectedToServer = false;

        private PublicChatListener pChatListener = null;
        private TCPMessagesSender myChatClient = null;

        TCPPrivateConnectionsListener tCPPrivateConnectionsListener = null;
        Dictionary<string, PrivateChatForm> privateChats = null;

        private LoginForm loginForm = null;

        public Form1()
        {
            InitializeComponent();
            InitializeChat();
            InitializeUsersList();
            InitializePrivateConnections(); 
        }

        private void InitializeChat()
        {
            StartListenPublicChat(); 

            myChatClient = new TCPMessagesSender(ServerIP, ServerTCPPort);
            loginForm = new LoginForm(myChatClient);
            loginForm.ShowDialog();
            isConnectedToServer = loginForm.IsConnected();
        }
        private void StartListenPublicChat()
        {
            pChatListener = new PublicChatListener(UdpPort, MultycastIP, AddNewMessageToPublicChat, RefreshUserList);
            pChatListener.StartListening();
        }
        private void InitializeUsersList()
        {
            listBox1.DisplayMember = "name";
        }

        private void InitializePrivateConnections()
        {
            privateChats = new Dictionary<string, PrivateChatForm>();
            tCPPrivateConnectionsListener = new TCPPrivateConnectionsListener(PrivateMessagesListenerTCPPort, privateChats, CreatePrivateChat); 
            tCPPrivateConnectionsListener.Start();
        }


        ///Нажатие на кнопку отправить сообщение
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


        private void AddNewMessageToPublicChat(string message)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(AddNewMessageToPublicChat), new object[] { message });
                return;
            }
            listBox2.Items.Add(message);
        }

        //Отрисовка списка пользователей
        private void RefreshUserList(List<User> users)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<List<User>>(RefreshUserList), new object[] { users });
                return;
            }
            listBox1.Items.Clear();
            foreach (User user in users)
            {
                listBox1.Items.Add(user);
            }
        }



        private void loginToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Disconnect();
            loginForm.ShowDialog();
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            var user = (User)listBox1.SelectedItem;
            if (user == null)
                return;

            MessageBox.Show($"Пользователь: {user.name} ip: {user.ip}", "информация о пользователе");

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Disconnect();
        }
        private void Disconnect()
        {
            foreach(var form in privateChats)
                form.Value.Close(); //закрыть все приватный чаты

            //Сигнализировать а сервер что я выходу
            myChatClient.SendMessageWithСonfirmation("USER_DISCONNECTED");

            listBox1.Items.Clear();
            //listBox2.Items.Clear(); //Закоментировано для отладки
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if(listBox1.SelectedItem == null)
            {
                return;
            }
            var user = listBox1.SelectedItem as User;
            MessageBox.Show("Открываю приват с " + user.name);
            CreatePrivateChat(user);
        }

        private void CreatePrivateChat(User user)
        {
            if (!privateChats.ContainsKey(user.ip))
                privateChats.Add(user.ip, new PrivateChatForm(loginForm.username, user, PrivateMessagesListenerTCPPort));
            privateChats[user.ip].Show();
        }
        private void CreatePrivateChat(string userIP)
        {
            foreach (var user in listBox1.Items )
            {
                if ((user as User).ip == userIP)
                {
                    CreatePrivateChat(user as User);
                    return;
                }
            }
        }
    }
}



