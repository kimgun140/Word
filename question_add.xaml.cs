using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Runtime.InteropServices;
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
using System.Windows.Shapes;
//using Microsoft.TeamFoundation.Common.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Word;

namespace EMPCLIENT
{
    /// <summary>
    /// question_add.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class question_add : Window
    {
        List<MyData> data = new List<MyData>();

        NetworkStream stream = Home.clients.GetStream(); //데이터 전송에 사용된 스트림

        public question_add()
        {
            InitializeComponent();



            string send_message = "문제관리";
            byte[] data;
            data = null;
            data = Encoding.UTF8.GetBytes(send_message);
            stream.Write(data, 0, data.Length);
            Thread.Sleep(100);
        }


        List<MyData> myDatas = new List<MyData>();
        public class MyData
        {
            public string word { get; set; }
            public string meaning { get; set; }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            // 문제추가 시그널 보내기
            byte[] data;
            string  send_message = "";
            send_message = "추가";
            data = null;
            data = Encoding.UTF8.GetBytes(send_message);
            stream.Write(data, 0, data.Length);
            Thread.Sleep(100);

            send_message = question_add_btn.Text;
            if (!string.IsNullOrEmpty(send_message))
            {
                // 추가할 문자 보내기
                data = null;
                data = Encoding.UTF8.GetBytes(send_message);
                stream.Write(data, 0, data.Length);
                Thread.Sleep(100);
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e) // 미리보기 버튼 
        {

            string send_message;
            send_message = "미리보기";
            byte[] data;
         
            data = null;
            data = Encoding.UTF8.GetBytes(send_message);
            stream.Write(data, 0, data.Length);
            Thread.Sleep(100);


            send_message = question_add_btn.Text; // 추가할 문제 검색어 
            if (!string.IsNullOrEmpty(send_message))
            {
                //  문자 검색어  보내기
                data = null;
                data = Encoding.UTF8.GetBytes(send_message); //검색어 변환
                stream.Write(data, 0, data.Length); // 검색어 보내기 
                Thread.Sleep(100);

            }

            MyData myData = new MyData();
            data = null;
            data = new byte[2000];
            string responses = "";
            int bytes = stream.Read(data, 0, data.Length);
            responses = Encoding.UTF8.GetString(data, 0, bytes);
            if(responses != "터짐")
            {
                //MessageBox.Show("검색어를 정확히 입력해주세요!");

           
            testbox.Text = responses + "\n";


            ////myData = JsonConvert.DeserializeObject<MyData>(responses); // 이것도 되나 ?
            Dictionary<string, string> dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(responses);
            if (dictionary != null)
            {
                //MyData quest_data = new MyData();
                foreach (var kvp in dictionary)
                {
                    MyData quest_data = new MyData();
                    quest_data.word = kvp.Key;
                    quest_data.meaning = kvp.Value;
                    myDatas.Add(quest_data); // 

                }
                question_listview.ItemsSource = myDatas;
                question_listview.Items.Refresh();
            }


            }
            else
            {
                MessageBox.Show("검색어를 정확히 입력해주세요!");
            }
        }


        public void aass()
        {

        }

    }
}
