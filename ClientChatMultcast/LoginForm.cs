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
    public partial class LoginForm: Form
    {
        private TCPChatClient myChatClient;
        private bool m_isConnected;
        public LoginForm(TCPChatClient tCPChatClient)
        {
            InitializeComponent();
            this.myChatClient = tCPChatClient;
            m_isConnected = false;
        }

        internal bool IsConnected()
        {
            return m_isConnected;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string message = textBox1.Text;
            if (string.IsNullOrEmpty(message))
            {
                return;
            }
            textBox1.Clear();
            if (myChatClient.SendMessageWithСonfirmation(message))
            {
                m_isConnected = true;
                Close();
            }
                
        }
    }
}
