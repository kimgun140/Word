using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Word;

namespace EMPCLIENT
{
    /// <summary>
    /// MainPage.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainPage : Page
    {
        NetworkStream stream = Home.clients.GetStream(); //데이터 전송에 사용된 스트림

        //public static TcpClient client = new TcpClient("10.10.21.118", 5004);

        //public static TcpClient client = new TcpClient("10.10.21.109", 5017);
        //public static TcpClient client = new TcpClient("10.10.21.111", 7997);



        public MainPage()
        {
            InitializeComponent();
        }



        private void MoveEMP_Login_Click(object sender, RoutedEventArgs e)
        {
            //NetworkStream stream =  client.GetStream();

            string send_msg;
            byte[] data;
            data = null;
            data = new byte[256];
            send_msg = "직원";// 시그널
            data = Encoding.UTF8.GetBytes(send_msg);
            stream.Write(data, 0, data.Length);//전송할 데이터의 바이트 배열, 전송을 시작할 배열의 인덱스, 전송할 데이터의 길이.

            Thread.Sleep(100);



            NavigationService.Navigate(
              new Uri("/Login_Page.xaml", UriKind.Relative));
        }
    }
}
