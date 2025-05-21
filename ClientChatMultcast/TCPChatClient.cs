using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Nodes;

namespace ClientChatMultcast
{
    public delegate void RefreshUsersCallback(List<User> users);
    public class TCPChatClient
    {
        private readonly string serverIP;
        private readonly int serverTCPPort = 8800;
        private List<User> userList;

        private bool isAvailable = false;

        RefreshUsersCallback refreshUsersCallback = null;



        public TCPChatClient(string serverIP, int serverTCPPort, RefreshUsersCallback refreshUsersCallback)
        {
            this.serverIP = serverIP;
            this.serverTCPPort = serverTCPPort;
            this.refreshUsersCallback = refreshUsersCallback;
        }

        public async void SendMessage(string message)
        {
            try
            {
                TcpClient tcpClient = new TcpClient();
                await tcpClient.ConnectAsync(serverIP, serverTCPPort);

                //TODO обернуть в using NetworkStream stream
                NetworkStream stream = tcpClient.GetStream();// получаем поток для взаимодействия с сервером
                var messageBytes = Encoding.Unicode.GetBytes(message);// конвертируем данные в массив байтов

                await stream.WriteAsync(messageBytes, 0, messageBytes.Length);// отправляем данные серверу

                stream.Close();
                tcpClient.Close();
                isAvailable = true;
            }
            catch (SocketException ex)
            {
                MessageBox.Show(ex.Message);
                isAvailable = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public bool SendMessageWithСonfirmation(string message)
        {
            try
            {
                TcpClient tcpClient = new TcpClient();
                 tcpClient.Connect(serverIP, serverTCPPort);

                //TODO обернуть в using NetworkStream stream
                NetworkStream stream = tcpClient.GetStream();// получаем поток для взаимодействия с сервером
                var messageBytes = Encoding.Unicode.GetBytes(message);// конвертируем данные в массив байтов

                stream.Write(messageBytes, 0, messageBytes.Length);// отправляем данные серверу

                stream.Close();
                tcpClient.Close();
                isAvailable = true;
                return true;
            }
            catch (SocketException ex)
            {
                MessageBox.Show(ex.Message);
                isAvailable = false;
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return false;
        }

        public async void RequestUserList(string requestText)
        {
            try
            {
                TcpClient tcpClient = new TcpClient();
                await tcpClient.ConnectAsync(serverIP, serverTCPPort);
                NetworkStream stream = tcpClient.GetStream();// получаем поток для взаимодействия с сервером
                var messageBytes = Encoding.Unicode.GetBytes(requestText);// конвертируем данные в массив байтов
                await stream.WriteAsync(messageBytes, 0, messageBytes.Length);// отправляем данные серверу



                var buffer = new byte[1024]; // буфер для получения данных
                var response = new StringBuilder();
                int bytes;  // количество полученных байтов
                do
                {
                    // получаем данные
                    bytes = await stream.ReadAsync(buffer, 0, buffer.Length);
                    // преобразуем в строку и добавляем ее в StringBuilder
                    response.Append(Encoding.Unicode.GetString(buffer, 0, bytes));
                }
                while (bytes > 0); // пока данные есть в потоке 



                RefreshUsersList(response.ToString());
                stream.Close();
                tcpClient.Close();
            }
            catch (SocketException ex)
            {
                MessageBox.Show(ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void RefreshUsersList(string userslistString)
        {
            var jsonObject = JsonSerializer.Deserialize<List<User>>(userslistString);

            userList = jsonObject;// пока всегда обновляем
            // уведомить подписчиков об обновлении списка
            refreshUsersCallback(userList);
        }


        public bool isServerAvailable()
        {
            return isAvailable;
        }
    }


}
