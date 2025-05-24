using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net.NetworkInformation;
using System.Text.Json.Nodes;


namespace ClientChatMultcast
{
    delegate void AddMessageFunction(string message);
    class PublicChatListener
    {
        private readonly int UdpPort;
        private readonly string MultycastIP;
        private readonly AddMessageFunction AddMessage;

        private List<User> userList;
        RefreshUsersCallback RefreshUsersList = null;

        Thread listenerThread = null;

        public PublicChatListener(int UdpPort, string MultycastIP, AddMessageFunction addMessage, RefreshUsersCallback refreshUsersCallback)
        {
            this.UdpPort = UdpPort;
            this.MultycastIP = MultycastIP;
            this.AddMessage = addMessage;
            this.RefreshUsersList = refreshUsersCallback;
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
            using (UdpClient udpClient = new UdpClient())
            {
                udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, UdpPort));
                var brodcastAddress = IPAddress.Parse(MultycastIP);// хост для отправки данных                                                        
                udpClient.JoinMulticastGroup(brodcastAddress); // присоединяемся к группе
                //udpClient.MulticastLoopback = false; // отключаем получение своих же сообщений
                try
                {
                    while (true)
                    {
                        var result = await udpClient.ReceiveAsync();
                        new Thread(() =>
                        {
                            string fullMessage = Encoding.Unicode.GetString(result.Buffer);
                            JsonNode forecastNode = JsonNode.Parse(fullMessage);//Получим DOM  

                            //Выделяем и отправляем сообщение
                            string mesage = forecastNode["message"].ToString();
                            AddMessage(mesage);

                            //парсим и обновляем список польщователей
                            var parsedUserList = JsonSerializer.Deserialize<List<User>>(forecastNode["users"].ToString());
                            userList = parsedUserList;// пока всегда обновляем --- потом подумаем над сверкой перед отрисовкой
                            RefreshUsersList(userList);
                        }).Start();
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
