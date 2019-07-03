using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KrestikiNolikiClient
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const string LoginConst = "Login";          // send
        const string NomerConst = "Nomer";          // send, receive
        const string RazdConst = ";";               // send, receive
        const string RivalConst = "Rival";          // send, receive

        static int playerNumber;
        static string rivalName;    // имя соперника
        static string playerImage;  // символ крестика или нолика
        static public Socket socket;
        IPEndPoint ipPoint;
        public MainWindow()
        {
            InitializeComponent();
            btnConnect.IsEnabled = true;
            btDisconnect.IsEnabled = false;
            tbUserName.IsEnabled = true;
            btRefresh.IsEnabled = false;
            Main.Title = "";
            playerNumber = 0;
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            // адрес и порт сервера, к которому будем подключаться
            int port;
            string address;
            if (tbUserName.Text == "")
            {
                MessageBox.Show("Введите имя игрока!");
                return;
            }
            address = tbIpAdress.Text;
            port = Convert.ToInt32(tbPort.Text);
            try
            {
                ipPoint = new IPEndPoint(IPAddress.Parse(address), port);
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                // подключаемся к удаленному хосту
                socket.Connect(ipPoint);
                LoginSend();
                btnConnect.IsEnabled = false;
                btDisconnect.IsEnabled = true;
                tbUserName.IsEnabled = false;
                //UsersRefresh();
            }
            catch (Exception exc)
            {
                ListDataAdd(exc.Message + "\n");
                return;
            }
        }

        private async void SendData(int sender)
        {
            try
            {
                //                    StringBuilder sb = await ThreadSendReceiveAsync(NomerConst + cbPoluch.SelectedIndex.ToString() + RazdConst + tbSend.Text);

                //ListDataAdd(sb.ToString() + "\n");

            }
            catch (Exception exc)
            {
                ListDataAdd(exc.Message + "\n");
                return;
            }
        }

        static StringBuilder ThreadSendReceive(string text)
        {
            byte[] data = Encoding.UTF8.GetBytes(text);
            socket.Send(data);
            // получаем ответ
            data = new byte[4 * 1024]; // буфер для ответа
            StringBuilder builder = new StringBuilder();
            int bytes = 0; // количество полученных байт
            do
            {
                bytes = socket.Receive(data, data.Length, 0);
                builder.Append(Encoding.UTF8.GetString(data, 0, bytes));
            }
            while (socket.Available > 0);
            return builder;
        }

        // определение асинхронного метода
        static async Task<StringBuilder> ThreadSendReceiveAsync(string text)
        {
            return await Task.Run(() => ThreadSendReceive(text));
        }

        private void btClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btDisconnect_Click(object sender, RoutedEventArgs e)
        {
            // закрываем сокет
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
            btnConnect.IsEnabled = true;
            btDisconnect.IsEnabled = false;
            tbUserName.IsEnabled = true;
            btRefresh.IsEnabled = false;
        }

        public void ListDataAdd(string text)
        {
            var textBlockWork = new TextBlock();
            textBlockWork.Width = 450;
            textBlockWork.Height = 15;
            textBlockWork.Text = text;
            ListData.Items.Add(textBlockWork);

        }

        public async void LoginSend()
        {
            string stroka;
            int ind;
            try
            {
                StringBuilder sb = await ThreadSendReceiveAsync(LoginConst + tbUserName.Text);

                stroka = sb.ToString();

                stroka = stroka.Substring(NomerConst.Length);
                ind = stroka.IndexOf(RazdConst);
                playerNumber = Int32.Parse(stroka.Remove(ind));

                stroka = stroka.Substring(ind + 1);
                ind = stroka.IndexOf(RazdConst);
                rivalName = stroka.Remove(ind);

                stroka = stroka.Substring(ind + 1);

                ListDataAdd(stroka + "\n");

                playerImage = "";
                if (playerNumber == 1)
                {
                    Main.Title = "Ваш порядковый номер = " + playerNumber.ToString() + ". У вас крестики.";
                    playerImage = "X";
                    btRefresh.IsEnabled = true;
                }
                else if (playerNumber == 2)
                {
                    Main.Title = "Ваш порядковый номер = " + playerNumber.ToString() + ". У вас нолики.";
                    playerImage = "O";
                    btRefresh.IsEnabled = false;
                }
                if (rivalName != "")
                {
                    // начинаем игру
                    tbRival.Text = rivalName;
                    bt1.IsEnabled = true;
                    bt2.IsEnabled = true;
                    bt3.IsEnabled = true;
                    bt4.IsEnabled = true;
                    bt5.IsEnabled = true;
                    bt6.IsEnabled = true;
                    bt7.IsEnabled = true;
                    bt8.IsEnabled = true;
                    bt9.IsEnabled = true;
                }

            }
            catch (Exception exc)
            {
                ListDataAdd(exc.Message + "\n");
                return;
            }
        }

        private void bt1_Click(object sender, RoutedEventArgs e)
        {
            //SendData(1);
            //this.Content = playerImage;
            //this.IsEnabled = false;
        }

        private async void btRefresh_Click(object sender, RoutedEventArgs e)
        {
            string stroka;
            int ind;
            try
            {
                StringBuilder sb = await ThreadSendReceiveAsync(RivalConst);

                stroka = sb.ToString();

                //stroka = stroka.Substring(NomerConst.Length);
                //ind = stroka.IndexOf(RazdConst);
                //playerNumber = Int32.Parse(stroka.Remove(ind));

                //stroka = stroka.Substring(ind + 1);
                //ind = stroka.IndexOf(RazdConst);
                //rivalName = stroka.Remove(ind);

                //stroka = stroka.Substring(ind + 1);

                ListDataAdd(stroka + "\n");
                tbRival.Text = stroka;

                //playerImage = "";
                //if (playerNumber == 1)
                //{
                //    Main.Title = "Ваш порядковый номер = " + playerNumber.ToString() + ". У вас крестики.";
                //    playerImage = "X";
                //}
                //else if (playerNumber == 2)
                //{
                //    Main.Title = "Ваш порядковый номер = " + playerNumber.ToString() + ". У вас нолики.";
                //    playerImage = "O";
                //}
                //if (rivalName != "")
                //{
                //    // начинаем игру
                //    tbRival.Text = rivalName;
                //    bt1.IsEnabled = true;
                //    bt2.IsEnabled = true;
                //    bt3.IsEnabled = true;
                //    bt4.IsEnabled = true;
                //    bt5.IsEnabled = true;
                //    bt6.IsEnabled = true;
                //    bt7.IsEnabled = true;
                //    bt8.IsEnabled = true;
                //    bt9.IsEnabled = true;
                //}

            }
            catch (Exception exc)
            {
                ListDataAdd(exc.Message + "\n");
                return;
            }

        }
    }
}
