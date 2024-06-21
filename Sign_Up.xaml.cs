using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Xml.Linq;
using Word;
namespace EMPCLIENT
{
    /// <summary>
    /// Sign_Up.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Sign_Up : Page
    {
        public Sign_Up()
        {
            InitializeComponent();
        }

        public void btn_login_Click(object sender, RoutedEventArgs e)
        {

            List<string> aaaa = new List<string>();
            byte[] data;
            //Mainmenu.clients
            //TcpClient client = new TcpClient("10.10.21.111", 5558); //연결객체
            NetworkStream stream = Home.clients.GetStream(); //데이터 전송에 사용된 스트림


            string send_msg;
            //data = Encoding.ASCII.GetBytes(aaaa[0]);
            //stream.Write(data, 0, data.Length);//전송할 데이터의 바이트 배열, 전송을 시작할 배열의 인덱스, 전송할 데이터의 길이.
            // 회원가입 플래그 보내기 
            send_msg = "회원가입요청";
            data = null;
            data = Encoding.UTF8.GetBytes(send_msg);
            stream.Write(data, 0, data.Length);//전송할 데이터의 바이트 배열, 전송을 시작할 배열의 인덱스, 전송할 데이터의 길이.
            Thread.Sleep(100);
            send_msg = "";

            //아이디 보내기
            data = null;
            data = new byte[256];
            send_msg = MyTextBoxid.Text; // id
            data = Encoding.UTF8.GetBytes(send_msg);
            stream.Write(data, 0, data.Length);//전송할 데이터의 바이트 배열, 전송을 시작할 배열의 인덱스, 전송할 데이터의 길이.
            send_msg = "";
            Thread.Sleep(100);
            //비밀번호 보내기
            data = null;
            data = new byte[256];
            send_msg = MyTextBoxpw.Text; // pw
            data = Encoding.UTF8.GetBytes(send_msg);
            stream.Write(data, 0, data.Length);//전송할 데이터의 바이트 배열, 전송을 시작할 배열의 인덱스, 전송할 데이터의 길이.
            send_msg = "";
            Thread.Sleep(100);


            data = null;
            data = new byte[256];
            int bytes = stream.Read(data, 0, data.Length);//받는 데이터의 바이트배열, 인덱스, 길이
            string responses = Encoding.UTF8.GetString(data, 0, bytes);
            MessageBox.Show(responses);
            if (responses == "회원가입성공")
            {
                NavigationService.Navigate(
                                        new Uri("/Login_Page.xaml", UriKind.Relative));
            }
            else if (responses == "중복된아이디")
            {
                MessageBox.Show(responses);
            }
        }
    }
}
