using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
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
using static EMPCLIENT.question_add;

namespace Word
{
    /// <summary>
    /// History.xaml에 대한 상호 작용 논리
    /// </summary>
    public class MyData3
    {
        public List<string> datas { get; set; } = new List<string>();
        public string Column1 { get; set; }
        public string Column2 { get; set; }
        public string Column3 { get; set; }
        public string Column4 { get; set; }
    }



    public partial class History : Page
    {
        ListViewItem item;
        MyData3 MyData3 = new MyData3();
        NetworkStream stream = Home.clients.GetStream();
        byte[] data = new byte[256];
        public History()
        {
            data = null;
            data = Encoding.UTF8.GetBytes("문제풀이불러오기");
            stream.Write(data, 0, data.Length);

            InitializeComponent();
            Task.Run(() => Read_History());
        }

        public async Task Read_History()
        {
            while (true)
            {
                int databyte = 0;
                byte[] recv_data = new byte[300];
                databyte = await stream.ReadAsync(recv_data, 0, recv_data.Length);
                string readdata = Encoding.UTF8.GetString(recv_data, 0, databyte);

                if (readdata == "전송완료")
                {
                    break;
                }

                MyData3.datas.Add(readdata);

                if (MyData3.datas.Count == 4)
                {
                    var rowData = new MyData3
                    {
                        Column1 = MyData3.datas[0],
                        Column2 = MyData3.datas[1],
                        Column3 = MyData3.datas[2],
                        Column4 = MyData3.datas[3]
                    };

                    await Dispatcher.BeginInvoke(new Action(() =>
                    {

                        lstv_history.Items.Add(rowData);
                    }));

                    MyData3.datas.Clear();
                }

            }
        }
    }
}


