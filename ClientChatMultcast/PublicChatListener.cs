using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClientChatMultcast
{
    delegate void AddMessageFunction(string message);
    class PublicChatListener
    {
        private readonly int UdpPort;
        private readonly string MultycastIP;
        private readonly AddMessageFunction AddMessage;

        Thread listenerThread = null;

        public PublicChatListener(int UdpPort, string MultycastIP, AddMessageFunction fn)
        {
            this.UdpPort = UdpPort;
            this.MultycastIP = MultycastIP;
            this.AddMessage = fn;
        }

        public void StartListening()
        {
            try
            {
                listenerThread = new Thread(new ThreadStart(Listen));
                listenerThread.IsBackground = true;
                listenerThread.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Проблема {ex.Message}");
            }
        }

        private async void Listen()
        {
            using (UdpClient udpClient = new UdpClient(UdpPort))
            {
                var brodcastAddress = IPAddress.Parse(MultycastIP);// хост для отправки данных                                                        
                udpClient.JoinMulticastGroup(brodcastAddress); // присоединяемся к группе
                //udpClient.MulticastLoopback = false; // отключаем получение своих же сообщений
                try
                {
                    while (true)
                    {
                        var result = await udpClient.ReceiveAsync();
                        string message = Encoding.Unicode.GetString(result.Buffer);
                        AddMessage(message);
                    }
                }
                catch (SocketException ex)
                {
                    // Обработка исключений, связанных с сокетом
                    MessageBox.Show($"SocketException: {ex.Message}");
                }
                catch (Exception ex)
                {
                    // Обработка других исключений 
                    MessageBox.Show($"Exception: {ex.Message}");// [1](https://www.iditect.com/faq/csharp/receive-messages-continuously-using-udpclient-in-c.html)
                }
                // отсоединяемся от группы
                udpClient.DropMulticastGroup(brodcastAddress);
                MessageBox.Show("Udp-клиент завершил свою работу");
            }
        }
    }
}
