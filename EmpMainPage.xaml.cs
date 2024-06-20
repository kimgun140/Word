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
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Word;
namespace EMPCLIENT // 이게 내 메인 
{
    /// <summary>
    /// EmpMainPage.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class EmpMainPage : Page
    {

        public EmpMainPage()
        {
            InitializeComponent();
        }

        private void Counseling_Click(object sender, RoutedEventArgs e) // 상담 페이지로 이동  채팅하기 채팅 시작할 때 채팅토큰 보내야함 
        {
            //byte[] data;
            //NetworkStream stream = MainPage.client.GetStream(); //데이터 전송에 사용된 스트림
            //data = null;
            //data = Encoding.UTF8.GetBytes("1:1상담"); // 채팅 요청하기 
            //stream.Write(data, 0, data.Length);//전송할 데 이터의 바이트 배열, 전송을 시작할 배열의 인덱스, 전송할 데이터의 길이.

            //byte[] data1 = new byte[256];
            //int bytes = stream.Read(data1, 0, data1.Length);//받는 데이터의 바이트배열, 인덱스, 길이
            //string responses = Encoding.UTF8.GetString(data, 0, bytes);
            //stream.Flush();
            //if (responses == "1:1상담")
            //{

     
                //NavigationService.Navigate(new Uri("/Counseling.xaml", UriKind.Relative));
                Counseling counseling = new Counseling();
                    counseling.Show();

            //}
        }

        private void ScoreQueryRequest_Click(object sender, RoutedEventArgs e)
        {
            // 성적 확인 창
            grade_show grade_Show = new grade_show(); //성적 확인 윈도우 띄우기 
            grade_Show.Show();
            //Window window = new Window();
            //window.Show();
        }

        private void Exam_Questions_Click(object sender, RoutedEventArgs e)
        {
            // 문제관리 창
            question_add question_Add = new question_add();
            question_Add.Show();

        }

        private void ScoreVisualize_Click(object sender, RoutedEventArgs e)
        {
            score_visualize visualize = new score_visualize();
            visualize.Show();

        }
    }
}
