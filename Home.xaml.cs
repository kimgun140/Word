using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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

namespace Word
{
    /// <summary>
    /// Home.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Home : Page
    {
        static readonly HttpClient API = new HttpClient();
        byte[] data = new byte[256];
        public static TcpClient clients = new TcpClient("10.10.21.118", 5005); //연결객체
        /*        public static TcpClient clients = new TcpClient("10.10.21.111", 8001); //연결객체*/
        static NetworkStream stream = clients.GetStream();
        public Home()
        {
            InitializeComponent();
        }

        private void btn_CC_Click(object sender, RoutedEventArgs e)
        {
            data = Encoding.UTF8.GetBytes("고객");
            stream.Write(data, 0, data.Length);
            NavigationService.Navigate(new Uri("/Login.xaml", UriKind.Relative));
        }

        private void btn_CS_Click(object sender, RoutedEventArgs e)
        {
            data = Encoding.UTF8.GetBytes("직원");
            stream.Write(data, 0, data.Length);
            NavigationService.Navigate(new Uri("/Login_Page.xaml", UriKind.Relative));
        }
    }
}
