using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Windows.Shapes;

namespace Word
{
    /// <summary>
    /// Chat.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Chat : Window
    {
        NetworkStream stream = Home.clients.GetStream(); 
        byte[] data = new byte[256];
        public Chat()
        {
            var mainWindow = Application.Current.MainWindow;
            if (mainWindow != null)
            {
                mainWindow.Width = 500; // 원하는 너비로 설정
                mainWindow.Height = 800; // 원하는 높이로 설정
            }
            string msg = "상담";
            data = null;
            data = Encoding.UTF8.GetBytes(msg);
            stream.Write(data, 0, data.Length);

            InitializeComponent();
            txtbox_chat.Text = "상담원과의 연결을 대기합니다.";
            Task.Run(() => Wait_cc());
        }
        public void Wait_cc() //Task 함수 (서버에선 고객 대기방 함수)
        {
            byte[] data1 = new byte[256];
            int bytes = stream.Read(data1, 0, data1.Length);//받는 데이터의 바이트배열, 인덱스, 길이
            string responses = Encoding.UTF8.GetString(data1, 0, bytes);

            if (responses == "채팅가능")
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    txtbox_send.IsReadOnly = false;
                    txtbox_chat.Text = " ";
                }));
                Task.Run(() => Read_Chat());
            }
        }
        public void Read_Chat()
        {
            while (true)
            {
                byte[] recv_data = new byte[300];
                int bytes = stream.Read(recv_data, 0, recv_data.Length);
                string responses = Encoding.UTF8.GetString(recv_data, 0, bytes);
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    txtbox_chat.Text += responses + "\n";
                }));
            }
        }

        private void txtbox_send_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string send_message = "[고객] : " + txtbox_send.Text;
                if (!string.IsNullOrEmpty(send_message))
                {
                    data = null;
                    data = Encoding.UTF8.GetBytes(send_message);
                    stream.Write(data, 0, data.Length);
                    /*    txtbox_chat.Text += send_message;*/
                    txtbox_send.Clear();
                    txtbox_chat.ScrollToEnd();
                }
            }
        }
    }
}
