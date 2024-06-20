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
    /// Main.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Main : Page
    {
        public Main()
        {
            InitializeComponent();
        }

        private void btn_chat_Click(object sender, RoutedEventArgs e)
        {
            Chat Chat = new Chat();
            Chat.Show();
        }

        private void btn_edu_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Edu.xaml", UriKind.Relative));
        }

        private void btn_quiz_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Quiz.xaml", UriKind.Relative));
        }

        private void btn_qna_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/QnA.xaml", UriKind.Relative));
        }

        private void btn_quiz_load_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/History.xaml", UriKind.Relative));
        }
    }
}
