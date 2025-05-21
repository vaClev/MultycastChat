using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics.Eventing.Reader;
using System.Threading;
using System.Net.Http;


namespace TCPMultycast
{
    class TcpMultycast
    {
        private const int TCPPort = 8800;
        private const int UdpPort = 8801;
        private const string MultycastIP = "224.5.5.5";

        private readonly List<TcpClient> _clients = new List<TcpClient>();
        public string clientIP = null;
        public string message = null;

        public Dictionary<string, string> users = new Dictionary<string, string>(); // ключ IP адресс  -- value имя пользователя 


        public void Start()
        {
            TcpListener tcpListener = new TcpListener(IPAddress.Any, TCPPort);
            tcpListener.Start();
            Console.WriteLine($"TCP сервер запущен на порте {TCPPort}");

            new Thread((ThreadStart) =>
            {
                while (true)
                {
                    TcpClient client = tcpListener.AcceptTcpClient();
                    lock (_clients)
                    {
                        _clients.Add(client);
                    }
                    Console.WriteLine($"Клиент подлючен");
                    clientIP = ((IPEndPoint)client.Client.LocalEndPoint).Address.ToString();

                    new Thread(() =>
                    {
                        if (users.ContainsKey(clientIP))
                        {
                            using (NetworkStream stream = client.GetStream())
                            {
                                byte[] buffer = new byte[4096];
                                int bytesRead;
                                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
                                {
                                    string message = Encoding.Unicode.GetString(buffer, 0, bytesRead);

                                    if(message == "GET_USERS")
                                    {
                                        // Формируем список пользователей клиенту 
                                        string response = GetUsersString(); // конвертировали всех в одну строку
                                        byte[] responseBytes = Encoding.Unicode.GetBytes(response);



                                        //
                                        //
                                        //TODO подумать как отправить правильно ответ
                                        stream.Write(responseBytes, 0, responseBytes.Length);
                                        break;
                                        //
                                    }
                                    else
                                    {
                                        string username = users[clientIP];
                                        string messagepost = username + ":" + message;
                                        Console.WriteLine(messagepost);
                                        BroadcastMessage(messagepost);
                                    }
                                }
                            }
                        }
                        else
                        {
                            //если user не был подключен
                            using (NetworkStream stream = client.GetStream())
                            {
                                Console.WriteLine("Новый клиент подключен");
                                byte[] buffer = new byte[4096];
                                int bytesRead;
                                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
                                {
                                    string message = Encoding.Unicode.GetString(buffer, 0, bytesRead);
                                    users.Add(clientIP, message);
                                    string messagepost = "Пользователь " + message + " вошел в чат.";
                                    Console.WriteLine(messagepost);

                                    BroadcastMessage(messagepost);
                                }
                            }
                        }
                        lock (_clients)
                        {
                            _clients.Remove(client);
                        }
                        Console.WriteLine("Клиент отключен");
                    }).Start();

                }
            }).Start();
        }

        private string GetUsersString()
        {
            var userlistString = new StringBuilder();
            userlistString.Append("[");
            foreach (var user in users)
            {
                userlistString.Append("{\"name\":\"");
                userlistString.Append(user.Value);
                userlistString.Append("\", \"ip\":\"");
                userlistString.Append(user.Key);
                userlistString.Append("\"},");
            }
            userlistString.Remove(userlistString.Length-1, 1); //Стирание запятой после последнего
            userlistString.Append("]");
            Console.WriteLine(userlistString.ToString());
            return userlistString.ToString();
        }

        private void BroadcastMessage(string messagepost)
        {
            UdpClient udpClient = new UdpClient();
            udpClient.JoinMulticastGroup(IPAddress.Parse(MultycastIP));

            byte[] data = Encoding.Unicode.GetBytes(messagepost + "|||" + GetUsersString());
            udpClient.Send(data, data.Length, MultycastIP, UdpPort);
            udpClient.Close();
            Console.WriteLine($"Широковещательное сообщение отправлено: {messagepost}");
        }


    }


    class Program
    {
        static void Main(string[] args)
        {
            TcpMultycast server = new TcpMultycast();
            server.Start();

            Console.WriteLine("Нажмите Enter для завершения работы ");
            Console.ReadKey();
        }
    }
}
