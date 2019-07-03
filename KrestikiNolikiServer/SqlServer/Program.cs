using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KrestikiNolikiServer
{
    class Program
    {

        const string LoginConst = "Login";          // send
        const string NomerConst = "Nomer";          // send, receive
        const string RazdConst = ";";               // send, receive
        const string RivalConst = "Rival";          // send, receive

        public static Thread SrvThread;
        public static Socket socServer;
        static List<UserServer> usersServer = new List<UserServer>();
        static int gloNomer = 0;

        static void Main(string[] args)
        {

            socServer =
                new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            string srvAddress = "0.0.0.0";
            int srvPort = 12345;
            socServer.Bind(new
            IPEndPoint(IPAddress.Parse(srvAddress), srvPort));
            socServer.Listen(100);
            // далее должна быть команда Accept()
            SrvThread = new Thread(ServerThreadRoutine);
            //SrvThread.IsBackground = true;
            SrvThread.Start(socServer);
            Console.WriteLine("Сервер игры 'Крестики-нолики' включен.");

            while (true)
            {
                Socket client = socServer.Accept();

                Console.WriteLine("Клиент подключен: ");
                Console.WriteLine(
                   client.RemoteEndPoint.ToString());
                ThreadPool.QueueUserWorkItem(
                  ClientThreadProc, client);
            }
        }

        static void ServerThreadRoutine(object obj)
        {
            TcpListener srvSock = obj as TcpListener;
            // синхронный вариант сервера
            try
            {
                while (true)
                {
                    // не ассинхронной блокирующий вызов Accept()
                    // работа с клиентом в отдельном потоке
                    TcpClient client = srvSock.AcceptTcpClient();
                    //   запуск клиентского потока -
                    ThreadPool.QueueUserWorkItem(
                        ClientThreadProc, client);
                }
            }
            catch
            {
                return;
            }
        }


        // поток обслуживания удаленного клиента
        static void ClientThreadProc(object obj)
        {
            // протокол работы сервера - эхо-сервер
            Socket client = (Socket)obj;// as Socket;
            string index, message, stroka;
            try
            {
                while (true)
                {
                    // получаем ответ
                    byte[] data = new byte[4 * 1024]; // буфер для ответа
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0; // количество полученных байт
                    int playerNumber;
                    string playerName;

                    do
                    {
                        bytes = client.Receive(data, data.Length, 0);
                        builder.Append(Encoding.UTF8.GetString(data, 0, bytes));
                    }
                    while (client.Available > 0);

                    // index - что получено
                    index = builder.ToString();
                    //                    var findStreet = streets.Where(t => t.Index == index); ;
                    message = "";
                    if (index.StartsWith(LoginConst) == true)
                    {
                        if (gloNomer == 2)
                        {
                            message = "Игра уже идет для других двух игроков. Попробуйте присоединиться попозже.";
                        }
                        else
                        {
                            if (gloNomer == 0)
                            {
                                usersServer.Clear();
                            }

                            gloNomer++;

                            stroka = index.Substring(LoginConst.Length);

                            usersServer.Add(new UserServer()
                            {
                                Nomer = gloNomer,
                                Name = stroka,
                                UserSocket = client
                            });

                            playerNumber = 0;
                            playerName = "";
                            if (gloNomer == 1)
                            {
                                playerNumber = 1;
                                playerName = "";
                                message = "Ждем второго игрока. У вас крестики. Нажимайте кнопку 'Обновить'";
                            }
                            else
                            {
                                playerNumber = 2;
                                playerName = usersServer[1 - (gloNomer - 1)].Name;
                                message = $"Ваш соперник = {playerName}. У вас нолики. Начинаем игру.";
                            }

                            message = NomerConst + playerNumber.ToString() + RazdConst + playerName + RazdConst + message;
                        }
                        data = Encoding.UTF8.GetBytes(message);
                        client.Send(data);
                        //client.Connected == t
                    }

                    else if (index.StartsWith(RivalConst) == true)
                    {
                        if (gloNomer == 2)
                        {
                            message = usersServer[1].Name;
                        }
                        else
                        {
                            message = "";
                        }
                        data = Encoding.UTF8.GetBytes(message);
                        client.Send(data);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR:" + ex.Message);
            }
            client.Shutdown(SocketShutdown.Both);
            client.Close();

        }

    }
}

