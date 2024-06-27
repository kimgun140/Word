using LiveCharts.Wpf.Charts.Base;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
//using System.Runtime.Remoting.Messaging;
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
using System.Windows.Shapes;
//using static EMPCLIENT.question_add;
using Word;

namespace EMPCLIENT
{
    /// <summary>
    /// grade_show.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class grade_show : Window
    {
        List<cc_info> cc_info_List = new List<cc_info>();// 선택된 회원 성적 리스트 
        List<cc_info> CC_list = new List<cc_info>(); // 전체 회원 목록 보여주기 
        NetworkStream stream = Home.clients.GetStream(); //데이터 전송에 사용된 스트림

        public grade_show()
        {
            InitializeComponent();
            //User_list_read();
        }
        class cc_info
        {
            public string CCNUM { get; set; }
            public string ID { get; set; }
            public string USER_WORD { get; set; }
            public string DEFINITION { get; set; }

            public string SCORE { get; set; }

            public string date { get; set; }

        }

        async public void User_list_read() // 전체 학생 리스트 읽어 오기 
        {


            string send_msg;
            string responses = "";

            // 성적조회 플래그 보내기 
            byte[] data;
            data = null;
            data = new byte[256];
            send_msg = "개인";// 시그널
            data = Encoding.UTF8.GetBytes(send_msg);
            stream.Write(data, 0, data.Length);//전송할 데이터의 바이트 배열, 전송을 시작할 배열의 인덱스, 전송할 데이터의 길이.
            Thread.Sleep(100);


            // 모든학생 아이디 여러번
            await Dispatcher.BeginInvoke(new Action(() =>
            {

                while (true)
                {
                    cc_info cc_Info = new cc_info();
                    data = null;
                    data = new byte[256];
                    int bytes = stream.Read(data, 0, data.Length); //받는 데이터의 바이트배열, 인덱스, 길이
                    responses = Encoding.UTF8.GetString(data, 0, bytes);
                    test.Text += responses;
                    //MessageBox.Show(responses);
                    if (responses == "전송완료")
                    {
                        break;
                    }
                    cc_Info.ID = responses;
                    CC_list.Add(cc_Info);

            }

                // 학생목록  리스트뷰에 넣기 
                user_list.ItemsSource = CC_list;
            }));

        }


        async private void Button_Click(object sender, RoutedEventArgs e) // 유저 성적 요청할거임
        {
            //NetworkStream stream = MainPage.client.GetStream(); //데이터 전송에 사용된 스트림

            byte[] data;
            string send_msg;
            string responses;
            // 특정 학생 아이디 보내기 
            data = null;
            data = new byte[256];
            if (user_id.Text != null)
            {
                send_msg = user_id.Text; //  유저어아디 보내기  
                data = Encoding.UTF8.GetBytes(send_msg);
                stream.Write(data, 0, data.Length);//전송할 데이터의 바이트 배열, 전송을 시작할 배열의 인덱스, 전송할 데이터의 길이.
                Thread.Sleep(100);
            }

            // 이거는 여러번 보내는거
            responses = "";
            //while (true) //
            //{
                cc_info cc_Info = new cc_info();
                data = null;
                data = new byte[256];
                int bytes = stream.Read(data, 0, data.Length); //받는 데이터의 바이트배열, 인덱스, 길이
                responses = Encoding.UTF8.GetString(data, 0, bytes);

                //if (responses == "전송종료")
                //{
                //    break;
                //}
                cc_info quest_data = new cc_info();
                quest_data.ID = user_id.Text;
                quest_data.SCORE = responses;
                cc_info_List.Add(quest_data);
                //Dictionary<string, string> dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(responses);
                // 여기가 제이슨이 아니요 
                //if (dictionary != null)
                //{
                //    //MyData quest_data = new MyData();
                //    foreach (var kvp in dictionary)
                //    {
                //        cc_info quest_data = new cc_info();
                //        quest_data.ID = kvp.Key;
                //        quest_data.SCORE = kvp.Value;
                //        cc_info_List.Add(quest_data);
                //    }
                //}
            //}
            score_listview.ItemsSource = cc_info_List;
            score_listview.Items.Refresh();

            //score_listview.ItemsSource = cc_info_List; // 리스트뷰 아이템에 넣기 


        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            User_list_read();
        }
    }



}
