using Newtonsoft.Json;
using System.Net.Sockets;
using System.Text;
using System.Windows.Controls;
using System.Windows;

namespace Word
{
    /// <summary>
    /// Edu.xaml에 대한 상호 작용 논리
    /// </summary>
    public class MyData
    {
        public List<string> words { get; set; } = new List<string>();
        public List<string> means { get; set; } = new List<string>();

    }

    public partial class Edu : Page
    {
        int cnt = 0;
        MyData MyData = new MyData();
        NetworkStream stream = Home.clients.GetStream();
        byte[] data = new byte[256];

        public Edu()
        {
            data = Encoding.UTF8.GetBytes("학습하기");
            stream.Write(data, 0, data.Length);

            InitializeComponent();
            txt_mean.Text = "";
            Task.Run(() => Read_Edu());
            
            
        }

        public async Task Read_Edu()
        {
            int databyte = 0;
            while (true)
            {
                byte[] recv_data = new byte[2000];
                databyte = await stream.ReadAsync(recv_data, 0, recv_data.Length);
                string readdata = Encoding.UTF8.GetString(recv_data, 0, databyte);

                if (readdata == "데이터전송종료")
                {
                    break;
                }

                try
                {
                    Dictionary<string, string> dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(readdata);

                    if (dictionary != null)
                    {
                        foreach (var kvp in dictionary)
                        { 
                            MyData.words.Add(kvp.Key);
                            MyData.means.Add(kvp.Value);
                           
                        }
                        await Dispatcher.BeginInvoke(new Action(async () =>
                        {
                            txt_mean.Text = "";
                            txt_mean.Text += MyData.words[0] + " : " + MyData.means[0] + "\n";

                        }));

                    }
                }
                catch (Exception ex)
                {
                    await Dispatcher.BeginInvoke(new Action(() =>
                    {
                        MessageBox.Show($"Error: {ex.Message}");
                    }));
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (cnt < MyData.words.Count-1)
            {
                cnt++;
                txt_mean.Text = "";
                txt_mean.Text = MyData.words[cnt] + " : " + MyData.means[cnt];
            }
  
        }
    }
}
