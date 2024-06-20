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
using System.Xml.Linq;

namespace Word
{
    /// <summary>
    /// Join.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Join : Page
    {
        byte[]? data;
        NetworkStream stream = Home.clients.GetStream();
        public Join()
        {
            InitializeComponent();
        }
        public void Signup(string id, string pw) // 고객 회원가입
        {
            try
            {
                string send_msg;

                send_msg = "회원가입요청";
                data = null;
                data = Encoding.UTF8.GetBytes(send_msg);
                stream.Write(data, 0, data.Length);//전송할 데이터의 바이트 배열, 전송을 시작할 배열의 인덱스, 전송할 데이터의 길이.
                Thread.Sleep(100);
                send_msg = "";

                //아이디 보내기
                data = null;
                data = new byte[256];
                send_msg = id; // id
                data = Encoding.UTF8.GetBytes(send_msg);
                stream.Write(data, 0, data.Length);//전송할 데이터의 바이트 배열, 전송을 시작할 배열의 인덱스, 전송할 데이터의 길이.
                send_msg = "";
                Thread.Sleep(100);
                //비밀번호 보내기
                data = null;
                data = new byte[256];
                send_msg = pw; // pw
                data = Encoding.UTF8.GetBytes(send_msg);
                stream.Write(data, 0, data.Length);//전송할 데이터의 바이트 배열, 전송을 시작할 배열의 인덱스, 전송할 데이터의 길이.
                send_msg = "";

                data = null;
                data = new byte[256];
                int bytes = stream.Read(data, 0, data.Length);//받는 데이터의 바이트배열, 인덱스, 길이
                string responses = Encoding.UTF8.GetString(data, 0, bytes);
                MessageBox.Show(responses);
                if (responses == "회원가입성공")
                {
                    NavigationService.Navigate(new Uri("/Home.xaml", UriKind.Relative));
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Exception: " + e.Message);
            }
        }

        private void btn_join_haki_Click(object sender, RoutedEventArgs e)
        {
            string id = txt_join_ID.Text;
            string pw = txt_join_PW.Text;
            Signup(id, pw);
          /*  MessageBox.Show("회원가입이 완료 되었습니다");
            NavigationService.Navigate(new Uri("/Home.xaml", UriKind.Relative));*/
        }

        private void txt_join_ID_GotFocus(object sender, RoutedEventArgs e)
        {
            txt_join_ID.Clear();
        }

        private void txt_join_PW_GotFocus(object sender, RoutedEventArgs e)
        {
            txt_join_PW.Clear();
        }
    }
}
