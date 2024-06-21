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
                Counseling counseling = new Counseling();
                    counseling.Show();
        }

        private void ScoreQueryRequest_Click(object sender, RoutedEventArgs e)
        {
            // 성적 확인 창
            grade_show grade_Show = new grade_show(); //성적 확인 윈도우 띄우기 
            grade_Show.Show();

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
