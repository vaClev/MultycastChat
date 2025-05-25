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
    delegate void OpenNewPrivateChatForm(string userIP);
    class TCPPrivateConnectionsListener
    {
        private readonly int myTCPPort;

        Dictionary<string, PrivateChatForm> privateChats;
        OpenNewPrivateChatForm openNewPrivateChatForm;
        public TCPPrivateConnectionsListener(
            int PrivateMessagesListenerTCPPort,
            Dictionary<string, PrivateChatForm> privateChats,
            OpenNewPrivateChatForm openNewPrivateChatForm
            )
        {
            this.myTCPPort = PrivateMessagesListenerTCPPort;
            this.privateChats = privateChats;
            this.openNewPrivateChatForm = openNewPrivateChatForm;
        }
        public void Start()
        {
            TcpListener tcpListener = new TcpListener(IPAddress.Any, myTCPPort);
            tcpListener.Start();

            new Thread(() => {
                while (true)
                {
                    TcpClient client = tcpListener.AcceptTcpClient();
                    string clientIP = ((IPEndPoint)client.Client.LocalEndPoint).Address.ToString();

                    new Thread(() =>
                    {
                        if (!privateChats.ContainsKey(clientIP))
                        {
                            //Создай новое окно с приватным чатом, добавь в privateChats
                            MessageBox.Show("Получен входящий запрос на подключение");
                            openNewPrivateChatForm(clientIP);
                        }
                        using (NetworkStream stream = client.GetStream())
                        {
                            byte[] buffer = new byte[4096];
                            int bytesRead;
                            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
                            {
                                string message = Encoding.Unicode.GetString(buffer, 0, bytesRead);
                                privateChats[clientIP].AddNewMessageToPrivateChat(message);
                            }
                        }
                    }).Start();
                }
            }).Start();
        }
    }
}
