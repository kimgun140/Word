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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Word
{
    /// <summary>
    /// Login.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Login : Page
    {
        NetworkStream stream = Home.clients.GetStream();
        List<string> str_list_jobject = new List<string>();
        List<string> login_list = new List<string>();
        byte[] data = new byte[256];
        public Login()
        {
            InitializeComponent();
        }

        private void txt_ID_GotFocus(object sender, RoutedEventArgs e)
        {
            txt_ID.Clear();
        }

        private void pw_PW_GotFocus(object sender, RoutedEventArgs e)
        {
            pw_PW.Clear();
        }
        private void btn_login_Click(object sender, RoutedEventArgs e)
        {
            
            login_list.Clear();
            login_list.Add("로그인요청");
            login_list.Add(txt_ID.Text);
            login_list.Add(pw_PW.Password);

            foreach (string login_index in login_list)
            {
                data = Encoding.UTF8.GetBytes(login_index);
                stream.Write(data, 0, data.Length);//전송할 데 이터의 바이트 배열, 전송을 시작할 배열의 인덱스, 전송할 데이터의 길이.
                Thread.Sleep(100);
            }

            byte[] recv_data = new byte[300];
            int bytes = stream.Read(recv_data, 0, recv_data.Length);
            string responses = Encoding.UTF8.GetString(recv_data, 0, bytes);
            Console.WriteLine("Received: " + responses);
            if (responses == "로그인 되었습니다")
            {
                stream.Flush();
                NavigationService.Navigate(new Uri("/Main.xaml", UriKind.Relative));
            }
            else if (responses == "일치하는 정보가 없습니다")
            {
               MessageBox.Show(responses);
            }
        }

        private void btn_join_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Join.xaml", UriKind.Relative));
        }
    }
}
