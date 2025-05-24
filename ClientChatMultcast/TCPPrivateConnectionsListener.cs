using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;

namespace ClientChatMultcast
{
    class TCPPrivateConnectionsListener
    {
        private readonly int myTCPPort;

        Dictionary<string, Form> privateChats = new Dictionary<string, Form>();

        public TCPPrivateConnectionsListener(int PrivateMessagesListenerTCPPort)
        {
            this.myTCPPort = PrivateMessagesListenerTCPPort;
        }
        public void Start()
        {
            TcpListener tcpListener = new TcpListener(IPAddress.Any, myTCPPort);
            tcpListener.Start();

            new Thread(() => {
                TcpClient client = tcpListener.AcceptTcpClient();
                string clientIP = ((IPEndPoint)client.Client.LocalEndPoint).Address.ToString();

                new Thread(() =>
                {
                    if (privateChats.ContainsKey(clientIP))
                    {
                        //уже есть окно с приватным чатом
                        // Перешли в него новое сообщение
                        MessageBox.Show("Получен входящий запрос на подключение");
                    }
                    else
                    {
                        //Создай новое окно с приватным чатом, добавь в privateChats
                        // Перешли в него новое сообщение
                        MessageBox.Show("Получено личное сообщение");
                    }
                }).Start();



                }).Start();
        }
    }
}
