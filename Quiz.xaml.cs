using Newtonsoft.Json;
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
    /// Quiz.xaml에 대한 상호 작용 논리
    /// </summary>
    public class MyData2
    {
        public string word { get; set; }
        public string meaning { get; set; }
    }
    public partial class Quiz : Page
    {
        NetworkStream stream = Home.clients.GetStream();
        byte[] data = new byte[256];
        List<MyData2> myDatas = new List<MyData2>();
        public Quiz()
        {
            data = null;
            data = Encoding.UTF8.GetBytes("문제풀기");
            stream.Write(data, 0, data.Length);

            InitializeComponent();
            txt_question.Text = "";
            Task.Run(() => Read_Quiz());
        }
        public void Read_Quiz()
        {
            int databyte = 1;
            while (databyte != 0)
            {
                byte[] recv_data = new byte[300];
                databyte = stream.Read(recv_data, 0, recv_data.Length);
                string readdata = Encoding.UTF8.GetString(recv_data, 0, databyte);
               /* Dictionary<string, string> dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(readdata);*/

                Dispatcher.BeginInvoke(new Action(() =>
                {
                try
                {
                        Dictionary<string, string> dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(readdata);
              
              
                        /*txt_question.Text += readdata + "\n";*/
                        if (dictionary != null)
                        {
                            foreach (var kvp in dictionary)
                            {
                                MyData2 quest_data = new MyData2();
                                quest_data.word = kvp.Key;
                                quest_data.meaning = kvp.Value;
                                myDatas.Add(quest_data);

                            }
                            listv_quiz.ItemsSource = myDatas;
                            listv_quiz.Items.Refresh();
                        }
                    }
                    catch (Exception ex)
                    {
                        // 로그를 출력하거나 에러 메시지를 표시
                        /*     MessageBox.Show($"Error: {ex.Message}");*/
                    }

                }));
                if (readdata == "데이터전송종료")
                {
                    databyte = 0;
                }
            }
        }
        private void txt_answer_GotFocus(object sender, RoutedEventArgs e)
        {
            txt_answer.Clear();
        }

        private void txt_answer_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string send_message = txt_answer.Text;
                if (!string.IsNullOrEmpty(send_message))
                {
                    data = null;
                    data = Encoding.UTF8.GetBytes(send_message);
                    stream.Write(data, 0, data.Length);
                    /*    txtbox_chat.Text += send_message;*/
                    txt_answer.Clear();
/*                    txt_question.ScrollToEnd();*/
                }
            }
        }

        private void btn_anwer_Click(object sender, RoutedEventArgs e)
        {
            data = null;
            data = Encoding.UTF8.GetBytes("답안제출");
            stream.Write(data, 0, data.Length);
        }

        private void btn_next_Click(object sender, RoutedEventArgs e)
        {
            Read_Quiz();
        }
    }
}
