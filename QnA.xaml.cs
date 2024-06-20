using System;
using System.Collections.Generic;
using System.Linq;
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
    /// QnA.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class QnA : Page
    {
        public QnA()
        {
            InitializeComponent();
        }

        private void btn_qna1_Click(object sender, RoutedEventArgs e)
        {
            txt_qna_answer.Text = "답변: \"되\"는 공식적인 글이나 문서에서 사용되는 형태로, \"돼\"는 구어체나 일상적인 대화에서 축약된 형태로 사용됩니다. \n예를 들어, \"계약이 체결되었습니다.\" (공식적) vs. \"이미 다 돼 있어요.\" (구어체)";
        }

        private void btn_qna2_Click(object sender, RoutedEventArgs e)
        {
            txt_qna_answer.Text = "답변: \"낫다\"는 병이 낫거나 상태가 개선되는 것을 의미하며, \"낳다\"는 새로운 생명을 태어나게 하거나 결과를 가져오는 것을 의미합니다. \n예를 들어, \"조금씩 상태가 낫습니다.\" vs. \"새끼를 낳다.\"";
        }

        private void btn_qna3_Click(object sender, RoutedEventArgs e)
        {
            txt_qna_answer.Text = "답변: \"금일\"은 '오늘'을 의미하며, 공식적인 문서나 글에서 사용됩니다. \n예를 들어, \"금일 회의 일정이 변경되었습니다.\" \n반면에 \"명일\"은 '내일'을 의미하며, 일반적인 대화나 문서에서 사용됩니다. \n예를 들어, \"명일 아침 일찍 출발할 예정입니다.\"";
        }
    }
}
