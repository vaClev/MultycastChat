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
    public class TCPMessagesSender
    {
        private readonly string serverIP;
        private readonly int serverTCPPort = 8800;
        private List<User> userList;

        private bool isAvailable = false;


        public TCPMessagesSender(string serverIP, int serverTCPPort)
        {
            this.serverIP = serverIP;
            this.serverTCPPort = serverTCPPort;
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

        public bool isServerAvailable()
        {
            return isAvailable;
        }
    }


}
