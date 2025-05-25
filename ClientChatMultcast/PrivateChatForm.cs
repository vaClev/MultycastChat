using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClientChatMultcast
{
    public partial class PrivateChatForm: Form
    {
        string myName = null;
        readonly User partner = null;
        TCPMessagesSender tcpSender = null;
        public PrivateChatForm(string myName,  User user, int privateListenerTCPPort)
        {
            InitializeComponent();
            this.myName = myName;
            this.partner = user;
            usernameLabel.Text = user.name;

            tcpSender = new TCPMessagesSender(user.ip, privateListenerTCPPort);
        }


        //Клик отправить сообщение 
        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox1.Text))
                return;
            string message = myName + ":" + textBox1.Text;

            //отправить сообщение получателю
            tcpSender.SendMessage(message);

            //Добавить свое сообщение в локальное отображение чата
            AddNewMessageToPrivateChat(message);
            textBox1.Clear();
        }

        public void AddNewMessageToPrivateChat(string message)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(AddNewMessageToPrivateChat), new object[] { message });
                return;
            }
            listBox1.Items.Add(message);
        }
    }
}
