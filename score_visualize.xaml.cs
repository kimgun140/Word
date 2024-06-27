using LiveCharts;
using LiveCharts.Wpf.Charts.Base;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Word;

namespace EMPCLIENT
{
    /// <summary>
    /// score_visualize.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class score_visualize : Window
    {
        NetworkStream stream = Home.clients.GetStream(); //데이터 전송에 사용된 스트림

        public ChartValues<double> C_Chart { get; set; }
        public string[] XLabel { get; set; }

        public class question_history
        {
            public string ID { get; set; }
            public string SCORE { get; set; }
        }
        List<question_history> question_Histories = new List<question_history>();
        public score_visualize()
        {
            InitializeComponent();
            C_Chart = new ChartValues<double>();
            XLabel = new string[] { };

            Chart1.DataContext = this;




        }
        async public void graph(string User_id) // 실험용 이제 안씀
        {
            //NetworkStream stream = MainPage.client.GetStream();

            // 차트 그리는 요청 메세지
            string send_msg;
            byte[] data;
            data = null;
            data = new byte[256];
            send_msg = "차트";// 시그널
            data = Encoding.UTF8.GetBytes(send_msg);
            stream.Write(data, 0, data.Length);//전송할 데이터의 바이트 배열, 전송을 시작할 배열의 인덱스, 전송할 데이터의 길이.
            Thread.Sleep(100);
            send_msg = "";

            send_msg = User_id;
            data = null;
            data = new byte[256];
            data = Encoding.UTF8.GetBytes(send_msg); // 그래프를 그릴 유저의 성적 요청 위해서 유저아이디 전송 
            stream.Write(data, 0, data.Length);//전송할 데이터의 바이트 배열, 전송을 시작할 배열의 인덱스, 전송할 데이터의 길이.
            Thread.Sleep(100);

            string responses = "";
            await Dispatcher.BeginInvoke(new Action(() =>
            {
                while (true) // 여러번 보내면 
                {
                    data = null;
                    data = new byte[256];
                    int bytes = stream.Read(data, 0, data.Length);//받는 데이터의 바이트배열, 인덱스, 길이
                    responses = Encoding.UTF8.GetString(data, 0, bytes);
                    if (responses != "전송종료")
                    {
                        int responses1 = int.Parse(responses); // 받은 고객의 성적을 
                        C_Chart.Add(responses1);
                    }
                    else if (responses == "전송종료")
                    {
                        break;
                    }
                }
            }

            ));
        }
        //async public void testgraph()
        //{

        //    await Dispatcher.BeginInvoke(new Action(() =>
        //        {
        //            for (int i = 0; i < 100; i++)
        //            {
        //                int testvalue = i;
        //                C_Chart.Add(Convert.ToInt32(testvalue));

        //            }
        //            //C_Chart = null;
        //            //ChartValues<double> value = new ChartValues<double>{1, 2, 3, 4, 5};
        //            //C_Chart.Add(Convert.ToInt32(value));

        //        }));
        //}
      

        async public void charttest(string User_id) // 차트 그리기 
        {
            //NetworkStream stream = MainPage.client.GetStream();

            //Dispatcher.BeginInvoke(() =>
            //{

            //});
            // 차트 그리는 요청 메세지
            string send_msg;
            byte[] data;
            data = null;
            data = new byte[256];
            send_msg = "그래프보기";// 시그널
            data = Encoding.UTF8.GetBytes(send_msg);
            stream.Write(data, 0, data.Length);//전송할 데이터의 바이트 배열, 전송을 시작할 배열의 인덱스, 전송할 데이터의 길이.
            Thread.Sleep(100);
            send_msg = "";


            // 데이터 받기 
            await Dispatcher.BeginInvoke(new Action(() =>
            {
                while (true)
                {
                    data = null;
                    data = new byte[256];
                    int bytes = stream.Read(data, 0, data.Length);//받는 데이터의 바이트배열, 인덱스, 길이
                    string responses = Encoding.UTF8.GetString(data, 0, bytes);
                    testblock.Text += responses + "\n";
                    if (responses == "전송완료")
                    {
                        break;
                    }
                    // 여러명이 오는데 그걸 받기는 할 수 있어 그러면 키값으로 찾아서 각각의 학생을 구분해야함 흐음 흠터레스팅 
                    // 여러명 와서 딕셔너리에 담아주고 내가 보여주고 사람것만 보여주면 되잖아 맞지? 맞아요~ 그러면 키값 비교만해서 내가 찾는 사람의  것만 받으면 되겠네 
                    // 그것만 차트에 넣어주면 굳 굳굳  근데 어케함 ㅋ
                    Dictionary<string, string> dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(responses);
                    // 우선 딕셔너리로 데이터 변환 함 이거
                    Dispatcher.BeginInvoke(new Action(() => // 
                    {
                        if (dictionary != null)
                        {
                            //MyData quest_data = new MyData();
                            foreach (var kvp in dictionary)
                            {
                                question_history quest_data = new question_history();
                                if (kvp.Key == User_id)
                                {  // 키 값이랑 받으면 되겠네 
                                    quest_data.ID = kvp.Key;
                                    quest_data.SCORE = kvp.Value;
                                    C_Chart.Add(int.Parse(kvp.Value));// 차트에 넣기  
                                                                      //cc_info_List.Add(quest_data);
                                    question_Histories.Add(quest_data);// 받아서 리스트에 넣어주고 있음 그러면 여기서 전체 리스트의 리스트뷰에 넣어주기
                                   
                                }

                            }
                            testlistview.ItemsSource = question_Histories; // 
                            testlistview.Items.Refresh();

                        }
                    }));

                }
            }));
        }

        private void Button_Click(object sender, object e)
        {
            //testgraph();
            string user_id = User_id.Text;
            charttest(user_id);
            //User_id = null;

        }
    }
}
